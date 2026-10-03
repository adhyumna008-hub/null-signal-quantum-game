using NullSignal.Player;
using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Observes successful actions, so blocked input never produces a misleading cue.</summary>
    public sealed class PlayerAudioFeedback : MonoBehaviour
    {
        [SerializeField] private StationFeedbackAudio feedback;
        private PlayerDodge dodge;
        private PlayerCombat combat;
        private PlayerHealth health;
        private bool wasDodging;
        private float previousAttack;
        private int previousHealth;
        public void Configure(StationFeedbackAudio audio) => feedback = audio;
        private void Awake()
        { dodge = GetComponent<PlayerDodge>(); combat = GetComponent<PlayerCombat>(); health = GetComponent<PlayerHealth>(); }
        private void OnEnable()
        { if (health != null) { previousHealth = health.Current; health.Changed += OnHealthChanged; } }
        private void OnDisable() { if (health != null) health.Changed -= OnHealthChanged; }
        private void OnHealthChanged()
        { if (health.Current < previousHealth) feedback?.Hit(); previousHealth = health.Current; }
        private void LateUpdate()
        {
            if (Time.timeScale <= 0f) return;
            bool dodging = dodge != null && dodge.IsDodging;
            if (dodging && !wasDodging) feedback?.Dodge(); wasDodging = dodging;
            float attack = combat != null ? combat.AttackPose : 0f;
            if (attack > previousAttack + .25f) feedback?.Attack(); previousAttack = attack;
        }
    }
}
