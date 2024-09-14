using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// Debug tool for quickly drawing gizmos into Unity without having to add OnDrawGizmos to a MonoBehavior
    /// Intended for quick, temporary debug drawing, and not for any long-term or permanent gizmos
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class DebugGizmoTool : Singleton<DebugGizmoTool>
    {
        
        public enum Shape { Sphere, WireSphere, Cube, WireCube }
        [System.Serializable]
        public class GizmoInfo
        {
            public Shape shape;
            public Vector3 position;
            public Vector3 size;
            public Color32 color;

            public GizmoInfo(Shape shape, Vector3 position, Vector3 size, Color32 color)
            {
                this.shape = shape;
                this.position = position;
                this.size = size;
                this.color = color;
            }
        }

        // ---------

        [SerializeField]
        private bool shouldDrawGizmos = true;
        [SerializeField, TableList]
        private List<GizmoInfo> gizmos;
        [SerializeField, TableList]
        private List<GizmoInfo> tempGizmos;

        // ---------------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            if(!shouldDrawGizmos)
                return;

            DrawGizmosFromList(gizmos);
            DrawGizmosFromList(tempGizmos);
            tempGizmos.Clear();

            // ---

            static void DrawGizmosFromList(in List<GizmoInfo> gizmos)
            {
                foreach(GizmoInfo gizmo in gizmos)
                {
                    Gizmos.color = gizmo.color;
                    switch(gizmo.shape)
                    {
                        case Shape.Sphere:
                            Gizmos.DrawSphere(gizmo.position, gizmo.size.x);
                            break;
                        case Shape.WireSphere:
                            Gizmos.DrawWireSphere(gizmo.position, gizmo.size.x);
                            break;
                        case Shape.Cube:
                            Gizmos.DrawCube(gizmo.position, gizmo.size);
                            break;
                        case Shape.WireCube:
                            Gizmos.DrawWireCube(gizmo.position, gizmo.size);
                            break;
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------

        #region Permanent Add/Remove

        #region All Gizmos

        public static void AddGizmo(GizmoInfo info)
        {
            if(Instance == null)
            {
                ConsoleLog.LogWarning("Failed to add Gizmo to GizmoDrawTool as it doesn't exist!");
                return;
            }

            Instance.gizmos.Add(info);
        }

        public static void AddGizmo(Shape shape, Vector3 position, Vector3 size, Color32 color) => Instance.gizmos.Add(new GizmoInfo(shape, position, size, color));

        public static void RemoveGizmo(GizmoInfo info)
        {
            if(Instance == null)
            {
                ConsoleLog.LogWarning("Failed to remove Gizmo from GizmoDrawTool as it doesn't exist!");
                return;
            }

            Instance.gizmos.Remove(info);
        }

        public static void ClearGizmos()
        {
            if(Instance == null)
            {
                ConsoleLog.LogWarning("Failed to clear GizmoDrawTool as it doesn't exist!");
                return;
            }

            Instance.gizmos.Clear();
        }

        #endregion

        // -----------------------------------

        #region Specific Shapes

        public static void AddSphere(Vector3 position, float size, Color32 color) => AddGizmo(Shape.Sphere, position, new Vector3(size, 0f, 0f), color);
        public static void AddWireSphere(Vector3 position, float size, Color32 color) => AddGizmo(Shape.WireSphere, position, new Vector3(size, 0f, 0f), color);
        public static void AddCube(Vector3 position, Vector3 size, Color32 color) => AddGizmo(Shape.Cube, position, size, color);
        public static void AddWireCube(Vector3 position, Vector3 size, Color32 color) => AddGizmo(Shape.WireCube, position, size, color);

        #endregion

        #endregion

        // ---------------------------------------------------------------------------

        #region Temporary Add

        #region All Gizmos

        public static void DrawGizmo(GizmoInfo info)
        {
            if(Instance == null)
            {
                ConsoleLog.LogWarning("Failed to draw Gizmo as GizmoDrawTool doesn't exist!");
                return;
            }

            Instance.tempGizmos.Add(info);
        }

        public static void DrawGizmo(Shape shape, Vector3 position, Vector3 size, Color32 color) => DrawGizmo(new GizmoInfo(shape, position, size, color));

        #endregion

        // -----------------------------------

        #region Specific Shapes

        public static void DrawSphere(Vector3 position, float size, Color32 color) => DrawGizmo(Shape.Sphere, position, new Vector3(size, 0f, 0f), color);
        public static void DrawWireSphere(Vector3 position, float size, Color32 color) => DrawGizmo(Shape.WireSphere, position, new Vector3(size, 0f, 0f), color);
        public static void DrawCube(Vector3 position, Vector3 size, Color32 color) => DrawGizmo(Shape.Cube, position, size, color);
        public static void DrawWireCube(Vector3 position, Vector3 size, Color32 color) => DrawGizmo(Shape.WireCube, position, size, color);

        #endregion

        #endregion

    }
}
