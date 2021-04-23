using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


public class Draggable : MonoBehaviour, IDragHandler
{
    
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.isValid)
        {
            var oldZ = gameObject.transform.position.z;
            var worldPosition = eventData.pointerCurrentRaycast.worldPosition;

            worldPosition.z = oldZ;
            gameObject.transform.position = worldPosition;
            
            
            //Debug.Log($"Position: {eventData.position}");
            //Debug.Log($"PressPosition: {eventData.pressPosition}");
            Debug.Log($"PressPosition: {worldPosition}");
        }
    }
}
