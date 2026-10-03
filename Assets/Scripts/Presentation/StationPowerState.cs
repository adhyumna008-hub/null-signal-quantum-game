using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Dims architectural cyan circuits only; candidate amplitudes own their own visuals.</summary>
    public sealed class StationPowerState : MonoBehaviour
    {
        [SerializeField] private bool powered = true;
        private Renderer[] circuits;
        public void Configure(bool initial) => powered = initial;
        private void Awake()
        {
            var found = new System.Collections.Generic.List<Renderer>();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.name == "Cyan practical") found.Add(renderer);
            circuits = found.ToArray(); SetPowered(powered);
        }
        public void SetPowered(bool value)
        {
            powered = value; if (circuits == null) return;
            var block = new MaterialPropertyBlock();
            foreach (Renderer renderer in circuits)
            {
                if (value) renderer.SetPropertyBlock(null);
                else
                {
                    block.SetColor("_BaseColor", new Color(0.025f, 0.055f, 0.065f));
                    block.SetColor("_EmissionColor", Color.black); renderer.SetPropertyBlock(block);
                }
            }
        }
    }
}
