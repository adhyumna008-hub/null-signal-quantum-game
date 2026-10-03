namespace NullSignal.Quantum
{
    /// <summary>An immutable snapshot; probability is always amplitude squared.</summary>
    public readonly struct QuantumCandidate
    {
        public QuantumCandidate(int index, double amplitude)
        {
            Index = index;
            Amplitude = amplitude;
        }

        public int Index { get; }
        public double Amplitude { get; }
        public double Probability => Amplitude * Amplitude;
        public bool HasInvertedPhase => Amplitude < 0.0;
    }
}
