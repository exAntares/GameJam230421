using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    [Serializable]
    public class TweenScaleBehaviour : TweenBehaviour {
        public bool IsFrom = false;
        public Vector3 TargetScale;
    }
}
