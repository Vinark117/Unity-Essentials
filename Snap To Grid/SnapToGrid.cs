using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VinTools.MonoBehaviours
{
    /// <summary>
    /// This script snaps the object that the script is added to to a grid, which can also be used in the editor to align objects with a pixel grid.
    /// This script only works in the editor and will be completely disabled when the game is running.
    /// </summary>
    /// 
    /// Script by Vinark
#if (UNITY_EDITOR)
    [ExecuteInEditMode]
#endif
    public class SnapToGrid : MonoBehaviour
    {
        [Header("Grid snapping")]
        [Tooltip("Number of pixels per Unity unit")] public float PixelsPerUnit;
        [Tooltip("Offset for snapping to the pixel grid in pixels, 0.5 would be a half pixel offset")] public Vector3 Offset;

        [Header("Constraints")]
        [Tooltip("Fix position on the X axis")] public bool FixXPos;
        [Tooltip("Fix position on the Y axis")] public bool FixYPos;
        [Tooltip("Fix position on the Z axis")] public bool FixZPos;
        [Tooltip("The fixed position")] public Vector3 FixedPos;

        private void Update()
        {
#if (UNITY_EDITOR)
            if (!Application.isPlaying) UpdatePos();
#endif
        }

#if (UNITY_EDITOR)
        public void UpdatePos()
        {
            if (PixelsPerUnit == 0) return;

            Vector3 gp = AlignToGridPosition(transform.localPosition, PixelsPerUnit, Offset);

            transform.localPosition = new Vector3(
                FixXPos ? FixedPos.x : gp.x,
                FixYPos ? FixedPos.y : gp.y,
                FixZPos ? FixedPos.z : gp.z);
        }

        /// <summary>
        /// Adjusts the position to fit on a pixel grid
        /// </summary>
        /// <param name="p">Position to adjust</param>
        /// <param name="gridSize">Grid size in pixels per unit</param>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        public static Vector3 AlignToGridPosition(Vector3 p, float gridSize, Vector3 offset)
        {
            return new Vector3(ToGridPos(p.x) + GetOffset(offset.x), ToGridPos(p.y) + GetOffset(offset.y), ToGridPos(p.z) + GetOffset(offset.z));

            float ToGridPos(float pos) => Mathf.Round(pos * gridSize) / gridSize;
            float GetOffset(float offset) => (1f / (float)gridSize * offset);
        }
#endif
    }
}