using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Fixed isometric orientation with a small, bounded player-follow offset.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 roomFocus = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private float yaw = 45f;
        [SerializeField, Range(10f, 80f)] private float pitch = 35.264f;
        [SerializeField, Min(1f)] private float distance = 20f;
        [SerializeField, Min(1f)] private float orthographicSize = 8.5f;
        [SerializeField, Min(0f)] private float followLimit = 1.5f;
        [SerializeField, Range(0f, 1f)] private float followAmount = 0.3f;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;

        private Vector3 currentFocus;
        private Vector3 focusVelocity;
        private bool initialized;

        public void Configure(Transform player, Vector3 center)
        {
            target = player;
            roomFocus = center;
            InitializeView();
        }

        private void Awake() => InitializeView();

        private void InitializeView()
        {
            Camera view = GetComponent<Camera>();
            view.orthographic = true;
            view.orthographicSize = orthographicSize;
            currentFocus = DesiredFocus();
            focusVelocity = Vector3.zero;
            initialized = true;
            PositionView();
        }

        private Vector3 DesiredFocus()
        {
            if (target == null)
                return roomFocus;
            Vector3 offset = target.position - roomFocus;
            offset.y = 0f;
            return roomFocus + Vector3.ClampMagnitude(offset * followAmount, followLimit);
        }

        private void LateUpdate()
        {
            if (!initialized)
                InitializeView();
            currentFocus = Vector3.SmoothDamp(currentFocus, DesiredFocus(), ref focusVelocity,
                smoothTime, Mathf.Infinity, Time.deltaTime);
            PositionView();
        }

        private void PositionView()
        {
            Quaternion orientation = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(currentFocus - orientation * Vector3.forward * distance, orientation);
        }
    }
}
