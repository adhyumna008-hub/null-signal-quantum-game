using NullSignal.Gameplay;
using UnityEngine;

namespace NullSignal.Presentation
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public sealed class VisualCameraController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private Vector3 roomFocus = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private float size = 8.2f;
        private Camera view;
        private Vector3 focus, velocity;
        private float zoomVelocity;

        public void Configure(Transform follow, QuantumEncounter owner)
        { player = follow; encounter = owner; Initialize(); }
        private void Awake() => Initialize();
        private void Initialize()
        {
            view = GetComponent<Camera>(); view.orthographic = true;
            view.orthographicSize = DesiredSize(); focus = roomFocus; Position();
        }
        private float DesiredSize()
        {
            float encounterExtra = encounter != null && encounter.HasScanned && !encounter.HasMeasured ? 0.45f : 0f;
            return (size + encounterExtra) * Mathf.Max(1f, 1.6f / Mathf.Max(0.5f, view.aspect));
        }
        private void LateUpdate()
        {
            Vector3 offset = player != null ? player.position - roomFocus : Vector3.zero;
            offset.y = 0f;
            float follow = encounter != null && encounter.HasScanned ? 0.10f : 0.18f;
            focus = Vector3.SmoothDamp(focus, roomFocus + Vector3.ClampMagnitude(offset * follow, 0.65f), ref velocity, 0.4f);
            view.orthographicSize = Mathf.SmoothDamp(view.orthographicSize, DesiredSize(), ref zoomVelocity, 0.6f);
            Position();
        }
        private void Position()
        {
            Quaternion rotation = Quaternion.Euler(42f, 45f, 0f);
            transform.SetPositionAndRotation(focus - rotation * Vector3.forward * 30f, rotation);
        }
    }
}
