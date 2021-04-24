using System;
using UnityEngine;

namespace HalfBlind.Audio {
    [Serializable]
    public class SequenceProvider : IAudioAssetVariationsProvider {
        private int _index = -1;
        public AudioClip GetNext(AudioClip[] clips) {
            _index = (_index + 1) % clips.Length;
            var result = clips[_index];
            return result;
        }
    }
}
