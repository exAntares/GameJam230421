using System;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    public abstract class TweenMixerBehaviour<T, TResult> : PlayableBehaviour where T : TweenBehaviour, new() {
        protected Transform _targetTransform;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            _targetTransform = playerData as Transform;
            if (_targetTransform == null) {
                return;
            }
            
            var lastKnown = GetInitialValue();
            var rootPlayable = playable.GetGraph().GetRootPlayable(0);
            var rootTime = rootPlayable.GetTime();
            
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++) {
                var inputPlayable = (ScriptPlayable<T>) playable.GetInput(i);
                var tweenScaleBehaviour = inputPlayable.GetBehaviour();
                if (tweenScaleBehaviour.EndTime < rootTime || Math.Abs(tweenScaleBehaviour.EndTime - rootTime) < 0.001) {
                    lastKnown = Evaluate(tweenScaleBehaviour, lastKnown, 1, 1);
                }
            }
            

            for (int i = 0; i < inputCount; i++) {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0) {
                    var inputPlayable = (ScriptPlayable<T>) playable.GetInput(i);
                    var tweenScaleBehaviour = inputPlayable.GetBehaviour();
                    var duration = (float) inputPlayable.GetDuration();
                    var time = (float) inputPlayable.GetTime();
                    lastKnown = Evaluate(tweenScaleBehaviour, lastKnown, time, duration);
                }
            }
            SetValue(lastKnown);
        }

        public abstract void SetValue(TResult lastKnown);
        public abstract TResult GetInitialValue();
        public abstract TResult Evaluate(T tween, TResult initial, float time, float duration);
    }
}
