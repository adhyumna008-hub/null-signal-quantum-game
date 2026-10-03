using System.Globalization;
using System.Text;
using NullSignal.Gameplay;
using NullSignal.Story;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    [DefaultExecutionOrder(100)]
    public sealed class AnveshHUD : MonoBehaviour
    {
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private AnveshController anvesh;
        [SerializeField] private Text objective, feedback, counts, warning, debugText;
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private Button[] buttons;
        [SerializeField] private Text[] stateLabels;
        [SerializeField] private AmplitudeScopeGraphic scope;
        [SerializeField] private ObjectiveSystem storyObjectives;
        private bool showDebug;
        private void LateUpdate()
        {
            if (warning == null || anvesh == null || anvesh.Operations == null) return;
            float age = Time.unscaledTime - anvesh.Operations.DataAt;
            float flash = anvesh.Operations.Overshot && age < .65f ? Mathf.Abs(Mathf.Sin(age * 18f)) : 0f;
            warning.color = Color.Lerp(new Color(.9f, .66f, .36f), new Color(1f, .35f, .55f), flash);
        }
        public void ConfigureStory(ObjectiveSystem objectives) => storyObjectives = objectives;

        public void Configure(QuantumEncounter owner, AnveshController controller, Text objectiveText,
            Text feedbackText, Text countText, Text warningText, Text diagnostics, GameObject diagnosticsPanel,
            Button[] controls, Text[] labels, AmplitudeScopeGraphic graph)
        {
            encounter = owner; anvesh = controller; objective = objectiveText; feedback = feedbackText;
            counts = countText; warning = warningText; debugText = diagnostics; debugPanel = diagnosticsPanel;
            buttons = controls; stateLabels = labels; scope = graph;
        }
        private void OnEnable()
        {
            if (encounter != null) encounter.Changed += Refresh;
            if (anvesh != null) anvesh.DebugToggleRequested += ToggleDebug;
            if (anvesh != null) { anvesh.ContextChanged += ChangeEncounter; anvesh.AvailabilityChanged += Refresh; }
            if (storyObjectives != null) storyObjectives.Changed += Refresh;
        }
        private void Start()
        {
            buttons[0].onClick.AddListener(Scan); buttons[1].onClick.AddListener(Mark);
            buttons[2].onClick.AddListener(Amplify); buttons[3].onClick.AddListener(Lock);
            buttons[4].onClick.AddListener(ResetEncounter);
            showDebug = false; Refresh();
        }
        private void OnDisable()
        {
            if (encounter != null) encounter.Changed -= Refresh;
            if (anvesh != null) anvesh.DebugToggleRequested -= ToggleDebug;
            if (anvesh != null) { anvesh.ContextChanged -= ChangeEncounter; anvesh.AvailabilityChanged -= Refresh; }
            if (storyObjectives != null) storyObjectives.Changed -= Refresh;
        }
        private void OnDestroy()
        {
            if (buttons == null || buttons.Length < 5) return;
            buttons[0].onClick.RemoveListener(Scan); buttons[1].onClick.RemoveListener(Mark);
            buttons[2].onClick.RemoveListener(Amplify); buttons[3].onClick.RemoveListener(Lock);
            buttons[4].onClick.RemoveListener(ResetEncounter);
        }
        private void Scan() => anvesh.Scan();
        private void Mark() => anvesh.Mark();
        private void Amplify() => anvesh.Amplify();
        private void Lock() => anvesh.Lock();
        private void ResetEncounter() => anvesh.Restart();
        private void ToggleDebug() { showDebug = !showDebug; Refresh(); }
        private void ChangeEncounter()
        {
            if (encounter != null) encounter.Changed -= Refresh;
            encounter = anvesh.Encounter;
            if (encounter != null) encounter.Changed += Refresh;
            scope.Configure(encounter); Refresh();
        }

        private void Refresh()
        {
            if (objective == null || anvesh == null) return;
            buttons[0].interactable = anvesh.CanScan; buttons[1].interactable = anvesh.CanMark;
            buttons[2].interactable = anvesh.CanAmplify; buttons[3].interactable = anvesh.CanLock;
            if (storyObjectives != null) objective.text = storyObjectives.Title + "\n" + storyObjectives.Hint;
            if (encounter == null || !encounter.IsInitialized)
            {
                counts.text = "ANVESH\nOFFLINE"; feedback.text = "WASD MOVE  /  SPACE DODGE  /  E INTERACT";
                warning.gameObject.SetActive(false); debugPanel.SetActive(showDebug); debugText.text = "DEVELOPER / F3\nNo active encounter.";
                foreach (Text label in stateLabels) label.gameObject.SetActive(false);
                return;
            }
            counts.text = $"STATES  {encounter.CandidateCount:00}\nITERATION  {encounter.IterationCount:00}";
            objective.text = encounter.HasMeasured ? (encounter.MeasurementSucceeded == true
                ? "SIGNATURE RESOLVED\nTarget state measured. R to reset."
                : "SIGNATURE MISSED\nA different state survived. R to retry.")
                : !encounter.HasScanned ? "RESOLVE THE AUTHORIZATION\nQ  Scan the four quantum nodes."
                : encounter.State == QuantumEncounterState.Marked ? "PHASE MARKED\n2  Redistribute amplitude."
                : encounter.IterationCount == 0 ? "SEARCH SPACE ACQUIRED\n1  Mark the desired signature."
                : "CHOOSE WHEN TO MEASURE\n3  Lock, or 1 then 2 to iterate again.";
            if (storyObjectives != null) objective.text = storyObjectives.Title + "\n" + storyObjectives.Hint;
            feedback.text = encounter.HasMeasured ? $"LOCK / NODE {encounter.MeasuredCandidateIndex + 1:00} / " +
                (encounter.MeasurementSucceeded == true ? "TARGET MEASURED" : "OTHER STATE MEASURED")
                : encounter.State == QuantumEncounterState.Marked ? "PHASE INVERTED  /  DIRECTION CHANGED, MAGNITUDE PRESERVED"
                : encounter.IterationCount > 0 ? "AMPLITUDE REDISTRIBUTED  /  COMPARE THE SIGNALS"
                : encounter.HasScanned ? $"{encounter.CandidateCount} POSSIBILITIES  /  ONE DESIRED SIGNATURE" : "ANVESH  /  SEARCH INTERFACE READY";
            if (encounter.StatusMessage.StartsWith("Candidate ", System.StringComparison.Ordinal))
                feedback.text = encounter.StatusMessage;
            if (anvesh.Operations != null && anvesh.Operations.Busy)
                feedback.text = "ANVESH / " + anvesh.Operations.Action.ToString().ToUpperInvariant() + " / RESOLVING";
            warning.gameObject.SetActive(encounter.LastAmplificationOvershot && !encounter.HasMeasured);
            warning.text = "!  OVERSHOOT  /  TARGET STRENGTH DECLINED";
            for (int i = 0; i < stateLabels.Length; i++)
            {
                bool present = i < encounter.CandidateCount;
                stateLabels[i].gameObject.SetActive(present);
                if (!present) continue;
                float width = 792f / encounter.CandidateCount;
                stateLabels[i].rectTransform.anchoredPosition = new Vector2(1041.6f + i * width, -105.6f);
                stateLabels[i].rectTransform.sizeDelta = new Vector2(width, 27.6f);
                var candidate = encounter.GetCandidate(i);
                stateLabels[i].text = encounter.CandidateCount > 8 ? $"{i + 1:00}" : !encounter.HasScanned ? $"{i + 1:00}  /  --" :
                    string.Format(CultureInfo.InvariantCulture, "{0:00}  /  {1:P0}", i + 1, candidate.Probability);
            }
            scope.SetVerticesDirty();
            debugPanel.SetActive(showDebug);
            if (!showDebug) return;
            var text = new StringBuilder("DEVELOPER / F3\n");
            text.AppendFormat(CultureInfo.InvariantCulture, "N {0}   ITER {1}   {2}\nTarget P {3:P3}{4}\nSum P {5:F8}\n",
                encounter.CandidateCount, encounter.IterationCount, encounter.State,
                encounter.ProbabilityBeforeLock ?? encounter.TargetProbability,
                encounter.HasMeasured ? " (before LOCK)" : "", encounter.ProbabilitySum);
            for (int i = 0; i < encounter.CandidateCount; i++)
            {
                var candidate = encounter.GetCandidate(i);
                text.AppendFormat(CultureInfo.InvariantCulture, "{0:00}  a {1:+0.000;-0.000;0.000}  P {2:P2}\n", i + 1, candidate.Amplitude, candidate.Probability);
            }
            debugText.text = text.ToString();
        }
    }
}
