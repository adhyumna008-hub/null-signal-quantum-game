using NullSignal.Gameplay;
using NullSignal.Player;
using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Read-only presentation of a candidate; deliberately has no target-index knowledge.</summary>
    [DisallowMultipleComponent]
    public sealed class QuantumNodePresentation : MonoBehaviour
    {
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private int candidateIndex;
        [SerializeField] private Transform core;
        [SerializeField] private Renderer coreRenderer;
        [SerializeField] private LineRenderer waveform;
        [SerializeField] private LineRenderer[] orbits;
        [SerializeField] private LineRenderer probabilityArc;
        [SerializeField] private LineRenderer lockHalo;
        [SerializeField] private Transform[] motes;
        private MaterialPropertyBlock properties;
        private readonly Vector3[] wavePoints = new Vector3[56];
        private float magnitude = 0.5f, visibility = 1f, clock, lockAge;
        private int runVersion = -1;
        private bool wasMeasured;
        private bool wasScanned;
        private float scanAt, phaseAt = -100f, unstableAt = -100f, previousAmplitude;
        private int previousIteration;
        private LineRenderer measurementBeam;
        private Renderer[] pedestalLights;
        private MaterialPropertyBlock pedestalProperties;
        private void Awake() => EnsureVisualCaches();
        private void OnEnable() => EnsureVisualCaches();
        private void EnsureVisualCaches()
        {
            if (pedestalLights == null)
            {
                var lights = new System.Collections.Generic.List<Renderer>();
                foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
                    if (renderer.name == "Cyan practical") lights.Add(renderer);
                pedestalLights = lights.ToArray();
            }
            if (pedestalProperties == null) pedestalProperties = new MaterialPropertyBlock();
            if (properties == null) properties = new MaterialPropertyBlock();
        }
        private static readonly Color Cyan = new Color(0.35f, 0.88f, 1f);
        private static readonly Color Violet = new Color(0.57f, 0.46f, 0.95f);

        public int CandidateIndex => candidateIndex;
        public bool CanInteract => encounter != null && encounter.IsInitialized && !encounter.HasMeasured;
        public void Configure(QuantumEncounter owner, int index, Transform floatingCore,
            LineRenderer wave, LineRenderer[] rings, LineRenderer probability, LineRenderer halo, Transform[] particles)
        {
            encounter = owner; candidateIndex = index; core = floatingCore;
            coreRenderer = core.GetComponent<Renderer>(); waveform = wave; orbits = rings;
            probabilityArc = probability; lockHalo = halo; motes = particles;
        }

        public void Interact(PlayerInteraction player) { if (encounter != null) encounter.InspectCandidate(candidateIndex); }

        private void LateUpdate()
        {
            if (encounter == null || !encounter.IsInitialized || core == null) return;
            EnsureVisualCaches();
            if (measurementBeam == null) measurementBeam = QuantumPresentationEffects.Beam(transform, lockHalo.sharedMaterial);
            var state = encounter.GetCandidate(candidateIndex);
            if (runVersion != encounter.RunVersion)
            {
                runVersion = encounter.RunVersion; visibility = 0f; wasScanned = false;
                previousAmplitude = (float)state.Amplitude; previousIteration = encounter.IterationCount;
                phaseAt = unstableAt = -100f;
                magnitude = (float)System.Math.Abs(state.Amplitude); wasMeasured = false; clock = 0f;
            }
            bool measured = encounter.HasMeasured;
            bool survivor = measured && encounter.MeasuredCandidateIndex == candidateIndex;
            bool collapsed = measured && !survivor;
            Color stateColor = survivor ? QuantumPresentationEffects.MeasuredColor(encounter.MeasurementSucceeded) : Cyan;
            if (encounter.HasScanned && !wasScanned) scanAt = Time.time;
            wasScanned = encounter.HasScanned;
            float acquisition = !wasScanned ? 0f : Mathf.SmoothStep(0, 1, Mathf.InverseLerp(candidateIndex * .22f / encounter.CandidateCount, .36f + candidateIndex * .22f / encounter.CandidateCount, Time.time - scanAt));
            if (!measured && encounter.State == QuantumEncounterState.Marked && previousAmplitude * state.Amplitude < 0) phaseAt = Time.time;
            if (encounter.IterationCount != previousIteration && encounter.LastAmplificationOvershot) unstableAt = Time.time;
            previousAmplitude = (float)state.Amplitude; previousIteration = encounter.IterationCount;
            float phase = Mathf.Clamp01(1f - (Time.time - phaseAt) / .35f);
            float instability = Mathf.Clamp01(1f - (Time.time - unstableAt) / .5f);
            float anticipation = QuantumPresentationEffects.Anticipation(encounter);
            if (measured && !wasMeasured) lockAge = 0f;
            wasMeasured = measured;
            lockAge += Time.deltaTime;
            clock += Time.deltaTime * QuantumPresentationEffects.MotionRate(encounter);
            float sign = state.Amplitude < 0.0 ? -1f : 1f;
            magnitude = Mathf.MoveTowards(magnitude, (float)System.Math.Abs(state.Amplitude), Time.deltaTime * 2.6f);
            visibility = Mathf.MoveTowards(visibility, collapsed ? 0f : acquisition, Time.deltaTime * (collapsed ? 4.5f : 8f));
            float strength = magnitude * visibility;
            pedestalProperties.SetColor("_EmissionColor", stateColor * (.08f + strength * 2f));
            foreach (Renderer light in pedestalLights) if (light != null) light.SetPropertyBlock(pedestalProperties);
            float height = 1.3f + magnitude * 0.6f;
            core.gameObject.SetActive(visibility > 0.001f);
            core.localPosition = new Vector3(0f, Mathf.Lerp(0.4f, height + Mathf.Sin(clock * 1.8f) * 0.065f, visibility), 0f);
            core.localRotation = Quaternion.Euler(12f, clock * 24f * sign, 8f);
            core.localScale = Vector3.one * ((0.16f + 0.64f * magnitude) * visibility);
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", new Color(stateColor.r, stateColor.g, stateColor.b, (0.10f + magnitude * 0.8f) * visibility));
            properties.SetColor("_EmissionColor", stateColor * (0.15f + 2f * strength));
            coreRenderer.SetPropertyBlock(properties);

            waveform.enabled = visibility > 0.001f;
            waveform.positionCount = wavePoints.Length;
            for (int i = 0; i < wavePoints.Length; i++)
            {
                float x = i / (float)(wavePoints.Length - 1) * 2f - 1f;
                // Sign reverses travel and mirrors the waveform; MARK does not change magnitude.
                wavePoints[i] = new Vector3(x * visibility, Mathf.Lerp(.4f, 1.35f, visibility) + sign * strength * 0.72f * Mathf.Sin(x * 6.28f - clock * 2.8f * sign), -0.45f * visibility);
            }
            waveform.SetPositions(wavePoints);
            Tint(waveform, stateColor, (0.20f + magnitude * 0.8f) * visibility);
            for (int i = 0; i < orbits.Length; i++)
            {
                LineRenderer ring = orbits[i];
                ring.enabled = visibility > 0.001f;
                ring.transform.localPosition = new Vector3(0f, Mathf.Lerp(.4f, height, acquisition), 0f);
                ring.transform.localRotation = Quaternion.Euler((i == 0 ? 0f : 64f) + instability * Mathf.Sin(clock * 35f + i) * 13f, clock * (i == 1 ? -20f : 15f) * sign + phase * sign * 90f, i == 2 ? 55f : 0f);
                ring.transform.localScale = Vector3.one * (0.5f + 0.55f * magnitude) * visibility;
                Tint(ring, Color.Lerp(i == 2 ? Violet : Cyan, Violet, instability), (0.12f + 0.70f * magnitude) * visibility);
            }
            int visibleMotes = Mathf.CeilToInt(motes.Length * strength);
            for (int i = 0; i < motes.Length; i++)
            {
                bool active = i < visibleMotes && visibility > 0.01f;
                motes[i].gameObject.SetActive(active);
                if (!active) continue;
                float t = Mathf.Repeat(i / (float)motes.Length + clock * 0.22f * sign, 1f);
                float angle = t * Mathf.PI * 4f;
                motes[i].localPosition = new Vector3(Mathf.Cos(angle) * 0.64f, 0.45f + t * (0.7f + magnitude * 1.8f), Mathf.Sin(angle) * 0.64f);
                if (collapsed) motes[i].localPosition = Vector3.Lerp(Vector3.up * .4f, motes[i].localPosition, visibility);
                motes[i].localScale = Vector3.one * (0.045f + 0.035f * magnitude) * visibility;
            }
            float probability = (float)state.Probability;
            probabilityArc.enabled = probability > 0.0001f && visibility > 0.001f;
            probabilityArc.positionCount = 49;
            for (int i = 0; i < 49; i++)
            {
                float a = i / 48f * Mathf.PI * 2f * probability;
                probabilityArc.SetPosition(i, new Vector3(Mathf.Sin(a) * 1.05f, 0.38f, Mathf.Cos(a) * 1.05f));
            }
            probabilityArc.transform.localScale = Vector3.one * (1f + anticipation * .15f);
            Tint(probabilityArc, stateColor, visibility * (.65f + anticipation * .35f));
            lockHalo.enabled = survivor || phase > 0f || instability > 0f;
            if (!survivor && lockHalo.enabled)
            {
                float ripple = Mathf.Max(phase, instability);
                lockHalo.transform.localScale = Vector3.one * (.5f + (1f - ripple) * 2f);
                Tint(lockHalo, Violet, ripple * .8f);
            }
            if (survivor)
            {
                float settle = 1f - Mathf.Clamp01(lockAge / 0.65f);
                lockHalo.transform.localScale = Vector3.one * (1f + 0.8f * (1f - settle));
                Tint(lockHalo, stateColor, 0.18f + settle * 0.82f);
            }
            QuantumPresentationEffects.DrawBeam(measurementBeam, survivor, lockAge, encounter.MeasurementSucceeded);
        }

        private static void Tint(LineRenderer line, Color tint, float alpha)
        {
            tint.a = alpha; line.startColor = tint; line.endColor = tint;
        }
    }
}
