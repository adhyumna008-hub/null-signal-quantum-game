using NullSignal.Presentation;
using UnityEngine;

namespace NullSignal.Gameplay
{
    public sealed class DoorController : MonoBehaviour
    {
        [SerializeField] private Transform leftLeaf, rightLeaf;
        [SerializeField] private Collider blocker;
        [SerializeField] private StationFeedbackAudio audioFeedback;
        private float fraction;
        public bool IsOpen { get; private set; }
        public void Configure(Transform left, Transform right, Collider collision, StationFeedbackAudio audio)
        { leftLeaf = left; rightLeaf = right; blocker = collision; audioFeedback = audio; }
        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true; if (blocker != null) blocker.enabled = false;
            if (audioFeedback != null) audioFeedback.Door();
        }
        private void Update()
        {
            fraction = Mathf.MoveTowards(fraction, IsOpen ? 1f : 0f, Time.deltaTime * 1.25f);
            float ease = fraction * fraction * (3f - 2f * fraction);
            leftLeaf.localPosition = new Vector3(-0.9f - ease * 1.8f, 1.5f, 0f);
            rightLeaf.localPosition = new Vector3(0.9f + ease * 1.8f, 1.5f, 0f);
            if (blocker != null) blocker.enabled = !IsOpen;
        }
    }
}
