using UnityEngine;

namespace Demon.Slice
{
    public sealed class InspectionFraming : MonoBehaviour
    {
        [SerializeField, Min(.01f), Tooltip("Distance from the inspection camera to the front clipping plane, in metres.")] float nearPlane = .05f;
        public float NearPlane => nearPlane;
    }
}
