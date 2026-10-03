using System;
using System.IO;
using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using NullSignal.Quantum;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    /// <summary>Logic checks plus an explicit batch-only Play Mode/input/render verification runner.</summary>
    [InitializeOnLoad]
    public static class NullSignalPhase2Validation
    {
        private const string RunningKey = "NullSignal.Phase2.BatchVerification";
        private static QuantumEncounter encounter;
        private static Keyboard keyboard;
        private static int stage;
        private static double nextStep;
        private static double started;
        private static Vector3 playerPosition;
        private static InputSettings originalInputSettings;
        private static InputSettings batchInputSettings;

        static NullSignalPhase2Validation()
        {
            if (SessionState.GetBool(RunningKey, false)) EditorApplication.update += Tick;
        }

        [MenuItem("Tools/NULL SIGNAL/Validate Playable Quantum Encounter")]
        public static void ValidateEncounterLogic()
        {
            foreach (string result in QuantumValidation.RunAll()) Debug.Log(result);
            var temporary = new GameObject("Temporary encounter validation") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                QuantumEncounter owner = temporary.AddComponent<QuantumEncounter>();
                foreach (int count in new[] { 4, 8, 16 })
                {
                    for (int target = 0; target < count; target++)
                    {
                        owner.Configure(count, target);
                        owner.RestartEncounter(new System.Random(42));
                        Require(!owner.Mark() && !owner.Amplify() && !owner.Lock(), "SCAN gates operations");
                        Require(owner.Scan(), "SCAN acquired encounter");
                        var reference = new AmplitudeAmplificationSystem(new GroverSearchScenario(count, target));
                        for (int iteration = 1; iteration <= 12; iteration++)
                        {
                            double probability = owner.TargetProbability;
                            double amplitude = owner.GetCandidate(target).Amplitude;
                            Require(owner.Mark(), "MARK accepted");
                            Close(owner.GetCandidate(target).Amplitude, -amplitude, "Phase flipped");
                            Close(owner.TargetProbability, probability, "MARK preserves probability");
                            Require(!owner.Mark(), "Double MARK rejected");
                            Require(owner.Amplify(), "AMPLIFY accepted");
                            reference.ApplyIteration();
                            Require(owner.IterationCount == iteration, "Iteration count delegated");
                            for (int i = 0; i < count; i++)
                                Close(owner.GetCandidate(i).Amplitude, reference.GetCandidate(i).Amplitude, "Encounter matches Phase 1 state");
                        }
                        double[] snapshot = owner.GetAmplitudes(); snapshot[0] = 123;
                        Close(owner.GetCandidate(0).Amplitude, reference.GetCandidate(0).Amplitude, "Snapshot isolation");
                        Require(owner.Lock() && owner.HasMeasured, "Real measurement resolves encounter");
                        Require(!owner.Mark() && !owner.Amplify() && !owner.Lock(), "Resolved encounter cannot mutate");
                        Close(owner.ProbabilitySum, 1, "Collapsed normalization");
                        owner.RestartEncounter();
                        Require(!owner.HasScanned && !owner.HasMeasured && owner.IterationCount == 0, "Restart clears state");
                    }
                    owner.Configure(count, 0);
                    owner.RestartEncounter(new System.Random(8719 + count));
                    int successes = 0;
                    const int trials = 3000;
                    for (int trial = 0; trial < trials; trial++)
                    {
                        if (trial > 0) owner.RestartEncounter();
                        owner.Scan(); owner.Lock();
                        if (owner.MeasurementSucceeded == true) successes++;
                    }
                    double expected = 1.0 / count;
                    Require(Math.Abs((double)successes / trials - expected) < 6 * Math.Sqrt(expected * (1 - expected) / trials), "Encounter LOCK samples uniform weights");
                    Debug.Log($"Phase 2 N={count}: all targets, delegated iteration/phase/math, restart, guards and 3000 LOCK samples passed.");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(temporary); }
        }

        // Called explicitly by Unity -executeMethod, never runs automatically in normal editing.
        public static void RunBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("RunBatch is reserved for isolated batch verification.");
            ValidateEncounterLogic();
            string pathRecord = Path.Combine(EvidenceDirectory, "scene-path.txt");
            string scenePath = File.Exists(pathRecord) ? File.ReadAllText(pathRecord) : null;
            if (!string.IsNullOrEmpty(scenePath) && AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
                EditorSceneManager.OpenScene(scenePath);
            else
                scenePath = NullSignalPhase2Builder.CreateAndSavePrototype();
            Directory.CreateDirectory(EvidenceDirectory);
            File.WriteAllText(Path.Combine(EvidenceDirectory, "scene-path.txt"), scenePath);
            SessionState.SetBool(RunningKey, true);
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }

        private static string EvidenceDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Phase2Evidence"));

        private static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (encounter == null)
                {
                    encounter = UnityEngine.Object.FindAnyObjectByType<QuantumEncounter>();
                    Require(encounter != null, "Prototype encounter exists");
                    encounter.Configure(4, 0);
                    encounter.RestartEncounter(new FixedSampleRandom());
                    // A hidden batch editor has no focused Game view. Use an in-memory clone
                    // for injected device events; never change or save the project's input settings.
                    originalInputSettings = InputSystem.settings;
                    batchInputSettings = UnityEngine.Object.Instantiate(originalInputSettings);
                    batchInputSettings.hideFlags = HideFlags.HideAndDontSave;
                    batchInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    batchInputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.settings = batchInputSettings;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                    started = now; nextStep = now + 1; stage = 0;
                    return;
                }
                if (now - started > 90) throw new InvalidOperationException("Play Mode verification timed out.");
                if (now < nextStep) return;
                switch (stage)
                {
                    case 0:
                        Require(!encounter.HasScanned, "Initial state unscanned");
                        for (int i = 0; i < 4; i++) Close(encounter.GetCandidate(i).Amplitude, 0.5, "Equal candidates");
                        Capture("00_equal"); Press(Key.Q); break;
                    case 1: Require(encounter.HasScanned, "Q SCAN input"); Capture("01_scan"); Press(Key.Digit1); break;
                    case 2:
                        Close(encounter.GetCandidate(0).Amplitude, -0.5, "1 MARK input");
                        Close(encounter.TargetProbability, 0.25, "MARK magnitude unchanged");
                        ValidateMarkedVisuals(); Capture("02_mark"); Press(Key.Digit2); break;
                    case 3:
                        Require(encounter.IterationCount == 1, "2 AMPLIFY input"); Close(encounter.TargetProbability, 1, "N4 amplification");
                        Capture("03_amplify"); Press(Key.Digit3); break;
                    case 4:
                        Require(encounter.MeasurementSucceeded == true && encounter.MeasuredCandidateIndex == 0, "3 LOCK successful weighted measurement");
                        ValidateCollapse(); Capture("04_lock_success"); Press(Key.R); break;
                    case 5: Require(!encounter.HasMeasured && !encounter.HasScanned, "R restart input"); Capture("05_reset"); Press(Key.Q); break;
                    case 6: Press(Key.Digit1); break;
                    case 7: Press(Key.Digit2); break;
                    case 8: Press(Key.Digit1); break;
                    case 9: Press(Key.Digit2); break;
                    case 10:
                        Require(encounter.IterationCount == 2 && encounter.LastAmplificationOvershot, "Natural overshooting");
                        Close(encounter.TargetProbability, 0.25, "N4 overshoot distribution"); Capture("06_overshoot"); Press(Key.Digit3); break;
                    case 11:
                        Require(encounter.MeasurementSucceeded == false && encounter.MeasuredCandidateIndex == 3, "LOCK miss uses CDF weights, not target selection");
                        ValidateCollapse(); Capture("07_lock_miss"); Press(Key.R); break;
                    case 12: Press(Key.F3); break;
                    case 13:
                        Require(!UnityEngine.Object.FindAnyObjectByType<HUDController>().ShowDebug, "F3 hides statistics");
                        Capture("08_debug_hidden"); Release(); Click("Q  SCAN"); break;
                    case 14: ReleaseMouse(); break;
                    case 15: Require(encounter.HasScanned, "SCAN mouse button"); Click("1  MARK"); break;
                    case 16: ReleaseMouse(); break;
                    case 17: Require(encounter.State == QuantumEncounterState.Marked, "MARK mouse button"); Click("2  AMPLIFY"); break;
                    case 18: ReleaseMouse(); break;
                    case 19: Require(encounter.IterationCount == 1, "AMPLIFY mouse button"); Click("3  LOCK"); break;
                    case 20: ReleaseMouse(); break;
                    case 21: Require(encounter.MeasurementSucceeded == true, "LOCK mouse button"); Click("R  RESTART"); break;
                    case 22: ReleaseMouse(); break;
                    case 23:
                        Require(!encounter.HasMeasured && !encounter.HasScanned, "RESTART mouse button");
                        playerPosition = UnityEngine.Object.FindAnyObjectByType<PlayerController>().transform.position; Press(Key.W); break;
                    case 24:
                        Require(Vector3.Distance(playerPosition, UnityEngine.Object.FindAnyObjectByType<PlayerController>().transform.position) > 0.2f, "Preserved movement reacts to W");
                        Release(); break;
                    case 25:
                        playerPosition = UnityEngine.Object.FindAnyObjectByType<PlayerController>().transform.position; Press(Key.Space); break;
                    case 26:
                        Require(Vector3.Distance(playerPosition, UnityEngine.Object.FindAnyObjectByType<PlayerController>().transform.position) > 0.5f, "Preserved dodge reacts to Space");
                        Release();
                        break;
                    case 27:
                        var movement = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                        var motor = movement.GetComponent<CharacterController>();
                        foreach (CandidateVisualizer candidate in UnityEngine.Object.FindObjectsByType<CandidateVisualizer>())
                            if (candidate.CandidateIndex == 0)
                            {
                                motor.enabled = false;
                                movement.transform.position = candidate.transform.position + Vector3.back * 1.1f;
                                motor.enabled = true;
                            }
                        Physics.SyncTransforms(); Press(Key.E); break;
                    case 28:
                        Require(encounter.StatusMessage.StartsWith("Candidate 1:") && !encounter.HasMeasured, "Preserved E interaction inspects without measuring");
                        Release();
                        File.WriteAllText(Path.Combine(EvidenceDirectory, "validation.txt"), "PASS: Unity Play Mode; injected Q/1/2/3/R/F3 keyboard events; UI raycasts and pointer-handler dispatch for all five HUD buttons; equal/marked/amplified/overshoot states; successful and missed weighted LOCK; collapse; repeated resets; preserved movement, dodge and E inspection.\nLIMIT: native mouse-event injection in the hidden batch editor was unreliable; physical mouse interaction and a WebGL player build were not verified.\n");
                        Debug.Log("NULL SIGNAL PHASE 2 PLAY MODE VERIFICATION PASSED."); Finish(0); return;
                }
                stage++;
                nextStep = now + (stage >= 14 ? 0.25 : 0.8);
            }
            catch (Exception exception) { Debug.LogException(exception); Finish(1); }
        }

        private static void Press(Key key) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        private static void Release() => InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        private static void Click(string name)
        {
            Canvas.ForceUpdateCanvases();
            foreach (Button button in UnityEngine.Object.FindObjectsByType<Button>())
                if (button.name == name)
                {
                    Require(button.interactable, name + " available");
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
                    var pointer = new PointerEventData(EventSystem.current) { position = point, button = PointerEventData.InputButton.Left };
                    var hits = new System.Collections.Generic.List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, hits);
                    Require(hits.Count > 0 && hits[0].gameObject == button.gameObject, name + " raycast reaches button");
                    ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                    return;
                }
            throw new InvalidOperationException("Missing HUD button: " + name);
        }
        private static void ReleaseMouse() { } // Pointer handlers above complete down/up/click together.

        private static void ValidateMarkedVisuals()
        {
            CandidateVisualizer[] visuals = UnityEngine.Object.FindObjectsByType<CandidateVisualizer>();
            Array.Sort(visuals, (a, b) => a.CandidateIndex.CompareTo(b.CandidateIndex));
            LineRenderer marked = visuals[0].GetComponentInChildren<LineRenderer>();
            LineRenderer other = visuals[1].GetComponentInChildren<LineRenderer>();
            Require(Math.Abs((marked.GetPosition(10).y - 1.3f) + (other.GetPosition(10).y - 1.3f)) < 0.002f, "Marked waveform mirrors its equal-magnitude peer");
            Renderer markedBody = visuals[0].transform.Find("Amplitude manifestation").GetComponent<Renderer>();
            Renderer otherBody = visuals[1].transform.Find("Amplitude manifestation").GetComponent<Renderer>();
            var a = new MaterialPropertyBlock(); var b = new MaterialPropertyBlock();
            markedBody.GetPropertyBlock(a); otherBody.GetPropertyBlock(b);
            Require(a.GetColor("_BaseColor") == b.GetColor("_BaseColor"), "MARK has no unique color or magnitude reveal");
        }

        private static void ValidateCollapse()
        {
            foreach (CandidateVisualizer visual in UnityEngine.Object.FindObjectsByType<CandidateVisualizer>())
            {
                bool measured = visual.CandidateIndex == encounter.MeasuredCandidateIndex;
                Require(visual.GetComponentInChildren<LineRenderer>().enabled == measured, "Only measured waveform remains");
            }
        }

        private static void Capture(string name)
        {
            Camera view = Camera.main;
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            RenderMode previousMode = canvas.renderMode;
            float previousAspect = view.aspect;
            RenderTexture previousTarget = view.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            var target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                target.Create(); view.targetTexture = target; view.aspect = 1280f / 720f;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = view; canvas.planeDistance = 2f;
                canvas.GetComponent<CanvasScaler>().SendMessage("Update");
                Canvas.ForceUpdateCanvases();
                canvas.GetComponent<HUDController>().SendMessage("LateUpdate");
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(view, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
                File.WriteAllBytes(Path.Combine(EvidenceDirectory, name + ".png"), pixels.EncodeToPNG());
            }
            finally
            {
                view.targetTexture = previousTarget; view.aspect = previousAspect;
                canvas.renderMode = previousMode; canvas.worldCamera = null;
                RenderTexture.active = previousActive;
                target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(pixels);
                canvas.GetComponent<CanvasScaler>().SendMessage("Update"); Canvas.ForceUpdateCanvases();
            }
        }

        private static void Finish(int code)
        {
            SessionState.SetBool(RunningKey, false); EditorApplication.update -= Tick;
            if (originalInputSettings != null) InputSystem.settings = originalInputSettings;
            if (batchInputSettings != null) UnityEngine.Object.DestroyImmediate(batchInputSettings);
            EditorApplication.Exit(code);
        }
        private static void Close(double actual, double expected, string message) => Require(Math.Abs(actual - expected) <= QuantumValidation.Tolerance, message);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("Phase 2 validation failed: " + message); }
        private sealed class FixedSampleRandom : System.Random { public override double NextDouble() => 0.99; }
    }
}
