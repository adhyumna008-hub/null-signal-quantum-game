using NullSignal.Player;
using NullSignal.Story;
using UnityEngine;

namespace NullSignal.Gameplay
{
    public enum StationInteraction { ManualRelease, AnveshPickup }
    public sealed class StoryInteractable : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private MainGameDirector director;
        [SerializeField] private StationInteraction action;
        [SerializeField] private Renderer indicator;
        [SerializeField] private string prompt;
        private MaterialPropertyBlock property;
        private float pulse;
        private PlayerInteraction interaction;
        public bool CanInteract => director != null && director.CanInteract(action);
        public string Prompt => prompt;
        public void Configure(MainGameDirector owner, StationInteraction kind, Renderer light, string hint)
        { director = owner; action = kind; indicator = light; prompt = hint; }
        public void Interact(PlayerInteraction player)
        {
            if (!CanInteract) return;
            director.Interact(action); pulse = 1f;
        }
        private void Update()
        {
            if (indicator == null) return;
            if (property == null) property = new MaterialPropertyBlock();
            if (interaction == null && director != null) interaction = director.Player.GetComponent<PlayerInteraction>();
            pulse = Mathf.MoveTowards(pulse, 0f, Time.deltaTime);
            bool nearby = CanInteract && interaction != null && interaction.CurrentTarget == this;
            property.SetColor("_EmissionColor", new Color(0.3f, 0.9f, 1f) * (nearby ? 1.6f + Mathf.Sin(Time.time * 3f) * .2f : .35f + pulse * 2f));
            indicator.SetPropertyBlock(property);
        }
    }
}
