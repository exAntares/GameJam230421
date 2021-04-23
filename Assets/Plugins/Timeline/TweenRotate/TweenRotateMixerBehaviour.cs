using System;
using System.Resources;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    [Serializable]
    public class TweenRotateMixerBehaviour : TweenMixerBehaviour<TweenRotateBehaviour, Vector3> {
        private Quaternion? _initialRotation;

        public override void OnGraphStop(Playable playable) {
            base.OnGraphStop(playable);
            if (_targetTransform && _initialRotation.HasValue) {
                _targetTransform.rotation = _initialRotation.Value;
            }
        }

        public override void SetValue(Vector3 lastKnown) {
            _targetTransform.rotation = Quaternion.Euler(lastKnown);
        }

        public override Vector3 GetInitialValue() {
            if (_initialRotation == null) {
                _initialRotation = _targetTransform.rotation;
            }
            return _initialRotation.Value.eulerAngles;
        }

        public override Vector3 Evaluate(TweenRotateBehaviour tween, Vector3 initial, float time, float duration) {
            var t = EasingUtils.Evaluate(tween.Easing, tween.CustomCurve, time, duration);
            var target = tween.TargetRotation;
            if (tween.IsFrom) {
                target = initial;
                initial = tween.TargetRotation;
            }
            return initial * (1 - t) + target * t;
        }
    }
}
