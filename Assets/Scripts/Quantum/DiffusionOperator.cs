namespace NullSignal.Quantum
{
    public static class DiffusionOperator
    {
        public static void Apply(double[] amplitudes)
        {
            QuantumValidation.RequireFiniteState(amplitudes);
            double sum = 0.0;
            for (int i = 0; i < amplitudes.Length; i++)
                sum += amplitudes[i];

            // Compute the original mean once, before mutating any component.
            double twiceMean = 2.0 * sum / amplitudes.Length;
            for (int i = 0; i < amplitudes.Length; i++)
                amplitudes[i] = twiceMean - amplitudes[i];
        }
    }
}
