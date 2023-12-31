using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VinTools.MonoBehaviours
{
    /// <summary>
    /// A parallax scroller script that stores every layer in a list of parallax layer objects.
    /// You can set how much does the parallax effect affects each axis, and set the parallax effect based on the Z distance of the objects.
    /// On the parallax layers you can change individually if they should wrap around and how much should they wrap.
    /// </summary>
    /// 
    /// Script by Vinark
    public class ParallaxScroller : MonoBehaviour
    {
        [SerializeField, Tooltip("The reference object, it's usually the camera")] public Transform ReferenceObject;
        [SerializeField, Tooltip("Determines if the spicified axes should be affected by the parallax effect, and how much")] public Vector2 AffectAxes = Vector2.one;
        [SerializeField, Tooltip("The behaviour of the parallax scroller")] public ParallaxScrollBehaviour ScrollBehaviour;

        //[Space]
        [SerializeField, Tooltip("If this is enabled the parallax scale will be set based on the Z distance of the objects")] public bool AdaptiveParallaxScale = false;
        [SerializeField, Tooltip("Used to determine the parallax scale with the adaptive parallax scale option")] public float HorizonDistance = 10;

        //[Space]
        [SerializeField, Tooltip("Set a custom reference position for the layers? " +
            "With this option you can create a more controlled parallax effect. " +
            "If you want your layers to align at a certain place, this option is the way to go.")]
        public bool CustomReferencePosition = false;
        [SerializeField, Tooltip("If the reference object is at this position, the layers will show the default layer alignment")] public Vector2 StartReference = Vector2.zero;

        //[Space]
        [SerializeField, Tooltip("Snap layers to a pixel grid? This option only works with absolute behaviour")] public bool SnapToPixelGrid = false;
        [SerializeField, Tooltip("The number of pixels to snap to per unit")] public int PixelsPerUnit = 16;
        [SerializeField, Tooltip("Offset for snapping to the pixel grid in pixels, 0.5 would be a half pixel offset")] public Vector2 PixelOffset = Vector2.zero;

        //[Space]
        [SerializeField] public ParallaxLayer[] Layers;

        private bool _lastAdaptiveScale;
        private float _lastHorizonDistance;

        private void OnValidate()
        {
            //if changed the settings regarding adaptive parallax
            if (_lastAdaptiveScale != AdaptiveParallaxScale || _lastHorizonDistance != HorizonDistance)
            {
                //save the current settings
                _lastAdaptiveScale = AdaptiveParallaxScale;
                _lastHorizonDistance = HorizonDistance;

                //break out if adaptive parallax is not enabled
                if (!AdaptiveParallaxScale || HorizonDistance == 0) return;

                //set parallax scale for every layer
                foreach (var layer in Layers) layer.SetParallaxScale(HorizonDistance);
            }
        }

        private void Start()
        {
            //set start reference position
            if (!CustomReferencePosition) StartReference = ReferenceObject.position;

            //for every layer
            foreach (var layer in Layers)
            {
                //setup parallax start values
                layer.SetupParallax(ReferenceObject, StartReference);
                //set parallax scale if adaptive
                if (AdaptiveParallaxScale) layer.SetParallaxScale(HorizonDistance);
            }
        }
        private void Update()
        {
            //update every parallax layer
            foreach (var layer in Layers) layer.UpdateParallax(ScrollBehaviour, ReferenceObject, AffectAxes);

            //align them to the grid
            if (ScrollBehaviour != ParallaxScrollBehaviour.Absolute || !SnapToPixelGrid) return;
            foreach (var layer in Layers) layer.Object.position = AlignToGridPosition(layer.Object.position, PixelsPerUnit, PixelOffset);
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

        public enum ParallaxScrollBehaviour
        {
            [Tooltip("The absolute behaviour compares the start position and the current position of the reference object. " +
                "This means that the layers cannot be moved, only by this script.")]
            Absolute = 0,

            [Tooltip("The relative behaviour compares the position of the reference object to the last frame." +
                "This means that the layers can be moved freely")]
            Relative = 1,
        }

        [System.Serializable]
        public class ParallaxLayer
        {
            [Tooltip("Object that should be moved with the ")]
            public Transform Object;
            [Tooltip("This scale determines how fast the object moves relative to the camera\n" +
                "\n>1 - inverts direction" +
                "\n=1 - stays on the screen" +
                "\n1-0 - objects in background" +
                "\n=0 - main layer, doesn't move" +
                "\n<0 - objects in foreground")]
            public float ParallaxScale = 0;

            [Space]
            [Tooltip("Wrap around after a distance?")] public bool Wrap;
            [Tooltip("The distance to wrap around after")] public Vector2 WrapLength = Vector2.positiveInfinity;

            Vector3 _objStartPos;
            Vector3 _lastReferencePos;
            Vector3 _startReferencePos;

            /// <summary>
            /// Set the starting position of the reference object
            /// </summary>
            /// <param name="ReferenceObject"></param>
            public void SetupParallax(Transform regerenceObject, Vector3 referencePosition)
            {
                _lastReferencePos = regerenceObject.position;
                _startReferencePos = referencePosition;
                _objStartPos = Object.position;
            }

            /// <summary>
            /// Set the parallax scale based on the object's Z position compared to the horizon distance
            /// </summary>
            /// <param name="horizonDistance"></param>
            public void SetParallaxScale(float horizonDistance)
            {
                if (Object == null) return;

                ParallaxScale = Object.position.z / horizonDistance;
            }

            public void UpdateParallax(ParallaxScrollBehaviour behaviour, Transform referenceObject, Vector2 affectAxes) => UpdateParallax(behaviour, referenceObject, affectAxes, ParallaxScale);
            public void UpdateParallax(ParallaxScrollBehaviour behaviour, Transform referenceObject, Vector2 affectAxes, float parallaxScale)
            {
                switch (behaviour)
                {
                    case ParallaxScrollBehaviour.Absolute:
                        UpdateParallaxAbsolute(referenceObject, affectAxes, parallaxScale);
                        break;
                    case ParallaxScrollBehaviour.Relative:
                        UpdateParallaxRelative(referenceObject, affectAxes, parallaxScale);
                        break;
                }
            }

            private void UpdateParallaxRelative(Transform referenceObject, Vector2 affectAxes, float parallaxScale)
            {
                //get how much did the reference move
                Vector3 referenceMovedBy = referenceObject.position - _lastReferencePos;
                Vector3 objMoveby = new Vector3(referenceMovedBy.x * affectAxes.x, referenceMovedBy.y * affectAxes.y, 0) * parallaxScale;

                //move the objects
                Object.position += objMoveby;

                //wrap
                if (Wrap)
                {
                    //get distance to reference object
                    Vector3 dist = Object.position - referenceObject.position;

                    //if distance is greater than the half of the wrap legth, than wrap by the wrap length amount
                    if (Mathf.Abs(dist.x) > WrapLength.x / 2) Object.position += Vector3.left * WrapLength.x * Mathf.Sign(dist.x);
                    if (Mathf.Abs(dist.y) > WrapLength.y / 2) Object.position += Vector3.down * WrapLength.y * Mathf.Sign(dist.y);
                }

                //save last point
                _lastReferencePos = referenceObject.position;
            }

            private void UpdateParallaxAbsolute(Transform referenceObject, Vector2 affectAxes, float parallaxScale)
            {
                //get how much did the reference move
                Vector3 referenceMovedBy = referenceObject.position - _startReferencePos;
                Vector3 objMoveby = new Vector3(referenceMovedBy.x * affectAxes.x, referenceMovedBy.y * affectAxes.y, 0) * parallaxScale;

                //wrap
                if (Wrap)
                {
                    //get distance to reference object
                    Vector3 dist = Object.position - referenceObject.position;

                    //if distance is greater than the half of the wrap legth, than wrap by the wrap length amount
                    if (Mathf.Abs(dist.x) > WrapLength.x / 2) _objStartPos += Vector3.left * WrapLength.x * Mathf.Sign(dist.x);
                    if (Mathf.Abs(dist.y) > WrapLength.y / 2) _objStartPos += Vector3.down * WrapLength.y * Mathf.Sign(dist.y);
                }

                //move the objects
                Object.position = _objStartPos + objMoveby;

                //save last point (this is required if switching to relative at runtime)
                _lastReferencePos = referenceObject.position;
            }
        }
    }
}