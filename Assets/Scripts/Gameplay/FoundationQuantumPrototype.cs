using System;
using System.Globalization;
using System.Text;
using NullSignal.Quantum;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NullSignal.Gameplay
{
    /// <summary>Temporary scene adapter. Presentation never owns or calculates quantum state.</summary>
    [DisallowMultipleComponent]
    public sealed class FoundationQuantumPrototype : MonoBehaviour
    {
        [SerializeField] private int candidateCount = 4;
        [SerializeField] private int targetIndex = 2;

        public AmplitudeAmplificationSystem Simulation { get; private set; }

        public void Configure(int count, int target)
        {
            // Validate configuration without exposing a mutable scenario to the scene.
            var scenario = new GroverSearchScenario(count, target);
            candidateCount = scenario.CandidateCount;
            targetIndex = scenario.TargetIndex;
            if (Application.isPlaying)
                Simulation = new AmplitudeAmplificationSystem(scenario);
        }

        private void Awake()
        {
            try
            {
                Simulation = new AmplitudeAmplificationSystem(new GroverSearchScenario(candidateCount, targetIndex));
            }
            catch (ArgumentException exception)
            {
                Debug.LogError("Invalid foundation quantum configuration: " + exception.Message, this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (Simulation == null)
                return;
            Debug.Log("NULL SIGNAL foundation: WASD/arrows move, Space dodge, E inspect nearby candidate. " +
                "Q SCAN, 1 MARK, 2 AMPLIFY, 3 LOCK, R reset. Results appear in the Console.", this);
            LogState("Initial uniform state");
        }

        private void Update()
        {
            if (Simulation == null || Time.timeScale <= 0f)
                return;
            try
            {
                if (ResetPressed())
                {
                    Simulation.Reset();
                    LogState("Reset");
                    return;
                }
                if (ScanPressed())
                    LogState("SCAN");
                if (MarkPressed())
                {
                    Simulation.ApplyOracle();
                    LogState("MARK: phase inversion; probabilities unchanged");
                }
                if (AmplifyPressed())
                {
                    Simulation.ApplyDiffusion();
                    LogState("AMPLIFY: reflection about the mean");
                }
                if (LockPressed())
                {
                    int measured = Simulation.Measure();
                    LogState($"LOCK: sampled candidate {measured + 1}, then collapsed the state");
                }
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogWarning(exception.Message, this);
            }
        }

        public void InspectCandidate(int index)
        {
            if (Simulation == null || index < 0 || index >= Simulation.CandidateCount)
                return;
            QuantumCandidate candidate = Simulation.GetCandidate(index);
            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "Candidate {0}: amplitude {1:F6}, probability {2:P2}. Inspection does not measure the state.",
                index + 1, candidate.Amplitude, candidate.Probability), this);
        }

        private void LogState(string label)
        {
            var text = new StringBuilder(label);
            text.Append(" | completed iterations: ").Append(Simulation.IterationCount);
            for (int i = 0; i < Simulation.CandidateCount; i++)
            {
                QuantumCandidate candidate = Simulation.GetCandidate(i);
                text.AppendFormat(CultureInfo.InvariantCulture, "\n  {0}: a={1:F6}, P={2:P2}",
                    i + 1, candidate.Amplitude, candidate.Probability);
            }
            text.AppendFormat(CultureInfo.InvariantCulture, "\n  probability sum={0:F12}", Simulation.ProbabilitySum);
            Debug.Log(text.ToString(), this);
        }

        private static bool ScanPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Q);
#else
            return false;
#endif
        }

        private static bool MarkPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Alpha1);
#else
            return false;
#endif
        }

        private static bool AmplifyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Alpha2);
#else
            return false;
#endif
        }

        private static bool LockPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.digit3Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Alpha3);
#else
            return false;
#endif
        }

        private static bool ResetPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.R);
#else
            return false;
#endif
        }
    }
}
