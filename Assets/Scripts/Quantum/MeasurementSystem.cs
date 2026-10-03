using System;

namespace NullSignal.Quantum
{
    public static class MeasurementSystem
    {
        public static int Sample(double[] amplitudes, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            return Sample(amplitudes, random.NextDouble());
        }

        /// <summary>Inverse CDF sampling. unitSample must be in [0, 1).</summary>
        public static int Sample(double[] amplitudes, double unitSample)
        {
            QuantumValidation.RequireFiniteState(amplitudes);
            if (double.IsNaN(unitSample) || unitSample < 0.0 || unitSample >= 1.0)
                throw new ArgumentOutOfRangeException(nameof(unitSample));

            double total = QuantumValidation.ProbabilitySum(amplitudes);
            if (Math.Abs(total - 1.0) > QuantumValidation.Tolerance)
                throw new ArgumentException("Measurement requires a normalized state.", nameof(amplitudes));

            // Scaling by total accounts only for floating-point summation drift.
            // It does not change the relative probabilities or amplify the target.
            double threshold = unitSample * total;
            double cumulative = 0.0;
            int lastPositive = -1;
            for (int i = 0; i < amplitudes.Length; i++)
            {
                double probability = amplitudes[i] * amplitudes[i];
                if (probability > 0.0)
                    lastPositive = i;
                cumulative += probability;
                if (threshold < cumulative)
                    return i;
            }

            // Rounding at the final boundary must never select a zero-mass state.
            return lastPositive;
        }
    }
}
