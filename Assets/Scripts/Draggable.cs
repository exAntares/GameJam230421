using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler {
    public void OnDrag(PointerEventData eventData) {
        if (eventData.pointerCurrentRaycast.isValid) {
            var oldZ = gameObject.transform.position.z;
            var worldPosition = eventData.pointerCurrentRaycast.worldPosition;

            worldPosition.z = oldZ;
            gameObject.transform.position = worldPosition;
        }
    }
}
