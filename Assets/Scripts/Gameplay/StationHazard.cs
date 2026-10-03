using System.Collections;
using NullSignal.Player;
using NullSignal.Presentation;
using UnityEngine;

namespace NullSignal.Gameplay
{
    public sealed class StationHazard : MonoBehaviour
    {
        [SerializeField] private Transform player, fallingPanel;
        [SerializeField] private Renderer warning;
        [SerializeField] private SubtitleController subtitles;
        [SerializeField] private StationCamera cameraRig;
        [SerializeField] private bool repeat;
        private bool active, spent;
        private float nextAllowed;
        public void Configure(Transform actor, Transform panel, Renderer danger, SubtitleController dialogue, StationCamera camera, bool recurring)
        { player = actor; fallingPanel = panel; warning = danger; subtitles = dialogue; cameraRig = camera; repeat = recurring; }
        private void Update()
        {
            Vector3 offset = player.position - transform.position; offset.y = 0f;
            if (!active && !spent && Time.time > nextAllowed && offset.sqrMagnitude < 14f) StartCoroutine(Discharge());
        }
        private IEnumerator Discharge()
        {
            active = true; warning.enabled = true;
            subtitles.Play("hazard-introduction", new SubtitleCue("TARA", "Panel failure. Move clear — Space to dodge.", 3.5f));
            float clock = 0f;
            while (clock < 1.25f)
            {
                clock += Time.deltaTime; warning.enabled = Mathf.Repeat(clock * 5f, 1f) > 0.25f;
                yield return null;
            }
            warning.enabled = true;
            while (fallingPanel.localPosition.y > 0.18f)
            { fallingPanel.localPosition = Vector3.MoveTowards(fallingPanel.localPosition, new Vector3(0f, 0.18f, 0f), Time.deltaTime * 18f); yield return null; }
            Vector3 delta = player.position - transform.position;
            if (Mathf.Abs(delta.x) < 0.9f && Mathf.Abs(delta.z) < 1.7f && !player.GetComponent<PlayerDodge>().IsDodging)
            {
                if (player.GetComponent<PlayerHealth>().Damage(repeat ? 25 : 15)) cameraRig.Impulse(0.10f);
            }
            yield return new WaitForSeconds(0.6f);
            warning.enabled = false;
            if (repeat) fallingPanel.localPosition = new Vector3(0f, 4f, 0f);
            else fallingPanel.gameObject.SetActive(false);
            spent = !repeat; active = false; nextAllowed = Time.time + 5f;
        }
    }
}
