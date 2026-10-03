using UnityEngine;

namespace NullSignal.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class StationCamera : MonoBehaviour
    {
        [SerializeField] private Transform player;
        private Vector3 center, focus, velocity;
        private Camera view;
        private int candidates;
        private bool encounterFraming;
        private bool combatFraming;
        public void FrameCombat(bool active) => combatFraming = active;
        private float zoomVelocity, impulse;
        private float quantumPush;
        private float sequenceArc;
        public static bool ReducedShake { get; set; }
        public void SequenceArc(float value) => sequenceArc = Mathf.Clamp(value, -1f, 1f);
        public void QuantumPush(float amount) => quantumPush = Mathf.Clamp(amount, 0, .15f);
        public void Configure(Transform actor) { player = actor; Initialize(); }
        private void Awake() => Initialize();
        private void Initialize()
        {
            view = GetComponent<Camera>(); view.orthographic = true;
            view.allowDynamicResolution = false; view.allowMSAA = true;
            if (player != null) { center = player.position; focus = Desired(); }
            view.orthographicSize = Size(); Position();
        }
        public void SetRoom(Vector3 origin, int count) { center = origin; candidates = count; }
        public void FrameEncounter(bool active) => encounterFraming = active;
        public void SnapToPlayer() { focus = Desired(); velocity = Vector3.zero; Position(); }
        public void Impulse(float amount) => impulse = ReducedShake ? 0 : Mathf.Min(amount, 0.12f);
        private Vector3 Desired()
        {
            if (player == null) return center + Vector3.up;
            Vector3 delta = player.position - center; delta.y = 0f;
            // Hold the array in view near its center; follow fully through long entrances and corridors.
            Vector3 target = center + Vector3.ClampMagnitude(delta * 0.15f, 0.75f);
            if (!encounterFraming && delta.magnitude > 5f) target = Vector3.Lerp(target, player.position, Mathf.InverseLerp(5f, 9f, delta.magnitude));
            if (combatFraming) target += new Vector3(.7071f, 0, -.7071f) * 1.15f;
            target.y = 0.85f; return target;
        }
        private float Size() => (combatFraming ? (candidates >= 16 ? 10f : candidates >= 8 ? 9.3f : 8.9f) : candidates >= 16 ? 9.4f : candidates >= 8 ? 7.9f : 7.2f) * Mathf.Max(1f, 1.5f / Mathf.Max(0.6f, view.aspect));
        private void LateUpdate()
        {
            focus = Vector3.SmoothDamp(focus, Desired(), ref velocity, 0.20f);
            view.orthographicSize = Mathf.SmoothDamp(view.orthographicSize, Size() - quantumPush, ref zoomVelocity, 0.25f);
            impulse = Mathf.MoveTowards(impulse, 0f, Time.deltaTime * 0.7f); Position();
        }
        private void Position()
        {
            Quaternion rotation = Quaternion.Euler(45f - sequenceArc * 3f, 45f + sequenceArc * 7f, 0f);
            Vector3 shake = rotation * Vector3.right * (Mathf.Sin(Time.unscaledTime * 48f) * impulse);
            transform.SetPositionAndRotation(focus + shake - rotation * Vector3.forward * 32f, rotation);
        }
    }
}
