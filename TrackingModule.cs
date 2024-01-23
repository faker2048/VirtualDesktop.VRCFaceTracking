using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using VRCFaceTracking;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;

namespace VirtualDesktop.FaceTracking
{
    public unsafe class TrackingModule : ExtTrackingModule
    {
        #region Constants
        private const string FaceStateMapName = "VirtualDesktop.FaceState";
        private const string FaceStateEventName = "VirtualDesktop.FaceStateEvent";
        #endregion

        #region Fields
        private MemoryMappedFile _mappedFile;
        private MemoryMappedViewAccessor _mappedView;
        private FaceState* _faceState;
        private EventWaitHandle _faceStateEvent;
        private bool? _isTracking = null;
        #endregion

        #region Properties
        private bool? IsTracking
        {
            get { return _isTracking; }
            set
            {
                if (value != _isTracking)
                {
                    _isTracking = value;
                    if ((bool)value)
                    {
                        Logger.LogInformation("[VirtualDesktop] Tracking is now active!");
                    }
                    else
                    {
                        Logger.LogWarning("[VirtualDesktop] Tracking is not active. Make sure you are connected to your computer, a VR game or SteamVR is launched and face/eye tracking is enabled in the Streaming tab.");
                    }
                }
            }
        }
        #endregion

        #region Overrides
        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            ModuleInformation.Name = "Virtual Desktop";

            var stream = GetType().Assembly.GetManifestResourceStream("VirtualDesktop.FaceTracking.Resources.Logo256.png");
            if (stream != null)
            {
                ModuleInformation.StaticImages = new List<Stream>() 
                { 
                    stream
                };
            }

            try
            {
                var size = Marshal.SizeOf<FaceState>();
                _mappedFile = MemoryMappedFile.OpenExisting(FaceStateMapName, MemoryMappedFileRights.ReadWrite);
                _mappedView = _mappedFile.CreateViewAccessor(0, size);

                byte* ptr = null;
                _mappedView.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                _faceState = (FaceState*)ptr;

                _faceStateEvent = EventWaitHandle.OpenExisting(FaceStateEventName);
            }
            catch
            {
                Logger.LogError("[VirtualDesktop] Failed to open MemoryMappedFile. Make sure the Virtual Desktop Streamer (v1.29 or later) is running.");
                return (false, false);
            }

            return (true, true);
        }

        public override void Update()
        {
            if (Status == ModuleState.Active)
            {
                if (_faceStateEvent.WaitOne(50))
                {
                    UpdateTracking();
                }
                else
                {
                    var faceState = _faceState;
                    IsTracking = faceState != null && (faceState->LeftEyeIsValid || faceState->RightEyeIsValid || faceState->IsEyeFollowingBlendshapesValid || faceState->FaceIsValid);
                }
            }
            else
            {
                Thread.Sleep(10);
            }
        }

        public override void Teardown()
        {
            if (_faceState != null)
            {
                _faceState = null;
                if (_mappedView != null)
                {
                    _mappedView.Dispose();
                    _mappedView = null;
                }
                if (_mappedFile != null)
                {
                    _mappedFile.Dispose();
                    _mappedFile = null;
                }
            }
            if (_faceStateEvent != null)
            {
                _faceStateEvent.Dispose();
                _faceStateEvent = null;
            }
            _isTracking = null;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Credit https://github.com/regzo2/VRCFaceTracking-QuestProOpenXR for calculations on converting from OpenXR weigths to VRCFT shapes
        /// </summary>
        private void UpdateTracking()
        {
            var isTracking = false;

            var faceState = _faceState;
            if (faceState != null)
            {
                var expressions = faceState->ExpressionWeights;

                if (faceState->LeftEyeIsValid || faceState->RightEyeIsValid)
                {                    
                    var leftEyePose = faceState->LeftEyePose;
                    var rightEyePose = faceState->RightEyePose;
                    UpdateEyeData(UnifiedTracking.Data.Eye, expressions, leftEyePose.Orientation, rightEyePose.Orientation);
                    isTracking = true;
                }

                if (faceState->IsEyeFollowingBlendshapesValid || faceState->FaceIsValid)
                {
                    UpdateEyeAndMouthExpressions(UnifiedTracking.Data.Shapes, expressions);
                    isTracking = true;
                }
            }

            IsTracking = isTracking;
        }
        
        private const float EmaFactor = 0.5f;
        private float[] _ema = new float[1000];
        
        private float Ema(int key, float newValue)
        {

            _ema[key] = _ema[key] * EmaFactor + newValue * (1 - EmaFactor);
            
            return _ema[key];
        }

        private void UpdateEyeData(UnifiedEyeData eye, float* expressions, Quaternion orientationL, Quaternion orientationR)
        {
            // Eye Openness parsing
            var leftEyeExpression = expressions[(int)Expressions.EyesClosedL] + expressions[(int)Expressions.EyesClosedL] * expressions[(int)Expressions.LidTightenerL];
            var rightEyeExpression = expressions[(int)Expressions.EyesClosedR] + expressions[(int)Expressions.EyesClosedR] * expressions[(int)Expressions.LidTightenerR];
            var emaKeyBegin = 900;
            eye.Left.Openness = Ema(++emaKeyBegin ,1.0f - Math.Clamp(leftEyeExpression, 0, 1));
            eye.Right.Openness = Ema(++emaKeyBegin,1.0f - Math.Clamp(rightEyeExpression, 0, 1));
            

            // Eye Gaze parsing
            double qx = orientationL.X;
            double qy = orientationL.Y;
            double qz = orientationL.Z;
            double qw = orientationL.W;

            var yaw = Math.Atan2(2.0 * (qy * qz + qw * qx), qw * qw - qx * qx - qy * qy + qz * qz);
            var pitch = Math.Asin(-2.0 * (qx * qz - qw * qy));

            var pitchL = (180.0 / Math.PI) * pitch; // from radians
            var yawL = (180.0 / Math.PI) * yaw;

            qx = orientationR.X;
            qy = orientationR.Y;
            qz = orientationR.Z;
            qw = orientationR.W;
            yaw = Math.Atan2(2.0 * (qy * qz + qw * qx), qw * qw - qx * qx - qy * qy + qz * qz);
            pitch = Math.Asin(-2.0 * (qx * qz - qw * qy));

            var pitchR = (180.0 / Math.PI) * pitch; // from radians
            var yawR = (180.0 / Math.PI) * yaw;

            // Eye Data to UnifiedEye
            var radianConst = 0.0174533f;

            var pitchRmod = (float)(Math.Abs(pitchR) + 4f * Math.Pow(Math.Abs(pitchR) / 30f, 30f)); // curves the tail end to better accomodate actual eye pos.
            var pitchLmod = (float)(Math.Abs(pitchL) + 4f * Math.Pow(Math.Abs(pitchL) / 30f, 30f));
            var yawRmod = (float)(Math.Abs(yawR) + 6f * Math.Pow(Math.Abs(yawR) / 27f, 18f)); // curves the tail end to better accomodate actual eye pos.
            var yawLmod = (float)(Math.Abs(yawL) + 6f * Math.Pow(Math.Abs(yawL) / 27f, 18f));
            
            Vector2 CalculateGaze(float pitch, float pitchMod, float yaw, float yawMod, float radianConst)
            {
                float pitchValue = pitch < 0 ? pitchMod * radianConst : -pitchMod * radianConst;
                float yawValue = yaw < 0 ? -yawMod * radianConst : yaw * radianConst;
                return new Vector2(pitchValue, yawValue);
            }
            
            eye.Right.Gaze = CalculateGaze((float)pitchR, pitchRmod, (float)yawR, yawRmod, radianConst);
            eye.Left.Gaze = CalculateGaze((float)pitchL, pitchLmod, (float)yawL, yawLmod, radianConst);
            eye.Right.Gaze.x = Ema(++emaKeyBegin, eye.Right.Gaze.x);
            eye.Right.Gaze.y = Ema(++emaKeyBegin, eye.Right.Gaze.y);
            eye.Left.Gaze.x = Ema(++emaKeyBegin, eye.Left.Gaze.x);
            eye.Left.Gaze.y = Ema(++emaKeyBegin, eye.Left.Gaze.y);
            

            // Eye dilation code, automated process maybe?
            eye.Left.PupilDiameter_MM = 5f;
            eye.Right.PupilDiameter_MM = 5f;

            // Force the normalization values of Dilation to fit avg. pupil values.
            eye._minDilation = 0;
            eye._maxDilation = 10;
        }
        
        private  readonly Expressions[] _expressionMap = ExpressionMappings.Map;

        private void UpdateEyeAndMouthExpressions(UnifiedExpressionShape[] unifiedExpressions, float* expressions)
        {
            for (var i = 0; i < (int)UnifiedExpressions.Max; i++)
            {
                unifiedExpressions[i].Weight = Ema(i, expressions[(int)_expressionMap[i]]);
            }
            
            // Special case for eye wide, since it's a combination of two expressions.
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpLeft].Weight = Math.Max(0, expressions[(int)Expressions.UpperLipRaiserL] - expressions[(int)Expressions.NoseWrinklerL]); // Workaround for upper lip up wierd tracking quirk.
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperDeepenLeft].Weight = Math.Max(0, expressions[(int)Expressions.UpperLipRaiserL] - expressions[(int)Expressions.NoseWrinklerL]); // Workaround for upper lip up wierd tracking quirk.
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpRight].Weight = Math.Max(0, expressions[(int)Expressions.UpperLipRaiserR] - expressions[(int)Expressions.NoseWrinklerR]); // Workaround for upper lip up wierd tracking quirk.
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperDeepenRight].Weight = Math.Max(0, expressions[(int)Expressions.UpperLipRaiserR] - expressions[(int)Expressions.NoseWrinklerR]); // Workaround for upper lip up wierd tracking quirk.
            // Apply EMA to the eye wide expressions.
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpLeft].Weight = Ema((int)UnifiedExpressions.MouthUpperUpLeft, unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpLeft].Weight);
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperDeepenLeft].Weight = Ema((int)UnifiedExpressions.MouthUpperDeepenLeft, unifiedExpressions[(int)UnifiedExpressions.MouthUpperDeepenLeft].Weight);
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpRight].Weight = Ema((int)UnifiedExpressions.MouthUpperUpRight, unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpRight].Weight);
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperDeepenRight].Weight = Ema((int)UnifiedExpressions.MouthUpperDeepenRight, unifiedExpressions[(int)UnifiedExpressions.MouthUpperDeepenRight].Weight);


            unifiedExpressions[(int)UnifiedExpressions.LipSuckUpperLeft].Weight = Math.Min(1f - (float)Math.Pow(expressions[(int)Expressions.UpperLipRaiserL], 1f / 6f), expressions[(int)Expressions.LipSuckLt]);
            unifiedExpressions[(int)UnifiedExpressions.LipSuckUpperRight].Weight = Math.Min(1f - (float)Math.Pow(expressions[(int)Expressions.UpperLipRaiserR], 1f / 6f), expressions[(int)Expressions.LipSuckRt]);
            // Apply EMA to the lip suck expressions.
            unifiedExpressions[(int)UnifiedExpressions.LipSuckUpperLeft].Weight = Ema((int)UnifiedExpressions.LipSuckUpperLeft, unifiedExpressions[(int)UnifiedExpressions.LipSuckUpperLeft].Weight);
            unifiedExpressions[(int)UnifiedExpressions.LipSuckUpperRight].Weight = Ema((int)UnifiedExpressions.LipSuckUpperRight, unifiedExpressions[(int)UnifiedExpressions.LipSuckUpperRight].Weight);


            unifiedExpressions[(int)UnifiedExpressions.TongueOut].Weight = 0f;
        }
        #endregion
    }
}