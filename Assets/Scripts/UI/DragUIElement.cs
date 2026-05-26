using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Skritty.Tools.Utilities
{
    public class DragUIElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Vector2 offset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = new Vector2(Mathf.Clamp(eventData.position.x, 0, Screen.width), Mathf.Clamp(eventData.position.y, 0, Screen.height)) - offset;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            offset = eventData.position - (Vector2)transform.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);
            Debug.Log(results.Count);

            LayoutGroup otherLayoutGroup = null;
            Transform otherTransform = null;
            foreach(RaycastResult result in results)
            {
                if (result.gameObject == gameObject) continue;

                if (result.gameObject.TryGetComponent(out IDropEvent dropEvent))
                {
                    dropEvent.OnDrop(transform);
                }

                if (result.gameObject.GetComponent<LayoutGroup>() != null)
                {
                    otherLayoutGroup = result.gameObject.GetComponent<LayoutGroup>();
                }
                else otherTransform = result.gameObject.transform;
            }

            if(transform.parent.GetComponent<LayoutGroup>() != null && otherTransform != null && transform.parent.GetComponent<LayoutGroup>() == otherLayoutGroup)
            {
                // Add this to the layout group in the correct position
                transform.SetSiblingIndex(otherTransform.GetSiblingIndex());
            }

            transform.gameObject.SetActive(false);
            transform.gameObject.SetActive(true);
        }
    }

    public interface IDropEvent
    {
        public void OnDrop(Transform other);
    }
}