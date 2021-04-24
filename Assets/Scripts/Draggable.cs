using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    public void OnDrag(PointerEventData eventData) => UpdatePosition(eventData);

    public void OnBeginDrag(PointerEventData eventData) => UpdatePosition(eventData);

    public void OnEndDrag(PointerEventData eventData) => UpdatePosition(eventData);

    private void UpdatePosition(PointerEventData eventData) {
        var myGameObject = gameObject;
        var position = myGameObject.transform.position;
        Vector3 worldPosition;
        var oldZ = position.z;
        if (eventData.pointerPressRaycast.isValid && eventData.pointerCurrentRaycast.worldPosition != Vector3.zero) {
            worldPosition = eventData.pointerCurrentRaycast.worldPosition;
            worldPosition.z = oldZ;
        }
        else {
            // this works because we use an orthographic camera
            var screenToWorldPoint = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
            worldPosition = screenToWorldPoint;
        }
        
        worldPosition.z = oldZ;
        myGameObject.transform.position = worldPosition;
    }
}
