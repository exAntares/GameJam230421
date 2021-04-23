using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    [Serializable]
    public class TweenMoveBehaviour : TweenBehaviour {
        public bool IsFrom = false;
        public bool IsRelative = false;
        public Vector3 TargetValue;
    }
}
