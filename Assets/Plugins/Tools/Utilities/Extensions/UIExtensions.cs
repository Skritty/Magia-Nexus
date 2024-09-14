// Created by: Bill D.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Skritty.Tools.Utilities
{
    namespace Devhouse.Extensions
    {
        public static class UIExtensions
        {
            public static List<GameObject> RaycastAllTargetObjects(this GraphicRaycaster raycaster, Vector3 startPos, string tagFilter = "")
            {
                PointerEventData data = new PointerEventData(EventSystem.current);
                data.position = startPos;
                List<RaycastResult> results = new List<RaycastResult>();

                raycaster.Raycast(data, results);
                List<GameObject> hits = new List<GameObject>();
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag(tagFilter) || string.IsNullOrEmpty(tagFilter))
                        hits.Add(result.gameObject);
                }
                return hits;
            }

            public static GameObject RaycastOneTargetObject(this GraphicRaycaster raycaster, Vector3 startPos, string tagFilter = "")
            {
                PointerEventData data = new PointerEventData(EventSystem.current);
                data.position = startPos;
                List<RaycastResult> results = new List<RaycastResult>();

                raycaster.Raycast(data, results);
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag(tagFilter) || string.IsNullOrEmpty(tagFilter))
                        return result.gameObject;
                }
                return null;
            }
        }
    }
}