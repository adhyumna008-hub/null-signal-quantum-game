using NullSignal.Player;
using NullSignal.Story;
using UnityEngine;

namespace NullSignal.Gameplay
{
    public sealed class FinaleInteractable : MonoBehaviour, IPlayerInteractable, IInteractionPrompt
    {
        [SerializeField] private FinaleRoomController owner;
        [SerializeField] private bool extraction;
        public string Prompt => extraction ? "BOARD EMERGENCY CRAFT" : "SHUT DOWN ANVESHAK";
        public bool CanInteract => owner != null && owner.CanUseDevice(extraction);
        public void Configure(FinaleRoomController controller, bool isExtraction) { owner = controller; extraction = isExtraction; }
        public void Interact(PlayerInteraction player) { if (CanInteract) owner.UseDevice(extraction); }
    }
}
