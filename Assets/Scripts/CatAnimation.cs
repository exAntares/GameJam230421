using System;
using Cysharp.Threading.Tasks;
using HalfBlind.Audio;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class CatAnimation : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
    [SerializeField] private SkeletonAnimation SkeletonAnimation;
    [SerializeField] private AnimationReferenceAsset _drag;
    [SerializeField] private AnimationReferenceAsset _fall;
    [SerializeField] private AnimationReferenceAsset _idle;
    [SerializeField] private AnimationReferenceAsset _puke;
    [SerializeField] private AnimationReferenceAsset _dragRelease;
    [SerializeField] private CraftingIngredient _seedPrefab;
    [SerializeField] private Vector2 _randomRangeMinMax = Vector2.one * 5;
    [SerializeField] private Vector3 _offset = new Vector3(0.395000011f, -0.622500002f, 0f);
    [SerializeField] private AudioAsset _pukeSound;
    [SerializeField] private AudioAsset _pickedUpSFX;
    
    private float _elapsed;
    private float _randomTime;
    private bool _isDragging;

    private void Awake() => _randomTime = Random.Range(_randomRangeMinMax.x, _randomRangeMinMax.y);

    private void Update() {
        _elapsed += Time.deltaTime;
        if (!_isDragging && _elapsed >= _randomTime) {
            _elapsed %= _randomTime;
            SpawnSeedAsync().Forget();
        }
    }

    private async UniTask SpawnSeedAsync() {
        SkeletonAnimation.state.SetAnimation(0, _puke.Animation, false);
        SkeletonAnimation.state.AddAnimation(0, _idle.Animation, true, _puke.Animation.Duration);
        _pukeSound.PlayClipAtPoint(transform.position);
        await UniTask.Delay(TimeSpan.FromSeconds(_puke.Animation.Duration * 0.5f), ignoreTimeScale: false, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        if (transform != null) {
            var transform1 = transform;
            var randomPos = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f), 0);
            Instantiate(_seedPrefab, transform1.position + _offset + randomPos, transform1.rotation);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        _isDragging = true;
        _pickedUpSFX.PlayClipAtPoint(transform.position);
        SkeletonAnimation.state.SetAnimation(0, _drag.Animation, true);
    }

    public void OnEndDrag(PointerEventData eventData) {
        _isDragging = false;
        SkeletonAnimation.state.SetAnimation(0, _dragRelease.Animation, false);
        SkeletonAnimation.state.AddAnimation(0, _fall.Animation, false, _dragRelease.Animation.Duration);
        SkeletonAnimation.state.AddAnimation(0, _idle.Animation, true, _fall.Animation.Duration);
    }
}
