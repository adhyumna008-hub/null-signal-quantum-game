using System;
using NullSignal.Quantum;
using UnityEngine;

namespace NullSignal.Gameplay
{
    public enum QuantumEncounterState { Unscanned, Ready, Marked, Resolved }

    /// <summary>Reusable encounter rules around the unchanged Phase 1 simulation.</summary>
    [DisallowMultipleComponent]
    public sealed class QuantumEncounter : MonoBehaviour
    {
        [SerializeField] private int candidateCount = 4;
        [SerializeField] private int targetIndex;
        [SerializeField] private bool randomizeTargetOnRestart;

        private AmplitudeAmplificationSystem simulation;
        private System.Random random;

        public event Action Changed;
        public AnveshOperationController PresentationOperations { get; internal set; }
        public bool IsInitialized => simulation != null;
        public int CandidateCount => candidateCount;
        public int TargetIndex => simulation?.Scenario.TargetIndex ?? -1;
        public int IterationCount => simulation?.IterationCount ?? 0;
        public int RunVersion { get; private set; }
        public bool HasScanned { get; private set; }
        public bool HasMeasured => simulation != null && simulation.HasMeasured;
        public int? MeasuredCandidateIndex => simulation?.LastMeasurementIndex;
        public bool? MeasurementSucceeded => HasMeasured
            ? MeasuredCandidateIndex == simulation.Scenario.TargetIndex : (bool?)null;
        public double TargetProbability => simulation?.TargetProbability ?? 0.0;
        public double ProbabilitySum => simulation?.ProbabilitySum ?? 0.0;
        public double? ProbabilityBeforeLock { get; private set; }
        public bool LastAmplificationOvershot { get; private set; }
        public string StatusMessage { get; private set; } = "Press Q to SCAN the four candidates.";
        public bool CanScan => IsInitialized && !HasMeasured;
        public bool CanMark => CanScan && HasScanned && !simulation.IsOracleApplied;
        public bool CanAmplify => CanScan && HasScanned && simulation.IsOracleApplied;
        public bool CanLock => CanScan && HasScanned;
        public QuantumEncounterState State => HasMeasured ? QuantumEncounterState.Resolved
            : !HasScanned ? QuantumEncounterState.Unscanned
            : simulation.IsOracleApplied ? QuantumEncounterState.Marked : QuantumEncounterState.Ready;

        public void Configure(int count, int target, bool randomizeOnRestart = false)
        {
            var scenario = new GroverSearchScenario(count, target);
            candidateCount = scenario.CandidateCount;
            targetIndex = scenario.TargetIndex;
            randomizeTargetOnRestart = randomizeOnRestart;
        }

        private void Awake() => RestartEncounter();

        public void RestartEncounter(System.Random randomSource = null)
        {
            random = randomSource ?? random ?? new System.Random();
            int target = randomizeTargetOnRestart ? random.Next(candidateCount) : targetIndex;
            simulation = new AmplitudeAmplificationSystem(new GroverSearchScenario(candidateCount, target), random);
            HasScanned = false;
            ProbabilityBeforeLock = null;
            LastAmplificationOvershot = false;
            RunVersion++;
            Notify("Equal candidates. Press Q to SCAN.");
        }

        public QuantumCandidate GetCandidate(int index)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The encounter has not initialized.");
            return simulation.GetCandidate(index);
        }

        public double[] GetAmplitudes() => simulation.GetAmplitudes();
        public double[] GetProbabilities() => simulation.GetProbabilities();

        public bool Scan()
        {
            if (!CanScan)
                return Reject("Encounter resolved. Press R to restart.");
            HasScanned = true;
            Notify($"SCAN acquired {CandidateCount} candidates. MARK changes phase, not magnitude.");
            return true;
        }

        public bool Mark()
        {
            if (!CanMark)
                return Reject(!HasScanned ? "SCAN first with Q." : HasMeasured
                    ? "Encounter resolved. Press R to restart." : "AMPLIFY the pending MARK before marking again.");
            simulation.ApplyOracle();
            Notify("MARK: phase inverted. Watch the reversed wave and particle flow; magnitude is unchanged.");
            return true;
        }

        public bool Amplify()
        {
            if (!CanAmplify)
                return Reject(HasMeasured ? "Encounter resolved. Press R to restart." : "Use MARK before each AMPLIFY.");
            double previous = simulation.TargetProbability;
            simulation.ApplyDiffusion();
            LastAmplificationOvershot = simulation.TargetProbability < previous - QuantumValidation.Tolerance;
            Notify(LastAmplificationOvershot
                ? "Overshoot: the target became weaker. More iterations do not always help."
                : "AMPLIFY: amplitude redistributed. Compare the signal heights, then choose when to LOCK.");
            return true;
        }

        public bool Lock()
        {
            if (!CanLock)
                return Reject(HasMeasured ? "Encounter resolved. Press R to restart." : "SCAN first with Q.");
            ProbabilityBeforeLock = simulation.TargetProbability;
            int result = simulation.Measure(); // Uses Phase 1 MeasurementSystem; never selects the largest weight.
            Notify(MeasurementSucceeded == true
                ? $"LOCK SUCCESS — candidate {result + 1}. Target measured. Press R to restart."
                : $"LOCK MISSED — candidate {result + 1}. A different state was measured. Press R to retry.");
            return true;
        }

        public void InspectCandidate(int index)
        {
            QuantumCandidate candidate = GetCandidate(index);
            Notify($"Candidate {index + 1}: " + (candidate.HasInvertedPhase ? "reversed phase flow." : "forward phase flow.")
                + " Inspection does not measure or select a state.");
        }

        private bool Reject(string message) { Notify(message); return false; }
        private void Notify(string message) { StatusMessage = message; Changed?.Invoke(); }
    }
}
