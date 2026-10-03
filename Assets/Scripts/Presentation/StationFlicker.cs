using UnityEngine;

namespace NullSignal.Presentation
{
    public sealed class StationFlicker : MonoBehaviour
    {
        [SerializeField] private Light practical;
        [SerializeField] private float intensity = 2f;
        public void Configure(Light lamp, float level) { practical = lamp; intensity = level; }
        private void Update()
        {
            float signal = Mathf.Sin(Time.time * 2.1f) * Mathf.Sin(Time.time * 7.3f);
            if (practical != null) practical.intensity = intensity * (signal > 0.55f ? 0.35f : 1f);
        }
    }
}
