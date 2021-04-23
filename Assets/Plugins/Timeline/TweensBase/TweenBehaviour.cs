using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    [Serializable]
    public class TweenBehaviour : PlayableBehaviour {
        [ReadOnly] public double EndTime;
        public Ease Easing = Ease.OutExpo;
        public AnimationCurve VISUAL_EASING;
        public AnimationCurve CustomCurve;
    }
}
