using NullSignal.Player;
using UnityEngine;

namespace NullSignal.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class FoundationCandidate : MonoBehaviour
    {
        [SerializeField] private FoundationQuantumPrototype owner;
        [SerializeField, Min(0)] private int candidateIndex;

        public bool CanInteract => owner != null && owner.Simulation != null &&
            candidateIndex >= 0 && candidateIndex < owner.Simulation.CandidateCount;

        public void Configure(FoundationQuantumPrototype simulationOwner, int index)
        {
            owner = simulationOwner;
            candidateIndex = index;
        }

        public void Interact(PlayerInteraction player)
        {
            if (CanInteract)
                owner.InspectCandidate(candidateIndex);
        }
    }
}
