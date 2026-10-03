using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Sound begins with the visible, committed danger telegraph.</summary>
    public sealed class ThreatAudioFeedback : MonoBehaviour
    {
        [SerializeField] private StationFeedbackAudio feedback;
        [SerializeField] private LineRenderer warning;
        private bool visible;
        public void Configure(StationFeedbackAudio audio, LineRenderer telegraph) { feedback = audio; warning = telegraph; }
        private void LateUpdate()
        {
            bool next = warning != null && warning.enabled && warning.gameObject.activeInHierarchy;
            if (next && !visible) feedback?.Threat(); visible = next;
        }
        private void OnDisable() => visible = false;
    }
}
