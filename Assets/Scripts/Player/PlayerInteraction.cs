using UnityEngine;
using NullSignal.Gameplay;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NullSignal.Player
{
    public interface IPlayerInteractable
    {
        bool CanInteract { get; }
        void Interact(PlayerInteraction player);
    }
    public interface IInteractionPrompt { string Prompt { get; } }

    [DisallowMultipleComponent]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float radius = 1.8f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        private readonly Collider[] nearby = new Collider[64];
        private float refreshAt;
        public MonoBehaviour CurrentTarget { get; private set; }
        public string Prompt => CurrentTarget == null ? string.Empty : CurrentTarget is StoryInteractable story
            ? "E — " + story.Prompt : CurrentTarget is IInteractionPrompt device ? "E — " + device.Prompt : "E — INTERACT";

        private void Update()
        {
            if (Time.unscaledTime >= refreshAt) { RefreshTarget(); refreshAt = Time.unscaledTime + .10f; }
            if (Time.timeScale > 0f && InteractionPressed())
                TryInteract();
        }

        public bool TryInteract()
        {
            RefreshTarget();
            if (!(CurrentTarget is IPlayerInteractable closest)) return false;
            closest.Interact(this);
            RefreshTarget();
            return true;
        }

        private void RefreshTarget()
        {
            CurrentTarget = null;
            var health = GetComponent<PlayerHealth>();
            if (health != null && health.Current <= 0) return;
            Vector3 origin = transform.position + Vector3.up * 0.7f;
            int count = Physics.OverlapSphereNonAlloc(origin, radius, nearby, interactionLayers, QueryTriggerInteraction.Collide);
            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider hit = nearby[i];
                if (hit.transform.IsChildOf(transform))
                    continue;
                float distance = (hit.ClosestPoint(origin) - origin).sqrMagnitude;
                if (distance >= closestDistance)
                    continue;
                foreach (MonoBehaviour behaviour in hit.GetComponentsInParent<MonoBehaviour>())
                {
                    if (behaviour != null && behaviour.isActiveAndEnabled &&
                        behaviour is IPlayerInteractable interactable && interactable.CanInteract)
                    {
                        CurrentTarget = behaviour;
                        closestDistance = distance;
                        break;
                    }
                }
            }
        }

        private static bool InteractionPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.E);
#else
            return false;
#endif
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.7f, radius);
        }
    }
}
