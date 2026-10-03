using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Story;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    public sealed class StoryHudOverlay : MonoBehaviour
    {
        [SerializeField] private MainGameDirector director;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Text healthText, prompt, area, unlockText;
        [SerializeField] private Image unlockPanel;
        private PlayerInteraction interaction;
        public void Configure(MainGameDirector owner, PlayerHealth vitals, Text healthLabel, Text context, Text location, Text unlock, Image pulse)
        { director = owner; health = vitals; healthText = healthLabel; prompt = context; area = location; unlockText = unlock; unlockPanel = pulse; }
        private void Start() => interaction = director.Player.GetComponent<PlayerInteraction>();
        private void Update()
        {
            healthText.text = $"VITALS  {health.Current:000} / {health.Maximum:000}";
            healthText.color = health.Current < 40 ? new Color(1f, 0.65f, 0.32f) : new Color(0.63f, 0.89f, 0.93f);
            area.text = director.Objectives.Area;
            float pulse = Mathf.Clamp01(1f - (Time.unscaledTime - director.UnlockAt) / 2f);
            unlockPanel.gameObject.SetActive(pulse > 0f);
            if (pulse > 0f)
            {
                unlockPanel.color = new Color(0.03f, 0.20f, 0.26f, pulse * 0.92f);
                unlockText.text = "ANVESH / ABILITY AVAILABLE"; unlockText.color = new Color(0.6f, 0.95f, 1f, pulse);
            }
            prompt.text = interaction != null ? interaction.Prompt : string.Empty;
        }
    }
}
