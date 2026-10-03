using NullSignal.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NullSignal.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private LineRenderer shot;
        [SerializeField] private Renderer wrist;
        private PlayerHealth health;
        private PlayerDodge dodge;
        private MaterialPropertyBlock properties;
        private float attackAt = -100f, damageAt = -100f;
        public float AttackPose => Mathf.Clamp01(1f - (Time.time - attackAt) / .3f);
        public float DamagePose => Mathf.Clamp01(1f - (Time.time - damageAt) / .2f);
        public void Configure(Camera camera, LineRenderer beam, Renderer device) { view = camera; shot = beam; wrist = device; }
        public void FlashDamage() => damageAt = Time.time;
        private void Awake() { health = GetComponent<PlayerHealth>(); dodge = GetComponent<PlayerDodge>(); properties = new MaterialPropertyBlock(); }
        private void OnEnable()
        {
            if (health == null) health = GetComponent<PlayerHealth>();
            if (dodge == null) dodge = GetComponent<PlayerDodge>();
            if (properties == null) properties = new MaterialPropertyBlock();
        }
        private void Update()
        {
            if (shot == null || view == null || health == null || dodge == null) return;
            shot.enabled = Time.time - attackAt < .12f;
            if (wrist != null)
            {
                Color tint = DamagePose > 0 ? new Color(1f, .2f, .12f) : new Color(.2f, .85f, 1f);
                properties.SetColor("_EmissionColor", tint * (1f + AttackPose * 4f)); wrist.SetPropertyBlock(properties);
            }
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || !Application.isFocused) return;
            if (health.Current <= 0 || dodge.IsDodging || Time.time - attackAt < .38f || Time.timeScale <= 0) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            Ray ray = view.ScreenPointToRay(Mouse.current.position.ReadValue());
            CombatManifestation target = null; float nearest = float.PositiveInfinity;
            foreach (RaycastHit hit in Physics.RaycastAll(ray, 100f, ~0, QueryTriggerInteraction.Collide))
            {
                CombatManifestation candidate = hit.collider.GetComponentInParent<CombatManifestation>();
                if (candidate != null && candidate.CanBeAimed && hit.distance < nearest)
                { target = candidate; nearest = hit.distance; }
            }
            Vector3 start = transform.position + Vector3.up * 1.35f;
            var plane = new Plane(Vector3.up, transform.position + Vector3.up * 1.4f);
            Vector3 end = plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : start + transform.forward * 12f;
            if (target != null) end = target.AimPoint;
            Vector3 aim = end - start; if (aim.magnitude > 18f) { end = start + aim.normalized * 18f; target = null; }
            Vector3 facing = end - start; facing.y = 0;
            if (facing.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(facing);
            attackAt = Time.time; shot.SetPosition(0, start + transform.right * .28f); shot.SetPosition(1, end); shot.enabled = true;
            bool blocked = false;
            foreach (RaycastHit hit in Physics.RaycastAll(start, (end - start).normalized, Vector3.Distance(start, end), ~0, QueryTriggerInteraction.Ignore))
                if (!hit.collider.transform.IsChildOf(transform)) { blocked = true; break; }
            if (!blocked && target != null) target.ReceiveHit(25f);
#endif
        }
    }
}
