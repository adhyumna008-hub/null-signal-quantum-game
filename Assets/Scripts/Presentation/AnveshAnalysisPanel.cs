using System.Collections.Generic;
using System.Globalization;
using NullSignal.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    public sealed class AnveshAnalysisPanel : MonoBehaviour
    {
        [SerializeField] private AnveshOperationController operations;
        [SerializeField] private AnveshController controller;
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text title, equation, values, explanation;
        [SerializeField] private TMP_Text operationLabel, equationLabel, stateLabel, resultLabel, teachingLabel;
        [SerializeField] private Image progress;
        private readonly HashSet<string> taught = new HashSet<string>();
        private bool debug;
        private float visibility;
        private double[] displayedBefore;
        private bool outcomeVisible;
        public void Configure(AnveshOperationController source, AnveshController input, RectTransform root, CanvasGroup alpha,
            Text header, Text formula, Text data, Text teaching, Image timer)
        { operations = source; controller = input; panel = root; group = alpha; title = header; equation = formula; values = data; explanation = teaching; progress = timer; }
        public void ConfigureSharp(AnveshOperationController source, AnveshController input, RectTransform root, CanvasGroup alpha,
            TMP_Text header, TMP_Text formula, TMP_Text states, TMP_Text result, TMP_Text teaching, Image timer)
        { operations = source; controller = input; panel = root; group = alpha; operationLabel = header; equationLabel = formula; stateLabel = states; resultLabel = result; teachingLabel = teaching; progress = timer; }
        private void OnEnable() { operations.Changed += Refresh; controller.DebugToggleRequested += ToggleDebug; }
        private void OnDisable() { operations.Changed -= Refresh; controller.DebugToggleRequested -= ToggleDebug; }
        private void ToggleDebug() { debug = !debug; Refresh(); }
        private static string Signed(double value) => value.ToString("+0.0000;-0.0000;0.0000", CultureInfo.InvariantCulture);
        private void Update()
        {
            // Let the world and the interpolated wave graph show the change before explaining it.
            if (operations.Action != AnveshAbility.None && operations.After != null && !outcomeVisible
                && Time.unscaledTime - operations.DataAt >= .45f)
            { outcomeVisible = true; Refresh(); }
            bool show = !debug && operations.Action != AnveshAbility.None && (operations.Busy || Time.unscaledTime - operations.ReportAt < 5f);
            visibility = Mathf.MoveTowards(visibility, show ? 1 : 0, Time.unscaledDeltaTime * 5f);
            panel.anchoredPosition = new Vector2(Mathf.Lerp(panel.rect.width + 24f, -24f, Mathf.SmoothStep(0, 1, visibility)), operationLabel != null ? -150f : -170f);
            group.alpha = visibility; progress.fillAmount = operations.Progress;
            if (operations.Busy && operations.Action == AnveshAbility.Scan) group.alpha *= Mathf.Clamp01(operations.Progress * 2.5f);
            if (resultLabel != null)
            {
                float warningAge = Time.unscaledTime - operations.DataAt;
                float flash = operations.Overshot && warningAge < .65f ? Mathf.Abs(Mathf.Sin(warningAge * 18f)) : 0;
                resultLabel.color = Color.Lerp(new Color(.9f, .72f, .46f), new Color(1f, .35f, .55f), flash);
            }
            progress.rectTransform.sizeDelta = new Vector2(panel.rect.width * operations.Progress, 3f);
        }
        private void Refresh()
        {
            if (operations.Action == AnveshAbility.None || operations.Before == null)
            { outcomeVisible = false; Write(teachingLabel, explanation, ""); return; }
            var old = operations.Before; var next = operations.After;
            if (!ReferenceEquals(displayedBefore, old)) { displayedBefore = old; outcomeVisible = false; Write(teachingLabel, explanation, ""); }
            int target = operations.Target;
            string operation, formula, state, result;
            string changed = Signed(old[target]) + " → " + (next == null ? "..." : Signed(next[target]));
            string lesson = ""; string key = operations.Action.ToString();
            switch (operations.Action)
            {
                case AnveshAbility.Scan:
                    operation = "SCAN";
                    formula = $"{old.Length} POSSIBLE STATES";
                    if (old.Length <= 4) formula += "\nSTATE / AMPLITUDE / CHANCE";
                    state = "";
                    for (int i = 0; i < old.Length; i++)
                        state += string.Format(CultureInfo.InvariantCulture, "{0:00}   {1}   {2:P1}\n", i + 1, Signed(old[i]), old[i] * old[i]);
                    if (old.Length > 4)
                    {
                        double min = old[0], max = old[0], minChance = old[0] * old[0], maxChance = minChance;
                        foreach (double amplitude in old)
                        {
                            min = System.Math.Min(min, amplitude); max = System.Math.Max(max, amplitude);
                            minChance = System.Math.Min(minChance, amplitude * amplitude); maxChance = System.Math.Max(maxChance, amplitude * amplitude);
                        }
                        state = "AMPLITUDE\n" + (min == max ? Signed(min) + " EACH" : Signed(min) + " TO " + Signed(max))
                            + "\nCHANCE\n" + (minChance == maxChance ? minChance.ToString("P1", CultureInfo.InvariantCulture) + " EACH"
                                : string.Format(CultureInfo.InvariantCulture, "{0:P1} TO {1:P1}", minChance, maxChance));
                    }
                    result = next == null ? "ACQUIRING..." : "SEARCH SPACE ACQUIRED";
                    break;
                case AnveshAbility.Mark:
                    operation = "MARK";
                    formula = "WAVE DIRECTION";
                    state = $"STATE {target + 1:00}\n{Signed(old[target])}\n↓\n{(next == null ? "..." : Signed(next[target]))}";
                    result = outcomeVisible ? "PHASE CHANGED" : "REVERSING...";
                    lesson = "Phase changed. Magnitude stayed the same."; break;
                case AnveshAbility.Amplify:
                    operation = "AMPLIFY";
                    formula = "WAVE HEIGHT / AMPLITUDE";
                    int other = target == 0 ? 1 : 0;
                    state = $"TARGET / STATE {target + 1:00}\n{changed}\nOTHER / STATE {other + 1:00}\n{Signed(old[other])} → {(next == null ? "..." : Signed(next[other]))}";
                    result = next == null ? "REDISTRIBUTING..." : operations.Overshot && outcomeVisible ? string.Format(CultureInfo.InvariantCulture, "TARGET CHANCE DECLINING\n{0:P1} → {1:P1}", old[target] * old[target], next[target] * next[target])
                        : string.Format(CultureInfo.InvariantCulture, "TARGET CHANCE\n{0:P1} → {1:P1}", old[target] * old[target], next[target] * next[target]);
                    if (next != null && next[target] * next[target] > old[target] * old[target])
                        lesson = "Amplitude was redistributed toward the marked state.";
                    if (operations.Overshot) { key = "Overshoot"; lesson = "You passed the peak. More amplification is not always better."; }
                    break;
                case AnveshAbility.Lock:
                    operation = "LOCK";
                    formula = "CHANCE → COLLAPSE → RESULT";
                    state = string.Format(CultureInfo.InvariantCulture, "TARGET CHANCE\n{0:P1}\n\n{1}", operations.TargetProbability,
                        operations.Measured.HasValue && outcomeVisible ? $"STATE {operations.Measured.Value + 1:00} REMAINS" : "COLLAPSING...");
                    result = operations.Measured.HasValue && outcomeVisible ? operations.Measured.Value == target ? "TARGET MEASURED" : "OTHER STATE MEASURED" : "MEASUREMENT IN PROGRESS";
                    lesson = "LOCK keeps one state, sampled using the chances shown.";
                    break;
                default: return;
            }
            Write(operationLabel, title, "ANVESH\n" + operation); Write(equationLabel, equation, formula);
            Write(stateLabel, values, state); Write(resultLabel, null, result);
            if (stateLabel != null) stateLabel.fontSize = operations.Action == AnveshAbility.Scan ? 20f : 23f;
            if (resultLabel == null && values != null) values.text += "\n" + result;
            // One short explanation per concept, after its first visible outcome. Repeats keep the live values.
            if (!controller.ShowTeaching) Write(teachingLabel, explanation, "");
            else if (!debug && outcomeVisible && lesson.Length > 0 && !taught.Contains(key)) { taught.Add(key); Write(teachingLabel, explanation, lesson); }
        }
        private static void Write(TMP_Text sharp, Text legacy, string content)
        { if (sharp != null) sharp.text = content; else if (legacy != null) legacy.text = content; }
    }
}
