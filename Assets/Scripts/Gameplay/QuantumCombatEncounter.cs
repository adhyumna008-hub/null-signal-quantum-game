using System.Collections;
using NullSignal.Player;
using NullSignal.Presentation;
using NullSignal.Story;
using UnityEngine;

namespace NullSignal.Gameplay
{
    public sealed class QuantumCombatEncounter : MonoBehaviour
    {
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private CombatManifestation[] actors;
        [SerializeField] private StoryQuantumRoom room;
        [SerializeField] private MainGameDirector director;
        [SerializeField] private Transform player;
        [SerializeField] private StationCamera cameraRig;
        [SerializeField] private Light[] lights;
        [SerializeField] private bool chhaya;
        [SerializeField] private bool prime;
        private PlayerHealth health;
        private PlayerDodge dodge;
        private bool active, introducing, measured, phaseB;
        private float until, nextStatus;
        private int primePhase;
        private float nextThreatSlot;
        public bool Prime => prime;
        public int Phase => prime ? primePhase + 1 : phaseB ? 2 : 1;
        public bool Introducing => introducing;
        public float IntroProgress => introducing ? Mathf.Clamp01(1f - (until - Time.time) / (prime ? 10f : 3f)) : 1f;
        public float ExposureRemaining => Vulnerable ? Mathf.Max(0, until - Time.time) : 0;
        public float OrbitRadius => prime ? (encounter.CandidateCount >= 16 ? 7.1f : encounter.CandidateCount >= 8 ? 6.5f : 5.5f) : 4.9f;
        private int PhaseCount => prime ? 4 << primePhase : phaseB ? 8 : 4;
        public bool Running => active && !introducing && health != null && health.Current > 0;
        public bool Chhaya => chhaya;
        public bool Vulnerable => Running && measured && encounter.MeasurementSucceeded == true && Time.time < until;
        public bool HostileDecoy => Running && measured && encounter.MeasurementSucceeded == false;
        public float Vitality { get; private set; }
        public float MaxVitality => prime ? 100f + primePhase * 25f : chhaya ? (phaseB ? 150f : 100f) : 100f;
        public float Formation => introducing && !prime ? IntroProgress : 1f;
        public QuantumEncounter Search => encounter;
        public Transform Player => player;
        public Vector3 Center => room.Center;
        public bool Active => active;

        public void Configure(QuantumEncounter search, CombatManifestation[] manifestations, StoryQuantumRoom section,
            MainGameDirector owner, Transform target, StationCamera camera, Light[] practicals, bool isChhaya, bool isPrime = false)
        {
            encounter = search; actors = manifestations; room = section; director = owner;
            player = target; cameraRig = camera; lights = practicals; chhaya = isChhaya; prime = isPrime;
        }
        private void Awake() { health = player.GetComponent<PlayerHealth>(); dodge = player.GetComponent<PlayerDodge>(); }
        private void OnEnable() { if (encounter != null) encounter.Changed += OnSearchChanged; }
        private void OnDisable() { if (encounter != null) encounter.Changed -= OnSearchChanged; StopAllCoroutines(); }
        public void Enter()
        {
            if (room.Completed) return;
            active = true; Restart();
            if (!prime) director.Subtitles.Play(chhaya ? "chhaya-intro" : "drone-intro",
                new SubtitleCue(chhaya ? "LUBNA" : "ANANYA", chhaya ? "The corruption is holding several positions at once." : "Security is searching you. Collapse the false states, then break the exposed core.", 4f));
            if (chhaya) StartCoroutine(Introduction());
        }
        public void Leave() { active = false; introducing = false; StopAllCoroutines(); cameraRig.SequenceArc(0); foreach (var actor in actors) actor.ClearAttack(); }
        public void Restart()
        {
            if (room.Completed) return;
            StopAllCoroutines(); introducing = false; active = true; phaseB = false; primePhase = 0; Vitality = MaxVitality;
            cameraRig.SequenceArc(0);
            director.Anvesh.SetSuppressed(false); director.Anvesh.PermitLock(true);
            ResetSearch(4); Status();
        }
        private IEnumerator Introduction()
        {
            introducing = true; until = Time.time + (prime ? 10f : 3f); director.Anvesh.SetSuppressed(true);
            Status();
            while (Time.time < until)
            {
                float progress = IntroProgress;
                if (prime) cameraRig.SequenceArc(Mathf.Sin(progress * Mathf.PI));
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].color = new Color(.49f, .23f, .85f);
                    lights[i].intensity = progress < .3f ? .08f : .35f + progress * .9f + Mathf.Sin(Time.time * 19f) * .12f;
                }
                yield return null;
            }
            introducing = false; director.Anvesh.SetSuppressed(false);
            cameraRig.SequenceArc(0);
            foreach (var actor in actors) actor.ResetThreat();
            director.Subtitles.Play(prime ? "prime-search" : "chhaya-search", new SubtitleCue("CHHAYA", "Search.", 2f));
        }
        private void ResetSearch(int count)
        {
            director.Anvesh.Operations?.Cancel(); measured = false;
            encounter.Configure(count, 0, true); encounter.RestartEncounter();
            foreach (var actor in actors) actor.ResetThreat();
            nextThreatSlot = Time.time + 1.6f;
            cameraRig.SetRoom(room.Center, count);
        }
        private void OnSearchChanged()
        {
            if (!Running || !encounter.HasMeasured || measured) return;
            measured = true; until = Time.time + (encounter.MeasurementSucceeded == true ? 5f : 2.6f);
            foreach (var actor in actors) actor.ClearAttack();
            cameraRig.Impulse(encounter.MeasurementSucceeded == true ? .08f : .11f);
            Status();
        }
        private void Update()
        {
            if (!Running) return;
            if (measured && Time.time >= until) { ResetSearch(PhaseCount); Status(); }
            if (Time.time >= nextStatus) { nextStatus = Time.time + .2f; Status(); }
        }
        private void Status()
        {
            if (!active || room.Completed) return;
            string title = prime ? "DEFEAT CHHAYA PRIME / PHASE " + Phase : chhaya ? "CHHAYA / " + (phaseB ? "EIGHT POSSIBILITIES" : "FOUR POSSIBILITIES") : "SECURITY LOCKDOWN";
            string hint = introducing ? "Stay moving. A presence is forming."
                : Vulnerable ? $"EXPOSED {Mathf.Max(0, until - Time.time):0.0}s  /  LMB fire at the surviving core  /  HP {Vitality:0}/{MaxVitality:0}"
                : HostileDecoy ? "WRONG STATE / evade the hostile decoy. Search recovers automatically."
                : $"Q SCAN  ·  1 MARK  ·  2 AMPLIFY  ·  3 LOCK  /  SPACE dodge  /  HP {Vitality:0}/{MaxVitality:0}";
            director.Objectives.Set(title, hint);
        }
        public bool CanThreaten(int index) => Running && index < encounter.CandidateCount && (!measured || HostileDecoy && encounter.MeasuredCandidateIndex == index);
        public bool TryBeginThreat(int index)
        {
            if (!CanThreaten(index) || prime && Time.time < nextThreatSlot) return false;
            nextThreatSlot = Time.time + Mathf.Lerp(1.15f, .85f, primePhase / 2f);
            return true;
        }
        public bool Damage(int index, float amount)
        {
            if (!Vulnerable || encounter.MeasuredCandidateIndex != index) return false;
            Vitality = Mathf.Max(0, Vitality - amount); cameraRig.Impulse(.06f);
            if (Vitality <= 0)
            {
                if (prime && primePhase < 2)
                {
                    primePhase++; Vitality = MaxVitality; health.Restore(); ResetSearch(PhaseCount);
                    director.Subtitles.Clear();
                    if (primePhase == 2) director.Subtitles.Play(null,
                        new SubtitleCue("TARA", "Anirudh...", 2.7f), new SubtitleCue("TARA", "You know what to do.", 3f));
                    else director.Subtitles.Play(null, new SubtitleCue("ANVESH", "Eight manifestations. Shield reformed.", 2.5f));
                }
                else if (chhaya && !prime && !phaseB)
                {
                    phaseB = true; Vitality = MaxVitality; ResetSearch(8);
                    director.Subtitles.Play("chhaya-eight", new SubtitleCue("ANVESH", "Search space expanded. Eight states.", 2.5f));
                }
                else
                {
                    active = false; foreach (var actor in actors) actor.ClearAttack();
                    if (prime && room.Finale != null) room.Finale.BossDefeated(); else room.Finish();
                }
            }
            Status(); return true;
        }
        public void Strike(Vector3 position)
        {
            if (!Running || dodge.IsDodging) return;
            Vector3 delta = player.position - position; delta.y = 0;
            if (delta.sqrMagnitude > 1.15f * 1.15f) return;
            float before = health.Current; health.Damage(prime ? 9 : chhaya ? 14 : 12);
            if (health.Current < before) { cameraRig.Impulse(.1f); player.GetComponent<PlayerCombat>()?.FlashDamage(); }
        }
    }
}
