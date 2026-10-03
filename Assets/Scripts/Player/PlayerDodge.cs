using UnityEngine;

namespace NullSignal.Player
{
    /// <summary>Movement-only dodge; damage, stamina and combat are outside Phase 1.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float duration = 0.18f;
        [SerializeField, Min(0f)] private float cooldown = 0.65f;
        [SerializeField, Min(0f)] private float speed = 11f;

        private Vector3 direction;
        private float remainingDuration;
        private float remainingCooldown;

        public bool IsDodging => isActiveAndEnabled && remainingDuration > 0f;
        public Vector3 Velocity => IsDodging ? direction * speed : Vector3.zero;

        // The controller advances this once per frame, before processing new input.
        public void Tick(float deltaTime)
        {
            remainingDuration = Mathf.Max(0f, remainingDuration - deltaTime);
            remainingCooldown = Mathf.Max(0f, remainingCooldown - deltaTime);
        }

        public bool TryDodge(Vector3 worldDirection)
        {
            if (!isActiveAndEnabled || IsDodging || remainingCooldown > 0f)
                return false;
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 0.001f)
                return false;

            direction = worldDirection.normalized;
            remainingDuration = duration;
            remainingCooldown = duration + cooldown;
            return true;
        }

        public void Cancel() => remainingDuration = 0f;

        private void OnDisable() => Cancel();
    }
}
