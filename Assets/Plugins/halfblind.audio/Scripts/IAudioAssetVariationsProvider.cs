using UnityEngine;

namespace HalfBlind.Audio {
    public interface IAudioAssetVariationsProvider {
        AudioClip GetNext(AudioClip[] clips);
    }
}
