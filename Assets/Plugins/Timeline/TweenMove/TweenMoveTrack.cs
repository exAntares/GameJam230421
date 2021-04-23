using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HalfBlind.Timeline {
    [TrackClipType(typeof(TweenMoveAsset))]
    [TrackBindingType(typeof(Transform))]
    [TrackColor(0,0,1)]
    public class TweenMoveTrack : TrackAsset {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
            foreach (var clip in GetClips()) {
                var myAsset = clip.asset as TweenMoveAsset;
                if (myAsset != null) {
                    myAsset.Template.EndTime = clip.end;
                    myAsset.Template.VISUAL_EASING = new AnimationCurve(EasingUtils.GetKeysForEasing(myAsset.Template.Easing));
                }
            }

            var scriptPlayable = ScriptPlayable<TweenMoveMixerBehaviour>.Create(graph, inputCount);
            return scriptPlayable;
        }
    }
}
