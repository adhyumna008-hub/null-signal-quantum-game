using System;
using NullSignal.Quantum;
using TMPro;
using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Bounded, real-time 3D opening and ending. Every pose is evaluated from one unscaled clock.</summary>
    public sealed class StationCinematicDirector : MonoBehaviour
    {
        [Serializable]
        public sealed class StageReferences
        {
            public Transform exterior, control, docking, cabin;
            public Transform stationRing, stationCore, exteriorCraft, earth, distantSignals;
            public Transform[] exteriorEnergy, arrayRings, signalBars, anomalyFragments, driftingMotes;
            public Transform anomaly, dockDoorL, dockDoorR, craftDoorL, craftDoorR, wristSignal;
            public LineRenderer[] signalWaves;
            public LineRenderer wristWave;
            public CinematicActor ananya, lubna, arrivingAnirudh, cabinAnirudh;
            public TMP_Text researchDisplay, cabinDisplay, locationTitle;
            public Light researchKey, researchAlarm, dockAlarm, cabinKey;
        }

        [SerializeField] private StageReferences stage;
        [SerializeField] private Camera cinematicCamera, gameplayCamera;
        [SerializeField] private SubtitleController subtitles;
        [SerializeField] private StationFeedbackAudio audioFeedback;
        private Action finished;
        private float clock;
        private bool backdrop, gameplayWasEnabled;
        private int nextLine, quantumStep;
        private AmplitudeAmplificationSystem research;
        private readonly float[] displayedAmplitudes = new float[16];
        private Vector3[] fragmentRest;
        private static readonly float[] QuantumTimes = { 10.2f, 10.9f, 12f, 12.7f, 13.8f, 14.5f };

        public bool IsPlaying { get; private set; }
        public bool IsEnding { get; private set; }

        private struct SpokenBeat
        {
            public float at;
            public SubtitleCue cue;
            public SpokenBeat(float at, string speaker, string text, float duration)
            { this.at = at; cue = new SubtitleCue(speaker, text, duration); }
        }
        private static readonly SpokenBeat[] OpeningLines = {
            new SpokenBeat(4.8f, "LUBNA", "Background search complete.", 2.3f),
            new SpokenBeat(7.2f, "ANANYA", "Again.", 1.8f),
            new SpokenBeat(10.4f, "LUBNA", "That signal was barely there.", 2.5f),
            new SpokenBeat(13.2f, "ANANYA", "One more iteration.", 2.2f),
            new SpokenBeat(16.7f, "UNKNOWN SIGNAL", "You found me.", 3.2f),
            new SpokenBeat(26.7f, "TARA", "Emergency retrieval protocol active.", 2.2f),
            new SpokenBeat(29f, "ANIRUDH", "Find the Core. Shut it down.", 2.5f)
        };
        private static readonly SpokenBeat[] EndingLines = {
            new SpokenBeat(17.8f, "ANIRUDH", "I destroyed the Array.", 2.4f),
            new SpokenBeat(21f, "UNKNOWN SIGNAL", "You destroyed one.", 2.4f),
            new SpokenBeat(25f, "UNKNOWN SIGNAL", "Now we know how to search back.", 3.6f)
        };

        public void Configure(StageReferences references, Camera camera, Camera gameCamera,
            SubtitleController dialogue, StationFeedbackAudio feedback)
        {
            stage = references; cinematicCamera = camera; gameplayCamera = gameCamera;
            subtitles = dialogue; audioFeedback = feedback;
        }

        private void Awake()
        {
            fragmentRest = new Vector3[stage.anomalyFragments.Length];
            for (int i = 0; i < fragmentRest.Length; i++) fragmentRest[i] = stage.anomalyFragments[i].localPosition;
            SetStage(null); cinematicCamera.enabled = false;
        }

        public void PlayOpening(Action completion) { Begin(false, completion); }
        public void PlayEnding(Action completion) { Begin(true, completion); }

        private void Begin(bool ending, Action completion)
        {
            if (!IsPlaying && !backdrop) gameplayWasEnabled = gameplayCamera != null && gameplayCamera.enabled;
            IsPlaying = true; IsEnding = ending; backdrop = false; finished = completion; clock = 0f; nextLine = 0;
            quantumStep = 0; research = new AmplitudeAmplificationSystem(new GroverSearchScenario(16, 7));
            for (int i = 0; i < 16; i++) displayedAmplitudes[i] = (float)research.GetCandidate(i).Amplitude;
            if (subtitles != null) subtitles.Clear();
            if (gameplayCamera != null) gameplayCamera.enabled = false;
            cinematicCamera.enabled = true;
            if (audioFeedback != null) audioFeedback.Cinematic(true, ending);
            Evaluate();
        }

        /// <summary>Animated menu backdrop, with no dialogue, timer, or completion callback.</summary>
        public void ShowBackdrop()
        {
            if (IsPlaying) Stop();
            if (!backdrop) gameplayWasEnabled = gameplayCamera != null && gameplayCamera.enabled;
            backdrop = true; clock = 0f; IsEnding = false;
            if (gameplayCamera != null) gameplayCamera.enabled = false;
            cinematicCamera.enabled = true;
            SetStage(stage.exterior);
        }

        public void Stop()
        {
            IsPlaying = false; backdrop = false; finished = null;
            SetStage(null);
            cinematicCamera.enabled = false;
            if (gameplayCamera != null) gameplayCamera.enabled = gameplayWasEnabled;
            if (subtitles != null) subtitles.Clear();
            if (audioFeedback != null) audioFeedback.Cinematic(false);
        }

        private void OnDisable() { if (IsPlaying || backdrop) Stop(); }

        private void Update()
        {
            if (!IsPlaying && !backdrop) return;
            clock += Time.unscaledDeltaTime;
            if (backdrop)
            {
                AnimateExterior(clock, 0f);
                float a = clock * .018f;
                Shot(stage.exterior, new Vector3(19f * Mathf.Cos(a), 10f, -24f + 3f * Mathf.Sin(a)),
                    new Vector3(0f, 0f, 0f), 45f);
                stage.locationTitle.gameObject.SetActive(false);
                return;
            }
            SpokenBeat[] beats = IsEnding ? EndingLines : OpeningLines;
            while (nextLine < beats.Length && clock >= beats[nextLine].at)
            {
                if (subtitles != null) subtitles.Play(null, beats[nextLine].cue);
                nextLine++;
            }
            Evaluate();
            if (clock >= (IsEnding ? 30f : 32f))
            {
                Action callback = finished;
                Stop();
                callback?.Invoke();
            }
        }

        private void Evaluate() { if (IsEnding) Ending(clock); else Opening(clock); }

        private void Opening(float t)
        {
            if (t < 4.2f)
            {
                SetStage(stage.exterior); AnimateExterior(t, 0f);
                Shot(stage.exterior, Vector3.Lerp(new Vector3(23f, 12f, -29f), new Vector3(18f, 8f, -23f), Ease(t / 4.2f)),
                    new Vector3(0f, .7f, 0f), 43f);
                stage.locationTitle.gameObject.SetActive(true);
                stage.locationTitle.text = "ASTRA–7\n<size=48%>QUANTUM RESEARCH STATION  ·  2189</size>";
            }
            else if (t < 25.2f)
            {
                SetStage(stage.control); AnimateResearch(t);
                if (t < 10f)
                    Shot(stage.control, Vector3.Lerp(new Vector3(-5f, 2.7f, -6.8f), new Vector3(-3.7f, 2.25f, -5.1f), Ease((t - 4.2f) / 5.8f)), new Vector3(.1f, 1.6f, .7f), 47f);
                else if (t < 16.3f)
                    Shot(stage.control, Vector3.Lerp(new Vector3(3.8f, 2.5f, -4.4f), new Vector3(2.7f, 2.15f, -3.7f), Ease((t - 10f) / 6.3f)), new Vector3(.2f, 1.75f, 1.4f), 43f);
                else if (t < 20.8f)
                    Shot(stage.control, Vector3.Lerp(new Vector3(-3.1f, 2.05f, 2.4f), new Vector3(-4.2f, 2.2f, 2.6f), Ease((t - 16.3f) / 4.5f)), new Vector3(.35f, 1.55f, -.5f), 51f);
                else
                    Shot(stage.control, Vector3.Lerp(new Vector3(2.3f, 2.3f, -1.4f), new Vector3(1.3f, 2.05f, -.6f), Ease((t - 20.8f) / 4.4f)), new Vector3(.1f, 2f, 4.6f), 44f);
                float alarmKick = StationCamera.ReducedShake ? 0f : Mathf.Exp(-Mathf.Max(0f, t - 19.5f) * 5f) * (t >= 19.5f ? .04f : 0f);
                cinematicCamera.transform.position += new Vector3(Mathf.Sin(t * 61f), Mathf.Cos(t * 47f), 0f) * alarmKick;
            }
            else
            {
                SetStage(stage.docking);
                float p = Ease((t - 25.2f) / 5.8f);
                stage.arrivingAnirudh.transform.localPosition = new Vector3(-.3f, 0f, Mathf.Lerp(2.2f, -1.15f, p));
                stage.arrivingAnirudh.transform.localRotation = Quaternion.Euler(0f, 180f - 15f * Ease((t - 30f) / 2f), 0f);
                stage.arrivingAnirudh.Pose(t, t < 30.7f ? .8f : .1f, .1f, .13f, 12f, t > 29f);
                float open = Ease((t - 25.4f) / 1.2f);
                stage.dockDoorL.localPosition = new Vector3(-.61f - open * 1.05f, 1.35f, 2.8f);
                stage.dockDoorR.localPosition = new Vector3(.61f + open * 1.05f, 1.35f, 2.8f);
                stage.dockAlarm.intensity = 2.2f + .5f * Mathf.Sin(t * 5f);
                Shot(stage.docking, Vector3.Lerp(new Vector3(-3.2f, 1.75f, -4f), new Vector3(-2.4f, 1.85f, -3.1f), p),
                    stage.arrivingAnirudh.transform.localPosition + Vector3.up * 1.35f, 47f);
                stage.locationTitle.gameObject.SetActive(false);
            }
        }

        private void AnimateResearch(float t)
        {
            float reaction = Ease((t - 16.6f) / .65f);
            float escape = Ease((t - 20f) / 3.2f);
            stage.ananya.transform.localPosition = new Vector3(-1.35f - .7f * escape, 0f, -.5f - .8f * reaction - 1.8f * escape);
            stage.lubna.transform.localPosition = new Vector3(1.9f + .55f * reaction, 0f, .1f - .8f * reaction - 1.5f * escape);
            stage.ananya.transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(18f, -140f, escape), 0f);
            stage.lubna.transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(-35f, 150f, escape), 0f);
            bool speakingAnanya = subtitles != null && subtitles.Speaker == "ANANYA";
            bool speakingLubna = subtitles != null && subtitles.Speaker == "LUBNA";
            stage.ananya.Pose(t, escape < 1f && escape > 0f ? 1f : 0f, (1f - reaction) * .85f, reaction * (1f - escape), 18f * Mathf.Sin(t * .3f), speakingAnanya);
            stage.lubna.Pose(t + .6f, escape < 1f && escape > 0f ? .9f : 0f, (1f - reaction) * .4f, reaction * .9f * (1f - escape), t < 10f ? -28f : 8f, speakingLubna);
            while (quantumStep < QuantumTimes.Length && t >= QuantumTimes[quantumStep])
            {
                if (quantumStep % 2 == 0) research.ApplyOracle(); else research.ApplyDiffusion();
                quantumStep++;
            }
            for (int i = 0; i < stage.signalBars.Length; i++)
            {
                float amplitude = (float)research.GetCandidate(i).Amplitude;
                // The sign flips without passing through zero magnitude: MARK changes phase, not strength.
                displayedAmplitudes[i] = Mathf.Sign(amplitude) * Mathf.MoveTowards(Mathf.Abs(displayedAmplitudes[i]),
                    Mathf.Abs(amplitude), Time.unscaledDeltaTime * 1.6f);
                float height = .07f + Mathf.Abs(displayedAmplitudes[i]) * 2.2f;
                stage.signalBars[i].localScale = new Vector3(.13f, height, .13f);
                stage.signalBars[i].localPosition = new Vector3(-1.65f + i * .22f, 1.05f + height * .5f, 1.4f);
                LineRenderer wave = stage.signalWaves[i];
                for (int p = 0; p < wave.positionCount; p++)
                {
                    float x = p / (float)(wave.positionCount - 1);
                    wave.SetPosition(p, new Vector3(-1.65f + i * .22f + Mathf.Sin(x * Mathf.PI * 2f - t * 3f * Mathf.Sign(amplitude)) * .075f,
                        1.12f + x * 1.7f, 1.38f + displayedAmplitudes[i] * Mathf.Cos(x * 6.28f - t * 2f) * .3f));
                }
            }
            stage.researchDisplay.text = t >= 16.5f ? "YOU FOUND ME" : "ANVESHAK\n<size=48%>RARE-STATE RESEARCH ARRAY</size>";
            stage.researchDisplay.color = t >= 16.5f ? new Color(.89f, .72f, 1f) : new Color(.7f, .9f, .95f);
            stage.researchDisplay.transform.localPosition = new Vector3(Mathf.Sin(t * 29f) * .018f * reaction, 3.05f, 3.9f);
            float danger = Ease((t - 18.2f) / 1.5f);
            stage.researchKey.intensity = Mathf.Lerp(5.5f, 1.2f, danger);
            stage.researchAlarm.intensity = danger * (3f + Mathf.Sin(t * 7f) * 1.2f);
            if (t >= 19.5f && t - Time.unscaledDeltaTime < 19.5f && audioFeedback != null) audioFeedback.Alarm();
            for (int i = 0; i < stage.arrayRings.Length; i++)
                stage.arrayRings[i].localRotation = Quaternion.Euler(25f + i * 47f, t * (12f + i * 5f) * (1f + danger), i * 40f + danger * Mathf.Sin(t * 3f) * 9f);
            float assembly = Ease((t - 20.6f) / 1.4f) * (1f - Ease((t - 24f) / .8f));
            stage.anomaly.gameObject.SetActive(assembly > .001f);
            for (int i = 0; i < fragmentRest.Length; i++)
            {
                float a = t * 2f + i * 2.4f;
                Vector3 scatter = new Vector3(Mathf.Sin(a) * 2.6f, Mathf.Cos(a * .7f) * 1.3f, Mathf.Cos(a) * 1.8f);
                stage.anomalyFragments[i].localPosition = Vector3.Lerp(fragmentRest[i] + scatter, fragmentRest[i], assembly)
                    + Vector3.right * Mathf.Sin(t * 16f + i) * .028f * assembly;
                stage.anomalyFragments[i].localRotation = Quaternion.Euler((1f - assembly) * t * 35f, (1f - assembly) * i * 51f, (1f - assembly) * t * 19f);
            }
            for (int i = 0; i < stage.driftingMotes.Length; i++)
            {
                float cycle = Mathf.Repeat(t * (.12f + i % 3 * .025f) + i * .071f, 1f);
                stage.driftingMotes[i].localPosition = new Vector3(Mathf.Sin(i * 2.4f + t * .4f) * 3.6f, .4f + cycle * 3.4f,
                    1.6f + Mathf.Cos(i * 3.1f + t * .3f) * 2.5f);
            }
        }

        private void Ending(float t)
        {
            if (t < 4f)
            {
                SetStage(stage.cabin); CabinPose(t, false);
                float p = Ease(t / 3.2f);
                stage.cabinAnirudh.transform.localPosition = new Vector3(.5f, 0f, Mathf.Lerp(1.2f, -.65f, p));
                stage.cabinAnirudh.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                stage.cabinAnirudh.Pose(t, 1f - p, .25f * p, 0f, -10f);
                float close = Ease((t - 1f) / 2f);
                stage.craftDoorL.localPosition = new Vector3(-1.6f + close, 1.35f, 2.7f);
                stage.craftDoorR.localPosition = new Vector3(1.6f - close, 1.35f, 2.7f);
                Shot(stage.cabin, new Vector3(-3f, 1.8f, -3.6f), new Vector3(.4f, 1.35f, .2f), 48f);
            }
            else if (t < 11f)
            {
                SetStage(stage.exterior); AnimateExterior(t, Ease((t - 4f) / 4f));
                float p = Ease((t - 4f) / 7f);
                stage.exteriorCraft.localPosition = Vector3.Lerp(new Vector3(8.7f, 0f, -1.4f), new Vector3(17f, 1.5f, -18f), p);
                stage.exteriorCraft.localRotation = Quaternion.Euler(-4f * p, -20f - 22f * p, 8f * Mathf.Sin(p * Mathf.PI));
                Shot(stage.exterior, Vector3.Lerp(new Vector3(21f, 8f, -19f), new Vector3(24f, 6f, -26f), p),
                    Vector3.Lerp(Vector3.zero, stage.exteriorCraft.localPosition, p * .6f), 45f);
                if (t - Time.unscaledDeltaTime < 4f && audioFeedback != null) audioFeedback.Shutdown();
            }
            else if (t < 24f)
            {
                SetStage(stage.cabin); CabinPose(t, true);
                if (t >= 16f && t - Time.unscaledDeltaTime < 16f && audioFeedback != null) audioFeedback.Pulse();
                stage.cabinAnirudh.transform.localPosition = new Vector3(.5f, 0f, -.65f);
                stage.cabinAnirudh.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                float notice = Ease((t - 16f) / 1.5f);
                stage.cabinAnirudh.Pose(t, 0f, .25f + .65f * notice, Ease((t - 21f) / .7f) * .35f, -10f - notice * 25f, t >= 17.8f && t < 20f);
                if (t < 16f)
                    Shot(stage.cabin, Vector3.Lerp(new Vector3(-2.5f, 1.85f, -3.1f), new Vector3(-1.65f, 1.8f, -2.9f), Ease((t - 11f) / 5f)), new Vector3(.4f, 1.42f, -.85f), 47f);
                else
                    Shot(stage.cabin, Vector3.Lerp(new Vector3(-1.65f, 1.8f, -2.9f), new Vector3(-.85f, 1.65f, -2.7f), Ease((t - 16f) / 8f)), new Vector3(.25f, 1.35f, -1.25f), 44f);
            }
            else
            {
                SetStage(stage.exterior); AnimateExterior(t, 1f);
                stage.distantSignals.gameObject.SetActive(true);
                for (int i = 0; i < stage.distantSignals.childCount; i++)
                {
                    Transform signal = stage.distantSignals.GetChild(i);
                    float p = Ease((t - 24.5f - i * .09f) / 1.8f);
                    signal.localScale = Vector3.one * (.02f + p * (.1f + (i % 4) * .035f));
                    signal.localRotation = Quaternion.Euler(25f, t * 7f + i * 35f, 0f);
                }
                stage.exteriorCraft.localPosition = new Vector3(15f, 1f, -20f);
                Shot(stage.exterior, Vector3.Lerp(new Vector3(17f, 4f, -25f), new Vector3(11f, 7f, -27f), Ease((t - 24f) / 6f)), new Vector3(-7f, 5f, 58f), 49f);
            }
        }

        private void CabinPose(float t, bool activated)
        {
            float on = activated ? Ease((t - 16f) / 1f) : 0f;
            stage.wristSignal.gameObject.SetActive(on > .001f);
            stage.wristSignal.localScale = Vector3.one * (.18f + on * .06f);
            stage.wristSignal.localRotation = Quaternion.Euler(0f, t * 24f, 0f);
            stage.wristWave.enabled = on > .001f;
            for (int i = 0; i < stage.wristWave.positionCount; i++)
            {
                float x = i / (float)(stage.wristWave.positionCount - 1);
                stage.wristWave.SetPosition(i, new Vector3((x - .5f) * .6f, Mathf.Sin(x * Mathf.PI * 4f - t * 3f) * .08f * on, 0f));
            }
            stage.cabinDisplay.gameObject.SetActive(on > .1f);
            stage.cabinDisplay.text = t < 18.1f ? "SEARCH SPACE: UNKNOWN" : t < 19.5f
                ? "SEARCH SPACE: UNKNOWN\nTARGETS: 1"
                : "SEARCH SPACE: UNKNOWN\nTARGETS: 1\nPOSSIBILITIES: 7,942,381,625";
            stage.cabinKey.intensity = .9f + on * 1.2f;
        }

        private void AnimateExterior(float t, float shutdown)
        {
            stage.stationRing.localRotation = Quaternion.Euler(0f, t * .65f, 0f);
            stage.stationCore.localRotation = Quaternion.Euler(0f, t * 4f * (1f - shutdown), 0f);
            for (int i = 0; i < stage.exteriorEnergy.Length; i++)
                stage.exteriorEnergy[i].localScale = Vector3.one * Mathf.Lerp(1f, .02f, shutdown);
            stage.earth.localRotation = Quaternion.Euler(0f, t * .05f, 16f);
            if (!IsEnding) stage.exteriorCraft.localPosition = new Vector3(12f + Mathf.Sin(t * .07f) * 2f, .8f, 2f + Mathf.Cos(t * .07f) * 2f);
            stage.distantSignals.gameObject.SetActive(IsEnding && t >= 24f);
            stage.locationTitle.gameObject.SetActive(false);
        }

        private void SetStage(Transform active)
        {
            stage.exterior.gameObject.SetActive(active == stage.exterior);
            stage.control.gameObject.SetActive(active == stage.control);
            stage.docking.gameObject.SetActive(active == stage.docking);
            stage.cabin.gameObject.SetActive(active == stage.cabin);
        }

        private void Shot(Transform origin, Vector3 position, Vector3 target, float fov)
        {
            cinematicCamera.transform.position = origin.TransformPoint(position);
            cinematicCamera.transform.rotation = Quaternion.LookRotation(origin.TransformPoint(target) - cinematicCamera.transform.position, Vector3.up);
            cinematicCamera.fieldOfView = fov;
        }
        private static float Ease(float value) { value = Mathf.Clamp01(value); return value * value * (3f - 2f * value); }
    }
}
