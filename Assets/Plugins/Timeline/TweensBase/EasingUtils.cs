using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

namespace HalfBlind.Timeline {
    public static class EasingUtils {
        public static Keyframe[] GetKeysForEasing(Ease templateEasing) {
            var keys = new List<Keyframe>();
            var count = 100;
            if (templateEasing != Ease.INTERNAL_Custom) {
                for (int i = 0; i < count; i++) {
                    var t = EaseManager.Evaluate(templateEasing, null, i, count,
                        DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
                    keys.Add(new Keyframe(i, t));
                }
            }

            return keys.ToArray();
        }
        
        public static float Evaluate(Ease easing, AnimationCurve customCurve, float time, float duration) {
            float CustomEase(float lTime, float lDuration, float unused, float unused2) {
                return customCurve.Evaluate(lTime / lDuration);
            }
            
            return EaseManager.Evaluate(easing, CustomEase, time, duration, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }
    }
}
