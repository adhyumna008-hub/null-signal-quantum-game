using NullSignal.Gameplay;
using NullSignal.Player;
using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Only reads actual candidate snapshots. It never knows which index is the target.</summary>
    [DisallowMultipleComponent]
    public sealed class CandidateVisualizer : MonoBehaviour
    {
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private int candidateIndex;
        [SerializeField] private Transform body;
        [SerializeField] private LineRenderer waveform;
        [SerializeField] private Transform[] flowParticles;
        [SerializeField] private Color signalColor = new Color(0.1f, 0.85f, 0.95f, 1f);
        [SerializeField, Min(0.05f)] private float collapseSeconds = 0.5f;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock properties;
        private Renderer bodyRenderer;
        private int runVersion = -1;
        private float visualMagnitude;
        private float visibility = 1f;
        private float waveClock;
        private float flowPosition;

        public int CandidateIndex => candidateIndex;
        public Vector3 LabelPosition => transform.position + Vector3.up * 3.1f;
        public bool CanInteract => encounter != null && encounter.IsInitialized &&
            (!encounter.HasMeasured || encounter.MeasuredCandidateIndex == candidateIndex);

        public void Configure(QuantumEncounter owner, int index, Transform signalBody,
            LineRenderer wave, Transform[] particles)
        {
            encounter = owner;
            candidateIndex = index;
            body = signalBody;
            waveform = wave;
            flowParticles = particles;
            RefreshVisual(0f);
        }

        public void Interact(PlayerInteraction player)
        {
            if (CanInteract) encounter.InspectCandidate(candidateIndex);
        }

        private void LateUpdate() => RefreshVisual(Time.deltaTime);

        public void RefreshVisual(float deltaTime)
        {
            if (encounter == null || !encounter.IsInitialized || body == null || waveform == null)
                return;
            if (properties == null) properties = new MaterialPropertyBlock();
            if (bodyRenderer == null) bodyRenderer = body.GetComponent<Renderer>();
            var candidate = encounter.GetCandidate(candidateIndex);
            float magnitude = (float)System.Math.Abs(candidate.Amplitude);
            if (runVersion != encounter.RunVersion)
            {
                runVersion = encounter.RunVersion;
                visualMagnitude = magnitude;
                visibility = 1f;
                waveClock = 0f;
                flowPosition = 0f;
            }
            visualMagnitude = Mathf.MoveTowards(visualMagnitude, magnitude, deltaTime * 3f);
            bool collapsed = encounter.HasMeasured && encounter.MeasuredCandidateIndex != candidateIndex;
            visibility = Mathf.MoveTowards(visibility, collapsed ? 0f : 1f, deltaTime / collapseSeconds);
            float strength = visualMagnitude * visibility;
            float height = (0.12f + 2.4f * visualMagnitude) * visibility;
            body.localScale = new Vector3(0.7f * visibility, Mathf.Max(0.001f, height * 0.5f), 0.7f * visibility);
            body.localPosition = new Vector3(0f, 0.22f + height * 0.5f, 0f);
            bodyRenderer.enabled = visibility > 0.001f && strength > 0.001f;
            Color color = signalColor;
            color.a = strength * 0.9f;
            properties.SetColor(BaseColor, color);
            properties.SetColor(EmissionColor, signalColor * (strength * 1.8f));
            bodyRenderer.SetPropertyBlock(properties);

            // A negative amplitude mirrors the wave; directional dots also reverse their travel.
            waveClock += deltaTime * 3f;
            float sign = candidate.HasInvertedPhase ? -1f : 1f;
            if (magnitude > 0.00001f)
                flowPosition = Mathf.Repeat(flowPosition + sign * deltaTime * 0.65f, 1f);
            waveform.enabled = visibility > 0.001f;
            for (int point = 0; point < waveform.positionCount; point++)
            {
                float x = Mathf.Lerp(-0.95f, 0.95f, (float)point / (waveform.positionCount - 1));
                float y = 1.3f + sign * visualMagnitude * 0.75f * Mathf.Sin(x * 7f - waveClock);
                waveform.SetPosition(point, new Vector3(x * visibility, y, -0.5f));
            }
            Color waveColor = signalColor;
            waveColor.a = (0.15f + 0.85f * visualMagnitude) * visibility;
            waveform.startColor = waveColor;
            waveform.endColor = waveColor;
            waveform.widthMultiplier = 0.035f + 0.045f * strength;

            if (flowParticles == null) return;
            for (int i = 0; i < flowParticles.Length; i++)
            {
                Transform particle = flowParticles[i];
                if (particle == null) continue;
                particle.gameObject.SetActive(strength > 0.001f);
                float fraction = Mathf.Repeat(flowPosition + (float)i / flowParticles.Length, 1f);
                particle.localPosition = new Vector3(0.48f, 0.22f + fraction * Mathf.Max(0.12f, height), 0f);
                particle.localScale = Vector3.one * (0.06f + 0.13f * strength);
            }
        }
    }
}
