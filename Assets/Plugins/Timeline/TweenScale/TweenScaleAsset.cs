using System;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    [Serializable]
    public class TweenScaleAsset : PlayableAsset {
        public TweenScaleBehaviour Template;
        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<TweenScaleBehaviour>.Create(graph, Template);
            //playable.SetTraversalMode(PlayableTraversalMode.Passthrough);
            return playable;
        }
    }
}
