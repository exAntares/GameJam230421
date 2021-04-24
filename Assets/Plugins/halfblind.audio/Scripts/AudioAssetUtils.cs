using UnityEngine;

namespace HalfBlind.Audio {
    public static class AudioAssetUtils {
        public static void PlayClipAtPoint(this AudioAsset asset, Vector3 position) {
            var gameObject = new GameObject("One shot audio");
            var audioSource = (AudioSource) gameObject.AddComponent(typeof (AudioSource));
            gameObject.transform.position = position;
            asset.ApplyToAudioSource(audioSource);
            audioSource.Play();
            Object.Destroy(gameObject, audioSource.clip.length * ((double) Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale));
        }
    }
}
