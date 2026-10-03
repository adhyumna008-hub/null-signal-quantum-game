using System;

namespace NullSignal.Quantum
{
    /// <summary>Pure C# state and operators; no Unity or presentation dependency.</summary>
    public sealed class AmplitudeAmplificationSystem
    {
        private readonly double[] amplitudes;
        private readonly Random random;

        public AmplitudeAmplificationSystem(GroverSearchScenario scenario, Random random = null)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            this.random = random ?? new Random();
            amplitudes = new double[scenario.CandidateCount];
            Reset();
        }

        public GroverSearchScenario Scenario { get; }
        public int CandidateCount => amplitudes.Length;
        public int IterationCount { get; private set; }
        public bool IsOracleApplied { get; private set; }
        public int? LastMeasurementIndex { get; private set; }
        public bool HasMeasured => LastMeasurementIndex.HasValue;
        public double TargetProbability => GetCandidate(Scenario.TargetIndex).Probability;
        public double ProbabilitySum => QuantumValidation.ProbabilitySum(amplitudes);
        public bool IsNormalized => Math.Abs(ProbabilitySum - 1.0) <= QuantumValidation.Tolerance;

        public int HighestProbabilityIndex
        {
            get
            {
                int best = 0;
                for (int i = 1; i < CandidateCount; i++)
                    if (GetCandidate(i).Probability > GetCandidate(best).Probability)
                        best = i;
                return best;
            }
        }

        public void Reset()
        {
            double initial = 1.0 / Math.Sqrt(CandidateCount);
            for (int i = 0; i < CandidateCount; i++)
                amplitudes[i] = initial;
            IterationCount = 0;
            IsOracleApplied = false;
            LastMeasurementIndex = null;
        }

        public QuantumCandidate GetCandidate(int index)
        {
            if (index < 0 || index >= CandidateCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return new QuantumCandidate(index, amplitudes[index]);
        }

        // Copies protect the simulation from accidental view-layer mutation.
        public double[] GetAmplitudes() => (double[])amplitudes.Clone();

        public double[] GetProbabilities()
        {
            var probabilities = new double[CandidateCount];
            for (int i = 0; i < CandidateCount; i++)
                probabilities[i] = GetCandidate(i).Probability;
            return probabilities;
        }

        public void ApplyOracle()
        {
            RequireUnmeasured();
            if (IsOracleApplied)
                throw new InvalidOperationException("Complete AMPLIFY before marking again.");
            OracleOperator.Apply(amplitudes, Scenario.TargetIndex);
            IsOracleApplied = true;
        }

        public void ApplyDiffusion()
        {
            RequireUnmeasured();
            if (!IsOracleApplied)
                throw new InvalidOperationException("Apply MARK before AMPLIFY; every iteration needs both operations.");
            DiffusionOperator.Apply(amplitudes);
            IsOracleApplied = false;
            IterationCount++;
        }

        public void ApplyIteration()
        {
            ApplyOracle();
            ApplyDiffusion();
        }

        public int Measure()
        {
            if (HasMeasured)
                return LastMeasurementIndex.Value;

            int result = MeasurementSystem.Sample(amplitudes, random);
            for (int i = 0; i < CandidateCount; i++)
                amplitudes[i] = i == result ? 1.0 : 0.0;
            LastMeasurementIndex = result;
            IsOracleApplied = false;
            return result;
        }

        private void RequireUnmeasured()
        {
            if (HasMeasured)
                throw new InvalidOperationException("Reset the search after measurement before starting another iteration.");
        }
    }
}
