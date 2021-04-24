using UnityEngine;

namespace HalfBlind.Audio {
    public class AudioAssetToSource : MonoBehaviour {
        [SerializeField] private AudioAsset _asset;
        [SerializeField] private AudioSource _source;
        [SerializeField] private bool _playOnEnabled;

        private void OnEnable() {
            if (_playOnEnabled) {
                Play();
            }
        }

        private void Reset() => _source = GetComponentInChildren<AudioSource>();

        public void Play() {
            _asset.ApplyToAudioSource(_source);
            _source.Play();
        }
    }
}
