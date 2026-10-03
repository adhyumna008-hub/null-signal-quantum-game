using System;
using UnityEngine;

namespace NullSignal.Gameplay
{
    /// <summary>Times presentation around one call to the original encounter operator.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(AnveshController))]
    public sealed class AnveshOperationController : MonoBehaviour
    {
        private AnveshController controller;
        private QuantumEncounter encounter;
        private double[] previous;
        private QuantumEncounterState previousState;
        private int version, iteration;
        private float elapsed, duration;
        private bool committed;
        public bool Busy { get; private set; }
        public AnveshAbility Action { get; private set; }
        public double[] Before { get; private set; }
        public double[] After { get; private set; }
        public int Target { get; private set; }
        public int? Measured { get; private set; }
        public double Mean { get; private set; }
        public double TargetProbability { get; private set; }
        public bool Overshot { get; private set; }
        public float Progress => Busy ? Mathf.Clamp01(elapsed / duration) : 1f;
        public bool Committed => committed;
        public float ReportAt { get; private set; } = -100f;
        public float DataAt { get; private set; } = -100f;
        public event System.Action Changed;

        private void OnEnable()
        {
            controller = GetComponent<AnveshController>();
            controller.ContextChanged += Bind;
            Bind();
        }
        private void OnDisable()
        {
            Cancel();
            if (controller != null) controller.ContextChanged -= Bind;
            if (encounter != null) encounter.Changed -= Observe;
            if (encounter != null && encounter.PresentationOperations == this) encounter.PresentationOperations = null;
        }
        private void Bind()
        {
            if (encounter != null) encounter.Changed -= Observe;
            if (encounter != null && encounter.PresentationOperations == this) encounter.PresentationOperations = null;
            encounter = controller.Encounter; Cancel();
            if (encounter != null) { encounter.PresentationOperations = this; encounter.Changed += Observe; Snapshot(); }
        }
        private void Snapshot()
        {
            if (encounter == null || !encounter.IsInitialized) return;
            previous = encounter.GetAmplitudes(); previousState = encounter.State;
            version = encounter.RunVersion; iteration = encounter.IterationCount;
        }
        public bool Begin(AnveshAbility action)
        {
            if (Busy || encounter == null || !encounter.IsInitialized) return false;
            Action = action; Capture(encounter.GetAmplitudes());
            duration = action == AnveshAbility.Scan ? .8f : action == AnveshAbility.Mark ? .7f : action == AnveshAbility.Amplify ? 1f : 1.1f;
            elapsed = 0f; committed = false; Busy = true; ReportAt = Time.unscaledTime;
            controller.RefreshAvailability(); Changed?.Invoke(); return true;
        }
        private void Capture(double[] before)
        {
            Before = before; After = null; Target = encounter.TargetIndex; Measured = null; Overshot = false;
            Mean = 0; foreach (double value in before) Mean += value; Mean /= before.Length;
            TargetProbability = before[Target] * before[Target];
        }
        private void Update()
        {
            if (!Busy) return;
            if (encounter == null || encounter.RunVersion != version) { Cancel(); Snapshot(); return; }
            elapsed += Time.deltaTime;
            float commitFraction = Action == AnveshAbility.Scan ? .25f : Action == AnveshAbility.Lock ? .35f : .45f;
            if (!committed && elapsed >= duration * commitFraction)
            {
                committed = true;
                if (!Apply(encounter, Action)) { Cancel(); return; }
            }
            if (elapsed >= duration) { Busy = false; ReportAt = Time.unscaledTime; controller.RefreshAvailability(); Changed?.Invoke(); }
        }
        private void Observe()
        {
            if (encounter.RunVersion != version || previous == null) { Cancel(); Snapshot(); return; }
            if (!Busy)
            {
                AnveshAbility observed = encounter.HasMeasured && previousState != QuantumEncounterState.Resolved ? AnveshAbility.Lock
                    : encounter.IterationCount > iteration ? AnveshAbility.Amplify
                    : encounter.State == QuantumEncounterState.Marked && previousState != QuantumEncounterState.Marked ? AnveshAbility.Mark
                    : previousState == QuantumEncounterState.Unscanned && encounter.HasScanned ? AnveshAbility.Scan : AnveshAbility.None;
                if (observed == AnveshAbility.None) { Snapshot(); return; }
                Action = observed; Capture(previous); committed = true; ReportAt = Time.unscaledTime;
            }
            After = encounter.GetAmplitudes(); Measured = encounter.MeasuredCandidateIndex;
            DataAt = Time.unscaledTime;
            Overshot = Action == AnveshAbility.Amplify && encounter.LastAmplificationOvershot;
            Snapshot(); Changed?.Invoke();
        }
        public void Cancel()
        {
            Busy = false; Action = AnveshAbility.None; ReportAt = -100f;
            controller?.RefreshAvailability(); Changed?.Invoke();
        }
        public static bool Apply(QuantumEncounter search, AnveshAbility action)
        {
            switch (action)
            {
                case AnveshAbility.Scan: return search.Scan();
                case AnveshAbility.Mark: return search.Mark();
                case AnveshAbility.Amplify: return search.Amplify();
                case AnveshAbility.Lock: return search.Lock();
                default: return false;
            }
        }
    }
}
