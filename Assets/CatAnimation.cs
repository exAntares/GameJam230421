using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatAnimation : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
    [SerializeField] private SkeletonAnimation SkeletonAnimation;
    [SerializeField] private AnimationReferenceAsset _drag;
    [SerializeField] private AnimationReferenceAsset _fall;
    [SerializeField] private AnimationReferenceAsset _idle;
    
    public void OnBeginDrag(PointerEventData eventData) {
        SkeletonAnimation.state.SetAnimation(0, _drag.Animation, true);
    }

    public void OnEndDrag(PointerEventData eventData) {
        SkeletonAnimation.state.SetAnimation(0, _fall.Animation, false);
        SkeletonAnimation.state.AddAnimation(0, _idle.Animation, true, _fall.Animation.Duration);
    }
}
