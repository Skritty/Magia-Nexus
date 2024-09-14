//Created by: Julian Noel
using Skritty.Tools.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Skritty.Tools.UI
{
    public enum RectWorldCorners
    {
        BottomLeft = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3
    }

    [ExecuteAlways]
    /// <summary>
    /// Little script that sets its size to match the rectangular bounds of a set of rectTransforms.
    /// </summary>
    public class EncapsulateRects : MonoBehaviour
    {
        public Transform referenceFrame;
        public List<RectTransform> encapsulatedElements;
        public bool updateAtRuntime = true;

        protected RectTransform rectTransform;

        public Vector3 RawWorldPosition { get; private set; }

        public virtual void OnValidate()
        {
            Initialize();
        }

        public virtual void Start()
        {
            Initialize();
        }

        public virtual void Update()
        {
            //TODO: find an event driven approach
            Encapsulate();
        }

        [Button, HideInEditorMode]
        public virtual void Encapsulate()
        {
            if(encapsulatedElements != null && encapsulatedElements.Count > 0)
            {
                //Initialize at center for consistency and simplicity.
                rectTransform.pivot = Vector2.one * 0.5f;
                rectTransform.anchorMax = Vector2.one * 0.5f;
                rectTransform.anchorMin = Vector2.one * 0.5f;

                //Initialize corner data array clockwise for min-max operations
                Vector3[] worldCorners = new Vector3[4];
                encapsulatedElements[0].GetWorldCorners(worldCorners); //counter-clockwise from from bottom-left
                Vector2 encapCornerWorldBottomLeft = worldCorners[(int)RectWorldCorners.BottomLeft];
                Vector2 encapCornerWorldTopRight = worldCorners[(int)RectWorldCorners.TopRight];

                //Find encapsulating world corner bounds
                foreach(RectTransform rt in encapsulatedElements)
                {
                    rt.GetWorldCorners(worldCorners); //counter-clockwise from from bottom-left

                    encapCornerWorldBottomLeft = Vector2.Min(encapCornerWorldBottomLeft, worldCorners[(int)RectWorldCorners.BottomLeft]);
                    encapCornerWorldTopRight = Vector2.Max(encapCornerWorldTopRight, worldCorners[(int)RectWorldCorners.TopRight]);
                }

                //Apply updates using the world bounds
                if(!Application.isPlaying || updateAtRuntime)
                {
                    //Position is relative to the reference frame for ease of use. Null ref frame is 
                    //output position data in reference to the refrence frame
                    RawWorldPosition = VectorExtensions.Average(encapCornerWorldTopRight, encapCornerWorldBottomLeft);
                    
                    rectTransform.position = (referenceFrame != null) ?
                        (RawWorldPosition - referenceFrame.position) : RawWorldPosition;

                    //Scale is untouched, we use size, which is with respect to this rect's "neighbor" space (local space of parent).
                    //Since we're finding our OWN new local position, not a position local TO us. Hence parent.
                    //If we're at root, this is equivalent to world space.
                    rectTransform.sizeDelta = rectTransform.parent != null ? 
                        rectTransform.parent.InverseTransformVector(encapCornerWorldTopRight - encapCornerWorldBottomLeft)
                        : encapCornerWorldTopRight - encapCornerWorldBottomLeft;
                }
            }
        }

        protected virtual void Initialize()
        {
            rectTransform = transform as RectTransform;
        }
    }
}
