using System;

namespace NullSignal.Quantum
{
    public static class OracleOperator
    {
        /// <summary>Phase marking does not change any measurement probability.</summary>
        public static void Apply(double[] amplitudes, int targetIndex)
        {
            QuantumValidation.RequireFiniteState(amplitudes);
            if (targetIndex < 0 || targetIndex >= amplitudes.Length)
                throw new ArgumentOutOfRangeException(nameof(targetIndex));

            amplitudes[targetIndex] = -amplitudes[targetIndex];
        }
    }
}
