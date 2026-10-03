using NullSignal.Player;
using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Two brief suit trails. Reads the existing dodge, without affecting movement or cooldowns.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(180)]
    public sealed class PlayerDodgeFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private TrailRenderer[] trails;
        [SerializeField] private Transform silhouette;
        private bool wasDodging;
        private float settle;
        private Vector3 restScale = Vector3.one;

        public void Configure(PlayerDodge source, TrailRenderer[] streaks, Transform visual)
        { dodge = source; trails = streaks; silhouette = visual; }

        private void Awake() { if (silhouette != null) restScale = silhouette.localScale; }
        private void OnEnable() { wasDodging = false; settle = 0f; ClearTrails(); }
        private void OnDisable() { ClearTrails(); if (silhouette != null) silhouette.localScale = restScale; }

        private void LateUpdate()
        {
            bool active = dodge != null && dodge.IsDodging;
            if (active && !wasDodging) { ClearTrails(); settle = 1f; }
            wasDodging = active;
            if (trails != null) foreach (TrailRenderer trail in trails) if (trail != null) trail.emitting = active;
            settle = Mathf.MoveTowards(settle, active ? 1f : 0f, Time.deltaTime * 7f);
            if (silhouette != null)
            {
                // Small compression reads as a committed dash, then returns to the original silhouette.
                silhouette.localScale = Vector3.Scale(restScale, new Vector3(1f + settle * .035f, 1f - settle * .045f, 1f + settle * .035f));
            }
        }

        private void ClearTrails()
        {
            if (trails == null) return;
            foreach (TrailRenderer trail in trails) if (trail != null) { trail.emitting = false; trail.Clear(); }
        }
    }
}
