using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    /// <summary>Preserves existing HUD bindings while rendering their values through SDF glyphs.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class SharpHudText : MonoBehaviour
    {
        [SerializeField] private Text source;
        [SerializeField] private TMP_Text output;
        public void Configure(Text original, TMP_Text sharp) { source = original; output = sharp; source.enabled = false; Synchronize(); }
        private void LateUpdate() => Synchronize();
        private void Synchronize()
        {
            if (source == null || output == null) return;
            source.enabled = false;
            if (output.text != source.text) output.text = source.text;
            output.color = source.color;
            output.fontSize = Mathf.Max(18, source.fontSize);
        }
    }
}
