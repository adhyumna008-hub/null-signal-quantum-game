using System;

namespace NullSignal.Quantum
{
    /// <summary>One marked state in the supported canonical search spaces.</summary>
    public sealed class GroverSearchScenario
    {
        public GroverSearchScenario(int candidateCount, int targetIndex)
        {
            if (!IsSupported(candidateCount))
                throw new ArgumentOutOfRangeException(nameof(candidateCount), "Supported sizes are 4, 8 and 16.");
            if (targetIndex < 0 || targetIndex >= candidateCount)
                throw new ArgumentOutOfRangeException(nameof(targetIndex));

            CandidateCount = candidateCount;
            TargetIndex = targetIndex;
        }

        public int CandidateCount { get; }
        public int TargetIndex { get; }

        public static bool IsSupported(int count) => count == 4 || count == 8 || count == 16;
    }
}
