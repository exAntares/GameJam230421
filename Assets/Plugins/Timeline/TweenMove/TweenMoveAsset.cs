using System;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    [Serializable]
    public class TweenMoveAsset : PlayableAsset {
        public TweenMoveBehaviour Template;
        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<TweenMoveBehaviour>.Create(graph, Template);
            //playable.SetTraversalMode(PlayableTraversalMode.Passthrough);
            return playable;
        }
    }
}
