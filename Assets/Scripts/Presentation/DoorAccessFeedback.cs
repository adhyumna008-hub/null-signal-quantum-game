using NullSignal.Gameplay;
using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Amber means sealed; green confirms the real door controller has opened.</summary>
    public sealed class DoorAccessFeedback : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        [SerializeField] private Renderer[] strips;
        private MaterialPropertyBlock properties;
        private float opened;
        public void Configure(DoorController source, Renderer[] indicators) { door = source; strips = indicators; }
        private void LateUpdate()
        {
            if (door == null || strips == null) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            opened = Mathf.MoveTowards(opened, door.IsOpen ? 1 : 0, Time.deltaTime * 3f);
            Color tint = Color.Lerp(QuantumPresentationEffects.Redistribution, QuantumPresentationEffects.Success, opened);
            properties.SetColor("_BaseColor", tint); properties.SetColor("_EmissionColor", tint * 1.7f);
            foreach (Renderer strip in strips) if (strip != null) strip.SetPropertyBlock(properties);
        }
    }
}
