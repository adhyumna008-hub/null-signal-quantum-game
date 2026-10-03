using System;
using System.Collections.Generic;

namespace NullSignal.Quantum
{
    /// <summary>Executable checks, usable both outside Unity and from the editor.</summary>
    public static class QuantumValidation
    {
        public const double Tolerance = 1e-10;

        public static void RequireFiniteState(double[] amplitudes)
        {
            if (amplitudes == null)
                throw new ArgumentNullException(nameof(amplitudes));
            if (amplitudes.Length == 0)
                throw new ArgumentException("A state must contain candidates.", nameof(amplitudes));
            for (int i = 0; i < amplitudes.Length; i++)
                if (double.IsNaN(amplitudes[i]) || double.IsInfinity(amplitudes[i]))
                    throw new ArgumentException("All amplitudes must be finite.", nameof(amplitudes));
        }

        public static double ProbabilitySum(double[] amplitudes)
        {
            RequireFiniteState(amplitudes);
            double sum = 0.0;
            for (int i = 0; i < amplitudes.Length; i++)
                sum += amplitudes[i] * amplitudes[i];
            return sum;
        }

        public static IReadOnlyList<string> RunAll()
        {
            var passed = new List<string>();
            foreach (int count in new[] { 4, 8, 16 })
            {
                for (int target = 0; target < count; target++)
                    ValidateScenario(count, target);
                ValidateSampling(count);
                passed.Add($"N={count}: all target positions, normalization, phase, reflection, amplification, overshoot and sampling passed.");
            }

            // The sampler has no target input; asymmetric signed states still sample a^2.
            double[] asymmetric = { -Math.Sqrt(0.1), Math.Sqrt(0.2), -Math.Sqrt(0.3), Math.Sqrt(0.4) };
            Check(MeasurementSystem.Sample(asymmetric, 0.05) == 0, "First CDF interval");
            Check(MeasurementSystem.Sample(asymmetric, 0.2) == 1, "Second CDF interval");
            Check(MeasurementSystem.Sample(asymmetric, 0.45) == 2, "Third CDF interval");
            Check(MeasurementSystem.Sample(asymmetric, 0.8) == 3, "Fourth CDF interval");
            Check(MeasurementSystem.Sample(new[] { 0.0, -1.0, 0.0, 0.0 }, 0.0) == 1, "Zero mass at the first boundary");
            Check(MeasurementSystem.Sample(new[] { 0.0, -1.0, 0.0, 0.0 }, 0.999999999) == 1, "Zero mass at the last boundary");
            ExpectException<ArgumentException>(() => MeasurementSystem.Sample(new[] { 0.5, 0.5 }, 0.2));
            ExpectException<ArgumentException>(() => MeasurementSystem.Sample(new[] { double.NaN }, 0.2));
            ExpectException<ArgumentOutOfRangeException>(() => MeasurementSystem.Sample(new[] { 1.0 }, 1.0));
            ExpectException<ArgumentOutOfRangeException>(() => new GroverSearchScenario(3, 0));
            ExpectException<ArgumentOutOfRangeException>(() => new GroverSearchScenario(4, 4));
            passed.Add("CDF boundaries, signed asymmetric states and invalid input checks passed.");
            return passed.AsReadOnly();
        }

        private static void ValidateScenario(int count, int target)
        {
            var system = new AmplitudeAmplificationSystem(new GroverSearchScenario(count, target), new Random(1234));
            double initial = 1.0 / Math.Sqrt(count);
            Close(system.ProbabilitySum, 1.0, "Initial normalization");
            for (int i = 0; i < count; i++)
                Close(system.GetCandidate(i).Amplitude, initial, "Uniform initialization");

            double[] before = system.GetAmplitudes();
            system.ApplyOracle();
            for (int i = 0; i < count; i++)
            {
                Close(system.GetCandidate(i).Amplitude, i == target ? -before[i] : before[i], "Oracle phase inversion");
                Close(system.GetCandidate(i).Probability, before[i] * before[i], "Oracle preserves probability");
            }
            Close(system.ProbabilitySum, 1.0, "Oracle normalization");
            ExpectException<InvalidOperationException>(() => system.ApplyOracle());

            double[] marked = system.GetAmplitudes();
            double mean = 0.0;
            foreach (double amplitude in marked)
                mean += amplitude / count;
            system.ApplyDiffusion();
            for (int i = 0; i < count; i++)
                Close(system.GetCandidate(i).Amplitude, 2.0 * mean - marked[i], "Diffusion reflects original mean");
            double[] reflected = system.GetAmplitudes();
            DiffusionOperator.Apply(reflected);
            for (int i = 0; i < count; i++)
                Close(reflected[i], marked[i], "Diffusion is an involution");
            OracleOperator.Apply(marked, target);
            for (int i = 0; i < count; i++)
                Close(marked[i], before[i], "Oracle is an involution");

            system.Reset();
            ExpectException<InvalidOperationException>(() => system.ApplyDiffusion());
            double theta = Math.Asin(initial);
            bool sawAmplification = false;
            bool sawOvershoot = false;
            double previous = system.TargetProbability;
            for (int iteration = 1; iteration <= 64; iteration++)
            {
                system.ApplyIteration();
                // Independent closed-form result verifies the implemented recurrence.
                double sine = Math.Sin((2 * iteration + 1) * theta);
                Close(system.TargetProbability, sine * sine, "Grover closed-form agreement");
                Close(system.ProbabilitySum, 1.0, "Repeated-iteration normalization");
                Check(system.IterationCount == iteration, "Completed iteration count");
                sawAmplification |= system.TargetProbability > 1.0 / count + Tolerance;
                sawOvershoot |= previous > 1.0 / count + Tolerance && system.TargetProbability < previous - Tolerance;
                previous = system.TargetProbability;
            }
            Check(sawAmplification && sawOvershoot, "Natural amplification and overshooting");

            double saved = system.GetCandidate(0).Amplitude;
            double[] snapshot = system.GetAmplitudes();
            snapshot[0] = 42.0;
            Close(system.GetCandidate(0).Amplitude, saved, "Snapshot isolation");
            int measured = system.Measure();
            Check(measured >= 0 && measured < count, "Measurement range");
            for (int i = 0; i < count; i++)
                Close(system.GetCandidate(i).Probability, i == measured ? 1.0 : 0.0, "Measurement collapse");
            Check(system.Measure() == measured, "Repeated measurement after collapse");
            ExpectException<InvalidOperationException>(() => system.ApplyIteration());
            system.Reset();
            Check(!system.HasMeasured && system.IterationCount == 0 && !system.IsOracleApplied, "Reset state");
        }

        private static void ValidateSampling(int count)
        {
            var random = new Random(8719 + count);
            var system = new AmplitudeAmplificationSystem(new GroverSearchScenario(count, count - 1));
            // Both uniform and amplified distributions must sample actual calculated weights.
            for (int phase = 0; phase < 2; phase++)
            {
                if (phase == 1)
                    system.ApplyIteration();
                double[] amplitudes = system.GetAmplitudes();
                const int trials = 20000;
                var frequencies = new int[count];
                for (int trial = 0; trial < trials; trial++)
                    frequencies[MeasurementSystem.Sample(amplitudes, random)]++;
                for (int i = 0; i < count; i++)
                {
                    double probability = system.GetCandidate(i).Probability;
                    double observed = (double)frequencies[i] / trials;
                    double bound = Math.Max(0.005, 6.0 * Math.Sqrt(probability * (1.0 - probability) / trials));
                    Check(Math.Abs(observed - probability) <= bound, "Seeded sampling follows calculated probability");
                    if (probability == 0.0)
                        Check(frequencies[i] == 0, "Zero-probability candidates never sampled");
                }
            }
        }

        private static void Close(double actual, double expected, string description)
        {
            Check(!double.IsNaN(actual) && !double.IsInfinity(actual) && Math.Abs(actual - expected) <= Tolerance,
                $"{description}: expected {expected:R}, got {actual:R}");
        }

        private static void Check(bool condition, string description)
        {
            if (!condition)
                throw new InvalidOperationException("Quantum validation failed: " + description);
        }

        private static void ExpectException<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            throw new InvalidOperationException("Quantum validation expected " + typeof(T).Name);
        }
    }
}
