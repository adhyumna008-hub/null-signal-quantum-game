using System.Collections;
using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using UnityEngine;

namespace NullSignal.Story
{
    public enum FinaleStage { Idle, Searching, Recovering, Reveal, Boss, Payoff, AwaitingShutdown, ShuttingDown, Escape, Ending }

    /// <summary>Final route lifecycle. QuantumEncounter still owns every operation and measurement.</summary>
    public sealed class FinaleRoomController : MonoBehaviour
    {
        [SerializeField] private StoryQuantumRoom room;
        [SerializeField] private MainGameDirector director;
        [SerializeField] private StationCamera cameraRig;
        [SerializeField] private ArenaSurgeHazard hazard;
        [SerializeField] private GameObject ananya, lubna, reconstruction;
        [SerializeField] private Transform signal, silhouette;
        [SerializeField] private QuantumEncounter demonstration;
        [SerializeField] private AnveshakCorePresentation core;
        private PlayerHealth health;
        private bool entered;
        private float signalSize, revealClock, nextObjective;
        private Vector3 signalRest, silhouetteRest;
        public FinaleStage Stage { get; private set; }
        public float Remaining { get; private set; }
        public float EndingAt { get; private set; }
        public void Configure(StoryQuantumRoom section, MainGameDirector owner, StationCamera camera, ArenaSurgeHazard pressure,
            GameObject firstHologram, GameObject secondHologram, Transform waveform, Transform figure,
            QuantumEncounter payoff, GameObject payoffVisuals, AnveshakCorePresentation apparatus)
        {
            room = section; director = owner; cameraRig = camera; hazard = pressure; ananya = firstHologram; lubna = secondHologram;
            signal = waveform; silhouette = figure; demonstration = payoff; reconstruction = payoffVisuals; core = apparatus;
        }
        private void Awake()
        {
            health = director.Player.GetComponent<PlayerHealth>();
            if (signal != null) { signalRest = signal.localPosition; signal.gameObject.SetActive(false); }
            if (silhouette != null) { silhouetteRest = silhouette.localPosition; silhouette.gameObject.SetActive(false); }
            if (reconstruction != null) reconstruction.SetActive(false);
        }
        public void Enter()
        {
            entered = true; health.Restore(); director.Anvesh.ShowTeaching = false;
            if (room.Kind == StoryRoomKind.Vault)
            {
                ResetVault(true);
                director.Subtitles.Play("vault-entry", new SubtitleCue("TARA", "Classical diagnostics can test the sectors individually.", 3.5f),
                    new SubtitleCue("ANIRUDH", "No time.", 1.5f), new SubtitleCue("TARA", "Then don't search them one by one.", 3f));
            }
            else if (room.Kind == StoryRoomKind.Prime)
            { Stage = FinaleStage.Boss; room.Combat.Enter(); }
            else { Stage = FinaleStage.Escape; director.Anvesh.SetSuppressed(true); EscapeObjective(); }
        }
        public void Leave()
        {
            entered = false; StopAllCoroutines(); cameraRig.SequenceArc(0); hazard?.SetPressure(false);
            if (ananya != null) ananya.SetActive(false); if (lubna != null) lubna.SetActive(false);
        }
        private void OnDisable() { StopAllCoroutines(); if (cameraRig != null) cameraRig.SequenceArc(0); }
        public void ResetRoom()
        {
            // Story success is persistent; R cannot strand a resolved room halfway through a sequence.
            if (room.Completed || Stage == FinaleStage.Reveal || Stage == FinaleStage.Payoff || Stage == FinaleStage.AwaitingShutdown
                || Stage == FinaleStage.ShuttingDown || Stage == FinaleStage.Ending) return;
            if (room.Kind == StoryRoomKind.Vault) { StopAllCoroutines(); ResetVault(true); }
            else if (room.Kind == StoryRoomKind.Prime)
            { health.Restore(); Stage = FinaleStage.Boss; room.Combat.Restart(); hazard.SetPressure(false); }
            else EscapeObjective();
        }
        private void ResetVault(bool full)
        {
            director.Anvesh.Operations?.Cancel();
            if (full) { Remaining = 60f; health.Restore(); }
            room.Encounter.Configure(16, 0, true); room.Encounter.RestartEncounter();
            director.Anvesh.Configure(room.Encounter); director.Anvesh.PermitLock(true); director.Anvesh.SetSuppressed(false);
            Stage = FinaleStage.Searching; hazard.SetPressure(false); nextObjective = 0;
        }
        private void Update()
        {
            if (!entered || director.CurrentRoom != room) return;
            if (hazard != null) hazard.SetPressure(health.Current > 0 && (Stage == FinaleStage.Searching
                || Stage == FinaleStage.Boss && room.Combat.Running && !room.Combat.Vulnerable),
                Stage == FinaleStage.Boss ? 6f - room.Combat.Phase : 5f);
            if (Stage == FinaleStage.Searching && health.Current > 0)
            {
                Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);
                // Wait for LOCK's visible resolution before changing route state or replacing the distribution.
                if (room.Encounter.HasMeasured && !(director.Anvesh.Operations?.Busy ?? false))
                {
                    if (room.Encounter.MeasurementSucceeded == true) StartCoroutine(Reveal());
                    else StartCoroutine(RecoverVault(false));
                }
                else if (Remaining <= 0f) StartCoroutine(RecoverVault(true));
                else if (Time.time >= nextObjective)
                {
                    nextObjective = Time.time + .25f;
                    director.Objectives.Set("RECOVER ORIGINAL TRANSMISSION", "16 memory sectors / Avoid amber surges / Measure the original signal / R restarts this attempt");
                }
            }
            if (Stage == FinaleStage.Reveal)
            {
                revealClock += Time.deltaTime;
                signal.localScale = Vector3.Lerp(signal.localScale, Vector3.one * signalSize, 1f - Mathf.Exp(-Time.deltaTime * 2f));
                signal.localPosition = signalRest + Vector3.up * Mathf.Sin(revealClock * 1.5f) * .15f;
                signal.Rotate(0, 32f * Time.deltaTime, 9f * Time.deltaTime);
                if (silhouette.gameObject.activeSelf)
                {
                    silhouette.localScale = Vector3.Lerp(silhouette.localScale, Vector3.one, Time.deltaTime * 1.8f);
                    silhouette.localPosition = silhouetteRest + Vector3.up * Mathf.Sin(revealClock * 2f) * .12f;
                    silhouette.localRotation = Quaternion.Euler(0, 45f + Mathf.Sin(revealClock) * 12f, 0);
                }
            }
        }
        private IEnumerator RecoverVault(bool expired)
        {
            Stage = FinaleStage.Recovering;
            if (expired) director.Anvesh.Operations?.Cancel();
            director.Anvesh.SetSuppressed(true); hazard.SetPressure(false);
            if (!expired) { Remaining = Mathf.Max(0, Remaining - 8f); health.Damage(8); }
            director.Objectives.Set(expired ? "ARCHIVE COLLAPSE / LOCAL RETRY" : "WRONG SECTOR / -8 SECONDS",
                expired ? "Restoring this room's checkpoint..." : "Distribution recovering. Keep moving; another attempt starts now.");
            yield return new WaitForSeconds(1.5f);
            if (health.Current <= 0) yield break; // The existing checkpoint owns death recovery.
            ResetVault(expired || Remaining <= 0);
        }
        private IEnumerator Line(string who, string words, float seconds)
        {
            director.Subtitles.Play(null, new SubtitleCue(who, words, seconds));
            yield return new WaitForSeconds(seconds + .05f);
        }
        private IEnumerator Reveal()
        {
            Stage = FinaleStage.Reveal; hazard.SetPressure(false); director.Anvesh.SetSuppressed(true); health.Restore();
            director.Subtitles.Clear(); director.Objectives.Set("ORIGINAL TRANSMISSION RECOVERED", "The archive is reconstructing the experiment. You can keep moving.");
            ananya.SetActive(true); lubna.SetActive(true); signal.gameObject.SetActive(true);
            signal.localScale = Vector3.one * .025f; signalSize = .07f; revealClock = 0;
            yield return Line("ANANYA", "Anveshak wasn't searching for extraterrestrial signals.", 3.5f);
            yield return Line("ANANYA", "We were testing how far amplitude amplification could push an almost undetectable marked state.", 5.5f);
            yield return Line("LUBNA", "There was one.", 1.8f);
            yield return Line("ANANYA", "Its initial amplitude was almost zero.", 3f);
            signalSize = .6f; yield return Line("ANANYA", "We amplified it.", 2.5f);
            signalSize = 1.1f; yield return Line("ANANYA", "Again.", 2.5f);
            signalSize = 1.6f; silhouette.localScale = Vector3.one * .02f; silhouette.gameObject.SetActive(true);
            yield return Line("ANANYA", "And again.", 2.5f);
            yield return Line("ANANYA", "Until it could observe us back.", 3.5f);
            yield return Line("ANIRUDH", "We didn't just find it...", 2.5f);
            yield return Line("TARA", "We may have amplified it into a stable manifestation.", 3.5f);
            Stage = FinaleStage.Idle; room.Finish();
        }
        public void BossDefeated()
        {
            if (Stage != FinaleStage.Boss) return;
            Stage = FinaleStage.Payoff; hazard.SetPressure(false); health.Restore();
            director.Anvesh.Operations?.Cancel(); director.Anvesh.SetSuppressed(true);
            StartCoroutine(ScientificPayoff());
        }
        private IEnumerator ScientificPayoff()
        {
            director.Subtitles.Clear(); director.Objectives.Set("APPROACH ANVESHAK CORE", "Chhaya Prime dispersed. Follow the four-state reconstruction to the core console.");
            reconstruction.SetActive(true); demonstration.Configure(4, 0, false); demonstration.RestartEncounter();
            cameraRig.SetRoom(room.Center, 4);
            director.Anvesh.Configure(demonstration); director.Anvesh.SetSuppressed(true);
            demonstration.Scan();
            yield return Line("TARA", "Equal starting amplitudes.", 2.7f);
            demonstration.Mark(); yield return Line("TARA", "Oracle phase marking.", 2.7f);
            demonstration.Amplify(); yield return Line("TARA", "Amplitude amplification.", 2.7f);
            demonstration.Lock(); yield return Line("TARA", "Measurement.", 2.5f);
            yield return Line("TARA", "Quantum amplitude amplification.", 2.5f);
            yield return Line("TARA", "Grover search is one application of the same principle.", 3.5f);
            yield return Line("ANIRUDH", "That's what Anveshak was doing.", 2.5f);
            yield return Line("TARA", "Yes.", 1.3f);
            Stage = FinaleStage.AwaitingShutdown;
            director.Objectives.Set("SHUT DOWN ANVESHAK", "Approach the cyan core control at the central apparatus. E to shut down.");
        }
        public bool CanUseDevice(bool extraction) => entered && director.CurrentRoom == room && health.Current > 0
            && (extraction ? Stage == FinaleStage.Escape : Stage == FinaleStage.AwaitingShutdown);
        public void UseDevice(bool extraction)
        {
            if (!CanUseDevice(extraction)) return;
            if (extraction)
            {
                Stage = FinaleStage.Ending; EndingAt = Time.unscaledTime;
                director.Anvesh.Operations?.Cancel(); director.Anvesh.SetSuppressed(true);
                director.Player.GetComponent<PlayerController>().enabled = false;
                director.Player.GetComponent<PlayerCombat>().enabled = false;
                director.Subtitles.Clear(); room.Finish();
            }
            else StartCoroutine(Shutdown());
        }
        private IEnumerator Shutdown()
        {
            Stage = FinaleStage.ShuttingDown; director.Subtitles.Clear();
            director.GetComponent<StationFeedbackAudio>()?.Shutdown();
            director.Subtitles.Play(null, new SubtitleCue("TARA", "Target amplitude collapsing.", 3f));
            director.Objectives.Set("ANVESHAK SHUTDOWN", "Stand by. Emergency route powering up.");
            float elapsed = 0;
            while (elapsed < 5f)
            {
                elapsed += Time.deltaTime; core.Shutdown(Mathf.Clamp01(elapsed / 5f));
                reconstruction.transform.localScale = Vector3.one * Mathf.Clamp01(1f - elapsed / 3f);
                yield return null;
            }
            core.Shutdown(1f); reconstruction.SetActive(false); Stage = FinaleStage.Escape;
            director.Anvesh.Configure(null); director.Anvesh.SetSuppressed(true); room.Finish();
            director.Subtitles.Play(null, new SubtitleCue("TARA", "Emergency craft ready. East passage. Move.", 3f));
        }
        private void EscapeObjective() => director.Objectives.Set("ESCAPE ASTRA-7", "Follow the amber route to the emergency craft. E to board.");
    }
}
