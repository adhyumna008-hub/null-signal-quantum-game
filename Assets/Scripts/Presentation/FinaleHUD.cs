using NullSignal.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    public sealed class FinaleHUD : MonoBehaviour
    {
        [SerializeField] private MainGameDirector director;
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private TMP_Text heading, status, endingTitle, endingWords;
        [SerializeField] private Image meter;
        [SerializeField] private CanvasGroup ending;
        [SerializeField] private bool cinematicEnding;
        public void UseCinematicEnding() => cinematicEnding = true;
        public void Configure(MainGameDirector owner, GameObject panel, TMP_Text title, TMP_Text detail, Image bar,
            CanvasGroup fade, TMP_Text finaleTitle, TMP_Text finaleWords)
        { director = owner; statusPanel = panel; heading = title; status = detail; meter = bar; ending = fade; endingTitle = finaleTitle; endingWords = finaleWords; }
        private void LateUpdate()
        {
            StoryQuantumRoom room = director.CurrentRoom;
            FinaleRoomController finale = room.Finale;
            bool vault = finale != null && room.Kind == StoryRoomKind.Vault && (finale.Stage == FinaleStage.Searching || finale.Stage == FinaleStage.Recovering);
            bool boss = room.Combat != null && room.Combat.Prime && room.Combat.Active;
            statusPanel.SetActive(vault || boss);
            if (vault)
            {
                int seconds = Mathf.CeilToInt(finale.Remaining);
                heading.text = $"SYSTEM COLLAPSE: {seconds / 60:00}:{seconds % 60:00}";
                status.text = "ORIGINAL TRANSMISSION / 16 MEMORY SECTORS";
                meter.fillAmount = finale.Remaining / 60f;
                meter.color = finale.Remaining < 15f ? new Color(1f, .35f, .15f) : new Color(.3f, .85f, 1f);
            }
            else if (boss)
            {
                var combat = room.Combat;
                heading.text = $"CHHAYA PRIME / PHASE {combat.Phase} OF 3 / {combat.Vitality:0} HP";
                status.text = combat.Introducing ? "MANIFESTATION DESTABILIZING"
                    : combat.Vulnerable ? $"QUANTUM SHIELD COLLAPSED / {combat.ExposureRemaining:0.0}s / LMB ATTACK"
                    : $"QUANTUM SHIELD ACTIVE / {combat.Search.CandidateCount} MANIFESTATIONS";
                meter.fillAmount = combat.Vitality / combat.MaxVitality;
                meter.color = combat.Vulnerable ? new Color(1f, .68f, .3f) : new Color(.64f, .4f, .95f);
            }
            bool finished = !cinematicEnding && finale != null && finale.Stage == FinaleStage.Ending;
            ending.alpha = finished ? Mathf.Clamp01((Time.unscaledTime - finale.EndingAt) / 1.5f) : 0;
            ending.blocksRaycasts = finished;
            if (!finished) return;
            float age = Time.unscaledTime - finale.EndingAt;
            endingTitle.text = age < 4.5f ? "MISSION COMPLETE" : age < 10.5f ? "ANVESHAK / LAST TRANSMISSION" : "NULL SIGNAL";
            endingWords.text = age < 4.5f ? "EMERGENCY EXTRACTION CONFIRMED"
                : age < 10.5f ? "SEARCH SPACE: UNKNOWN\n\nTARGETS: 1\n\nPOSSIBILITIES: 7,942,381,625"
                : "THE SEARCH HAS ONLY BEGUN";
        }
    }
}
