using UnityEngine;
using NullSignal.Presentation;

namespace NullSignal.Gameplay
{
    /// <summary>Moving presentation and telegraphed threats. Amplitudes are read, never synthesized.</summary>
    public sealed class CombatManifestation : MonoBehaviour
    {
        [SerializeField] private QuantumCombatEncounter owner;
        [SerializeField] private int index;
        [SerializeField] private Transform body, sensor, shield;
        [SerializeField] private Transform[] fragments, motes;
        [SerializeField] private LineRenderer waveform, telegraph, beam;
        [SerializeField] private Renderer[] energy;
        private Vector3[] fragmentPositions;
        private MaterialPropertyBlock properties;
        private float nextAttack, fireAt, beamUntil, hitUntil, visibility, magnitude, collapseAt = -100;
        private int version = -1;
        private Vector3 strikePoint;
        private bool charging, wasMeasured;
        private float visualClock, scanAt, previousAmplitude, phaseAt = -100f, unstableAt = -100f;
        private int previousIteration;
        private bool wasScanned;
        private LineRenderer measurementBeam;
        private static readonly Color Cyan = new Color(.2f, .85f, 1f), Red = new Color(1f, .16f, .1f), Violet = new Color(.65f, .28f, 1f);
        public QuantumCombatEncounter Owner => owner;
        public bool CanBeAimed => owner.Running && index < owner.Search.CandidateCount && visibility > .15f && (!owner.Search.HasMeasured || owner.Search.MeasuredCandidateIndex == index);
        public Vector3 AimPoint => body.position + Vector3.up * .25f;
        public void Configure(QuantumCombatEncounter encounter, int state, Transform model, Transform eye, Transform barrier,
            Transform[] pieces, Transform[] particles, LineRenderer wave, LineRenderer warning, LineRenderer shot, Renderer[] emissives)
        { owner = encounter; index = state; body = model; sensor = eye; shield = barrier; fragments = pieces; motes = particles; waveform = wave; telegraph = warning; beam = shot; energy = emissives; }
        private void Awake()
        {
            properties = new MaterialPropertyBlock(); fragmentPositions = new Vector3[fragments.Length];
            measurementBeam = QuantumPresentationEffects.Beam(transform, waveform.sharedMaterial);
            for (int i = 0; i < fragments.Length; i++) fragmentPositions[i] = fragments[i].localPosition;
        }
        public void ResetThreat() { ClearAttack(); nextAttack = Time.time + 1.6f + index * .48f; }
        public void ClearAttack() { charging = false; if (telegraph != null) telegraph.enabled = false; if (beam != null) beam.enabled = false; beamUntil = 0; }
        public bool ReceiveHit(float damage)
        {
            if (!owner.Active || index >= owner.Search.CandidateCount || visibility < .2f) return false;
            bool applied = owner.Damage(index, damage); hitUntil = Time.time + (applied ? .17f : .07f); return applied;
        }
        private void Update()
        {
            QuantumEncounter search = owner.Search;
            if (!owner.Active || !search.IsInitialized || index >= search.CandidateCount)
            { body.gameObject.SetActive(false); shield.gameObject.SetActive(false); waveform.enabled = false; measurementBeam.enabled = false; ClearAttack(); return; }
            body.gameObject.SetActive(true); shield.gameObject.SetActive(true); waveform.enabled = true;
            if (version != search.RunVersion) { version = search.RunVersion; visibility = 0; wasMeasured = wasScanned = false; magnitude = 1f / Mathf.Sqrt(search.CandidateCount); previousAmplitude = magnitude; previousIteration = 0; phaseAt = unstableAt = -100f; ResetThreat(); }
            visualClock += Time.deltaTime * QuantumPresentationEffects.MotionRate(search);
            if (search.HasScanned && !wasScanned) scanAt = Time.time;
            wasScanned = search.HasScanned;
            float acquisition = !wasScanned ? 0f : Mathf.SmoothStep(0, 1, Mathf.InverseLerp(index * .22f / search.CandidateCount, .36f + index * .22f / search.CandidateCount, Time.time - scanAt));
            bool survivor = !search.HasMeasured || search.MeasuredCandidateIndex == index;
            if (search.HasMeasured && !wasMeasured) collapseAt = Time.time;
            wasMeasured = search.HasMeasured;
            visibility = Mathf.MoveTowards(visibility, survivor ? owner.Formation : 0f, Time.deltaTime * 3f);
            float signed = (float)search.GetCandidate(index).Amplitude;
            if (!search.HasMeasured && search.State == QuantumEncounterState.Marked && previousAmplitude * signed < 0) phaseAt = Time.time;
            if (search.IterationCount != previousIteration && search.LastAmplificationOvershot) unstableAt = Time.time;
            previousAmplitude = signed; previousIteration = search.IterationCount;
            float phase = Mathf.Clamp01(1 - (Time.time - phaseAt) / .35f);
            float instability = Mathf.Clamp01(1 - (Time.time - unstableAt) / .5f);
            magnitude = Mathf.MoveTowards(magnitude, Mathf.Abs(signed), Time.deltaTime * 2.3f);
            float sign = signed < 0 ? -1f : 1f;
            float angle = index * Mathf.PI * 2f / search.CandidateCount + visualClock * .12f;
            float radius = owner.OrbitRadius;
            float intro = owner.IntroProgress;
            bool transforming = owner.Prime && owner.Introducing;
            if (transforming)
            {
                radius = intro < .22f ? Mathf.Lerp(radius, .35f, intro / .22f)
                    : intro < .7f ? .6f + Mathf.Sin((intro - .22f) / .48f * Mathf.PI) * 1.7f
                    : Mathf.Lerp(.6f, radius, Mathf.SmoothStep(0, 1, (intro - .7f) / .3f));
                angle += Mathf.Sin(intro * Mathf.PI) * intro * 18f;
            }
            Vector3 orbit = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            // Actor transforms keep moving; quantum values do not depend on their world positions.
            transform.position = owner.Center + orbit;
            Vector3 towardPlayer = owner.Player.position - transform.position; towardPlayer.y = 0;
            if (towardPlayer.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(towardPlayer);
            body.localPosition = Vector3.up * (1.4f + .16f * Mathf.Sin(Time.time * 2f + index));
            float assemblyScale = transforming ? Mathf.Lerp(.3f, 1.1f, Mathf.Abs(intro * 2f - 1f)) : owner.Prime ? 1.1f : 1f;
            body.localScale = Vector3.one * visibility * assemblyScale * (.65f + .7f * magnitude);
            sensor.localRotation = Quaternion.Euler(0, 0, visualClock * 90f * sign + phase * sign * 90f);
            float pulse = Mathf.Clamp01((Time.time - collapseAt) / .6f);
            shield.localPosition = new Vector3(0, .08f, 0);
            shield.localScale = Vector3.one * visibility * (owner.Vulnerable ? 1.5f + Mathf.Sin(Time.time * 6f) * .08f : 1.1f + magnitude * .4f);
            shield.localScale *= acquisition * (1f + QuantumPresentationEffects.Anticipation(search) * .18f + phase * .18f);
            shield.localRotation = Quaternion.Euler(instability * Mathf.Sin(visualClock * 30f) * 15f, 0, 0);
            if (search.HasMeasured && pulse < 1f) shield.localScale *= 1f + pulse * 2.5f;
            Color tint = Time.time < hitUntil ? Color.white : owner.Vulnerable ? new Color(1f, .7f, .28f)
                : owner.HostileDecoy || charging ? (owner.Chhaya ? Violet : Red) : owner.Chhaya ? Violet : Cyan;
            properties.SetColor("_BaseColor", tint);
            properties.SetColor("_EmissionColor", tint * (.4f + magnitude * 2.2f));
            foreach (Renderer renderer in energy) renderer.SetPropertyBlock(properties);
            for (int i = 0; i < fragments.Length; i++)
            {
                float scatter = (1f - owner.Formation) * 2f;
                if (transforming) scatter = Mathf.Sin(intro * Mathf.PI) * 2.7f;
                fragments[i].localPosition = fragmentPositions[i] + new Vector3(Mathf.Sin(Time.time * 1.7f + i), Mathf.Cos(Time.time * 2f + i), Mathf.Sin(Time.time * 1.9f + i * 4f)) * (owner.Chhaya ? .06f + scatter : .025f);
                fragments[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2f + i) * (owner.Chhaya ? 9f : 4f));
            }
            for (int i = 0; i < motes.Length; i++)
            {
                motes[i].gameObject.SetActive(i < Mathf.CeilToInt(motes.Length * magnitude * acquisition) && visibility > .01f);
                float t = visualClock * sign * 1.8f + i * 2.4f;
                motes[i].localPosition = new Vector3(Mathf.Cos(t) * .8f, Mathf.Sin(t * .5f) * magnitude, Mathf.Sin(t) * .8f);
            }
            waveform.startColor = waveform.endColor = tint;
            waveform.widthMultiplier = .025f * visibility * acquisition;
            for (int i = 0; i < waveform.positionCount; i++)
            {
                float x = i / (float)(waveform.positionCount - 1) * 2f - 1f;
                waveform.SetPosition(i, new Vector3(x * visibility, 2.7f + Mathf.Sin(x * 6.28f - visualClock * sign * 4f) * magnitude * sign * .65f, 0));
            }
            QuantumPresentationEffects.DrawBeam(measurementBeam, search.HasMeasured && survivor, Time.time - collapseAt);
            if (!owner.CanThreaten(index)) { ClearAttack(); return; }
            if (!charging && Time.time >= nextAttack && owner.TryBeginThreat(index))
            {
                charging = true; fireAt = Time.time + (owner.HostileDecoy ? .7f : 1.05f);
                strikePoint = owner.Player.position; strikePoint.y = owner.Center.y + .06f;
            }
            if (charging)
            {
                telegraph.enabled = true; telegraph.startColor = telegraph.endColor = owner.Chhaya ? Violet : Red;
                for (int p = 0; p < telegraph.positionCount; p++)
                { float a = p * Mathf.PI * 2f / (telegraph.positionCount - 1); telegraph.SetPosition(p, strikePoint + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * 1.15f); }
                if (Time.time >= fireAt)
                {
                    charging = false; telegraph.enabled = false; owner.Strike(strikePoint); beamUntil = Time.time + .15f;
                    beam.SetPosition(0, AimPoint); beam.SetPosition(1, strikePoint); beam.startColor = beam.endColor = owner.Chhaya ? Violet : Red;
                    nextAttack = Time.time + (owner.HostileDecoy ? .5f : owner.Prime ? 7f - owner.Phase * .65f : owner.Chhaya ? 4.5f : 3.8f);
                }
            }
            beam.enabled = Time.time < beamUntil;
        }
    }
}
