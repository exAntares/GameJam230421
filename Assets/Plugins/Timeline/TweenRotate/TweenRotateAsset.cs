using System;
using UnityEngine;
using UnityEngine.Playables;

namespace HalfBlind.Timeline {
    [Serializable]
    public class TweenRotateAsset : PlayableAsset {
        public TweenRotateBehaviour Template;
        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<TweenRotateBehaviour>.Create(graph, Template);
            return playable;
        }
    }
}
