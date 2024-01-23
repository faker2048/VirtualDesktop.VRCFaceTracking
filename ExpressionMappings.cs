using VRCFaceTracking.Core.Params.Expressions;

namespace VirtualDesktop.FaceTracking
{
    public static class ExpressionMappings
    {
        public static readonly Expressions[] Map;

        static ExpressionMappings()
        {
            const int maxN = (int)UnifiedExpressions.Max + 5;
            Map = new Expressions[maxN];

            for (var i = 0; i < maxN; i++)
            {
                Map[i] = Expressions.None;
            }
            
            Map[(int)UnifiedExpressions.EyeWideLeft] = Expressions.UpperLidRaiserL;
            Map[(int)UnifiedExpressions.EyeWideRight] = Expressions.UpperLidRaiserR;
            Map[(int)UnifiedExpressions.EyeSquintLeft] = Expressions.LidTightenerL;
            Map[(int)UnifiedExpressions.EyeSquintRight] = Expressions.LidTightenerR;
            Map[(int)UnifiedExpressions.BrowInnerUpLeft] = Expressions.InnerBrowRaiserL;
            Map[(int)UnifiedExpressions.BrowInnerUpRight] = Expressions.InnerBrowRaiserR;
            Map[(int)UnifiedExpressions.BrowOuterUpLeft] = Expressions.OuterBrowRaiserL;
            Map[(int)UnifiedExpressions.BrowOuterUpRight] = Expressions.OuterBrowRaiserR;
            Map[(int)UnifiedExpressions.BrowPinchLeft] = Expressions.BrowLowererL;
            Map[(int)UnifiedExpressions.BrowLowererLeft] = Expressions.BrowLowererL;
            Map[(int)UnifiedExpressions.BrowPinchRight] = Expressions.BrowLowererR;
            Map[(int)UnifiedExpressions.BrowLowererRight] = Expressions.BrowLowererR;
            Map[(int)UnifiedExpressions.JawOpen] = Expressions.JawDrop;
            Map[(int)UnifiedExpressions.JawLeft] = Expressions.JawSidewaysLeft;
            Map[(int)UnifiedExpressions.JawRight] = Expressions.JawSidewaysRight;
            Map[(int)UnifiedExpressions.JawForward] = Expressions.JawThrust;
            Map[(int)UnifiedExpressions.MouthClosed] = Expressions.LipsToward;
            Map[(int)UnifiedExpressions.MouthUpperLeft] = Expressions.MouthLeft;
            Map[(int)UnifiedExpressions.MouthLowerLeft] = Expressions.MouthLeft;
            Map[(int)UnifiedExpressions.MouthUpperRight] = Expressions.MouthRight;
            Map[(int)UnifiedExpressions.MouthLowerRight] = Expressions.MouthRight;
            Map[(int)UnifiedExpressions.MouthCornerPullLeft] = Expressions.LipCornerPullerL;
            Map[(int)UnifiedExpressions.MouthCornerSlantLeft] =
                Expressions.LipCornerPullerL; // Slant (Sharp Corner Raiser) is baked into Corner Puller.
            Map[(int)UnifiedExpressions.MouthCornerPullRight] = Expressions.LipCornerPullerR;
            Map[(int)UnifiedExpressions.MouthCornerSlantRight] =
                Expressions.LipCornerPullerR; // Slant (Sharp Corner Raiser) is baked into Corner Puller.
            Map[(int)UnifiedExpressions.MouthFrownLeft] = Expressions.LipCornerDepressorL;
            Map[(int)UnifiedExpressions.MouthFrownRight] = Expressions.LipCornerDepressorR;
            Map[(int)UnifiedExpressions.MouthLowerDownLeft] = Expressions.LowerLipDepressorL;
            Map[(int)UnifiedExpressions.MouthLowerDownRight] = Expressions.LowerLipDepressorR;
            Map[(int)UnifiedExpressions.MouthRaiserUpper] = Expressions.ChinRaiserT;
            Map[(int)UnifiedExpressions.MouthRaiserLower] = Expressions.ChinRaiserB;
            Map[(int)UnifiedExpressions.MouthDimpleLeft] = Expressions.DimplerL;
            Map[(int)UnifiedExpressions.MouthDimpleRight] = Expressions.DimplerR;
            Map[(int)UnifiedExpressions.MouthTightenerLeft] = Expressions.LipTightenerL;
            Map[(int)UnifiedExpressions.MouthTightenerRight] = Expressions.LipTightenerR;
            Map[(int)UnifiedExpressions.MouthPressLeft] = Expressions.LipPressorL;
            Map[(int)UnifiedExpressions.MouthPressRight] = Expressions.LipPressorR;
            Map[(int)UnifiedExpressions.MouthStretchLeft] = Expressions.LipStretcherL;
            Map[(int)UnifiedExpressions.MouthStretchRight] = Expressions.LipStretcherR;
            Map[(int)UnifiedExpressions.LipPuckerUpperRight] = Expressions.LipPuckerR;
            Map[(int)UnifiedExpressions.LipPuckerLowerRight] = Expressions.LipPuckerR;
            Map[(int)UnifiedExpressions.LipPuckerUpperLeft] = Expressions.LipPuckerL;
            Map[(int)UnifiedExpressions.LipPuckerLowerLeft] = Expressions.LipPuckerL;
            Map[(int)UnifiedExpressions.LipFunnelUpperLeft] = Expressions.LipFunnelerLt;
            Map[(int)UnifiedExpressions.LipFunnelUpperRight] = Expressions.LipFunnelerRt;
            Map[(int)UnifiedExpressions.LipFunnelLowerLeft] = Expressions.LipFunnelerLb;
            Map[(int)UnifiedExpressions.LipFunnelLowerRight] = Expressions.LipFunnelerRb;
            Map[(int)UnifiedExpressions.LipSuckLowerLeft] = Expressions.LipSuckLb;
            Map[(int)UnifiedExpressions.LipSuckLowerRight] = Expressions.LipSuckRb;
            Map[(int)UnifiedExpressions.CheekPuffLeft] = Expressions.CheekPuffL;
            Map[(int)UnifiedExpressions.CheekPuffRight] = Expressions.CheekPuffR;
            Map[(int)UnifiedExpressions.CheekSuckLeft] = Expressions.CheekSuckL;
            Map[(int)UnifiedExpressions.CheekSuckRight] = Expressions.CheekSuckR;
            Map[(int)UnifiedExpressions.CheekSquintLeft] = Expressions.CheekRaiserL;
            Map[(int)UnifiedExpressions.CheekSquintRight] = Expressions.CheekRaiserR;
            Map[(int)UnifiedExpressions.NoseSneerLeft] = Expressions.NoseWrinklerL;
            Map[(int)UnifiedExpressions.NoseSneerRight] = Expressions.NoseWrinklerR;
        }
    }
}