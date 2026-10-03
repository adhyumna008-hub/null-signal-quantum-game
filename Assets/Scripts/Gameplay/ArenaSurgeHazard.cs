using NullSignal.Player;
using NullSignal.Presentation;
using UnityEngine;

namespace NullSignal.Gameplay
{
    /// <summary>One bounded warning at a time; no rigidbodies, projectiles or quantum state changes.</summary>
    public sealed class ArenaSurgeHazard : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private StationCamera cameraRig;
        [SerializeField] private LineRenderer outline;
        private PlayerHealth health;
        private PlayerDodge dodge;
        private bool pressure, charging, radial, struck;
        private int cycle;
        private float next, started, interval = 5f;
        private Vector3 origin;
        public void Configure(Transform actor, StationCamera camera, LineRenderer warning)
        { player = actor; cameraRig = camera; outline = warning; }
        private void Awake() { health = player.GetComponent<PlayerHealth>(); dodge = player.GetComponent<PlayerDodge>(); outline.enabled = false; }
        public void SetPressure(bool value, float seconds = 5f)
        {
            interval = seconds;
            if (pressure == value) return;
            pressure = value; charging = false; outline.enabled = false; next = Time.time + 3f;
        }
        private void OnDisable() { if (outline != null) outline.enabled = false; }
        private void Update()
        {
            if (!pressure || health.Current <= 0) { outline.enabled = false; return; }
            if (!charging)
            {
                if (Time.time < next) return;
                charging = true; struck = false; radial = ++cycle % 3 == 0; started = Time.time;
                origin = radial ? transform.position : player.position;
                origin.y = transform.position.y + .07f;
            }
            float age = Time.time - started;
            bool impact = age >= 1.4f;
            float radius = radial ? (impact ? Mathf.Lerp(.6f, 10f, (age - 1.4f) / 2f) : .6f) : 1.7f;
            outline.enabled = true;
            Color tint = impact ? new Color(1f, .38f, .15f) : new Color(1f, .7f, .25f, .55f + .3f * Mathf.Sin(age * 9f));
            outline.startColor = outline.endColor = tint; outline.widthMultiplier = impact ? .10f : .045f;
            for (int i = 0; i < outline.positionCount; i++)
            {
                float a = i * Mathf.PI * 2f / (outline.positionCount - 1);
                outline.SetPosition(i, origin + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius);
            }
            Vector3 delta = player.position - origin; delta.y = 0;
            bool contact = radial ? Mathf.Abs(delta.magnitude - radius) < .4f : delta.sqrMagnitude < radius * radius;
            if (impact && contact && !struck && !dodge.IsDodging)
            {
                struck = true; health.Damage(8); player.GetComponent<PlayerCombat>()?.FlashDamage(); cameraRig.Impulse(.06f);
            }
            if (age >= (radial ? 3.4f : 1.65f))
            { charging = false; outline.enabled = false; next = Time.time + interval; }
        }
    }
}
