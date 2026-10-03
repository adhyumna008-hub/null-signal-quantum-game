using System.Globalization;
using System.Text;
using NullSignal.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private AnveshController anvesh;
        [SerializeField] private Camera view;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Text statusText;
        [SerializeField] private Text iterationText;
        [SerializeField] private Text debugText;
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private Text[] candidateLabels;
        [SerializeField] private CandidateVisualizer[] candidates;
        [SerializeField] private bool showDebug;

        public bool ShowDebug { get => showDebug; set { showDebug = value; Refresh(); } }

        public void Configure(QuantumEncounter owner, AnveshController controller, Camera camera,
            Canvas hudCanvas, Text status, Text iterations, Text debug, Button[] buttons,
            Text[] labels, CandidateVisualizer[] visualizers, bool debugInitiallyVisible)
        {
            encounter = owner; anvesh = controller; view = camera; canvas = hudCanvas;
            statusText = status; iterationText = iterations; debugText = debug;
            actionButtons = buttons; candidateLabels = labels; candidates = visualizers;
            showDebug = debugInitiallyVisible;
        }

        private void OnEnable()
        {
            if (encounter != null) encounter.Changed += Refresh;
            if (anvesh != null) anvesh.DebugToggleRequested += ToggleDebug;
        }

        private void Start()
        {
            // Runtime listeners survive scene serialization; no editor-created UnityEvent closures.
            actionButtons[0].onClick.AddListener(Scan);
            actionButtons[1].onClick.AddListener(Mark);
            actionButtons[2].onClick.AddListener(Amplify);
            actionButtons[3].onClick.AddListener(Lock);
            actionButtons[4].onClick.AddListener(Restart);
            Refresh();
        }

        private void OnDisable()
        {
            if (encounter != null) encounter.Changed -= Refresh;
            if (anvesh != null) anvesh.DebugToggleRequested -= ToggleDebug;
        }
        private void OnDestroy()
        {
            if (actionButtons == null || actionButtons.Length != 5) return;
            actionButtons[0].onClick.RemoveListener(Scan);
            actionButtons[1].onClick.RemoveListener(Mark);
            actionButtons[2].onClick.RemoveListener(Amplify);
            actionButtons[3].onClick.RemoveListener(Lock);
            actionButtons[4].onClick.RemoveListener(Restart);
        }
        private void Scan() => anvesh.Scan();
        private void Mark() => anvesh.Mark();
        private void Amplify() => anvesh.Amplify();
        private void Lock() => anvesh.Lock();
        private void Restart() => anvesh.Restart();

        private void ToggleDebug() => ShowDebug = !showDebug;

        private void Refresh()
        {
            if (encounter == null || !encounter.IsInitialized || statusText == null) return;
            statusText.text = encounter.StatusMessage;
            statusText.color = encounter.HasMeasured && encounter.MeasurementSucceeded == false
                || encounter.LastAmplificationOvershot ? new Color(1f, 0.76f, 0.35f) : Color.white;
            iterationText.text = $"{encounter.State.ToString().ToUpperInvariant()}  |  ITERATIONS {encounter.IterationCount}";
            actionButtons[0].interactable = encounter.CanScan;
            actionButtons[1].interactable = encounter.CanMark;
            actionButtons[2].interactable = encounter.CanAmplify;
            actionButtons[3].interactable = encounter.CanLock;
            actionButtons[4].interactable = true;
            debugText.gameObject.SetActive(showDebug);
            if (showDebug)
            {
                var text = new StringBuilder("DEVELOPMENT ONLY  /  F3 HIDE\n");
                text.AppendFormat(CultureInfo.InvariantCulture, "Target probability: {0:P2}\n",
                    encounter.ProbabilityBeforeLock ?? encounter.TargetProbability);
                if (encounter.HasMeasured) text.Append("(distribution before LOCK)\n");
                text.AppendFormat(CultureInfo.InvariantCulture, "Probability sum: {0:F6}  |  N={1}",
                    encounter.ProbabilitySum, encounter.CandidateCount);
                debugText.text = text.ToString();
            }
        }

        private void LateUpdate()
        {
            if (encounter == null || !encounter.IsInitialized || canvas == null || view == null) return;
            RectTransform canvasRect = (RectTransform)canvas.transform;
            for (int i = 0; i < candidateLabels.Length; i++)
            {
                var snapshot = encounter.GetCandidate(candidates[i].CandidateIndex);
                Vector3 screen = view.WorldToScreenPoint(candidates[i].LabelPosition);
                candidateLabels[i].gameObject.SetActive(screen.z > 0f);
                Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCamera, out Vector2 position);
                candidateLabels[i].rectTransform.anchoredPosition = position;
                candidateLabels[i].text = showDebug
                    ? string.Format(CultureInfo.InvariantCulture, "{0}   a={1:+0.00;-0.00;0.00}\nP={2:P1}",
                        i + 1, snapshot.Amplitude, snapshot.Probability)
                    : (i + 1).ToString(CultureInfo.InvariantCulture);
                if (encounter.HasMeasured && encounter.MeasuredCandidateIndex != candidates[i].CandidateIndex)
                    candidateLabels[i].text = showDebug ? $"{i + 1}  COLLAPSED" : string.Empty;
            }
        }
    }
}
