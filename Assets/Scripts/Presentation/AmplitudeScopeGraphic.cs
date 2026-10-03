using NullSignal.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    /// <summary>Small live signed-amplitude oscilloscope; data comes exclusively from the encounter.</summary>
    public sealed class AmplitudeScopeGraphic : MaskableGraphic
    {
        [SerializeField] private QuantumEncounter encounter;
        public void Configure(QuantumEncounter owner) { encounter = owner; raycastTarget = false; SetVerticesDirty(); }
        private void Update() { if (encounter != null && encounter.HasScanned) SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (encounter == null || !encounter.IsInitialized) return;
            Rect rect = rectTransform.rect;
            float cell = rect.width / encounter.CandidateCount;
            for (int c = 0; c < encounter.CandidateCount; c++)
            {
                float left = rect.xMin + c * cell + 8f, right = left + cell - 16f;
                float mid = rect.center.y;
                Stroke(vh, new Vector2(left, mid), new Vector2(right, mid), 1f, new Color(0.25f, 0.4f, 0.48f, 0.6f));
                if (!encounter.HasScanned) continue;
                float amplitude = (float)encounter.GetCandidate(c).Amplitude;
                float sign = amplitude < 0f ? -1f : 1f;
                Vector2 previous = Vector2.zero;
                for (int i = 0; i < 33; i++)
                {
                    float t = i / 32f;
                    Vector2 point = new Vector2(Mathf.Lerp(left, right, t), mid + amplitude * rect.height * 0.43f * Mathf.Sin(t * Mathf.PI * 4f - Time.time * 2.8f * sign));
                    if (i > 0) Stroke(vh, previous, point, 1.6f, color);
                    previous = point;
                }
            }
        }
        private static void Stroke(VertexHelper vh, Vector2 a, Vector2 b, float width, Color tint)
        {
            Vector2 direction = b - a;
            Vector2 normal = new Vector2(-direction.y, direction.x).normalized * width * 0.5f;
            int start = vh.currentVertCount;
            vh.AddVert(a - normal, tint, Vector2.zero); vh.AddVert(a + normal, tint, Vector2.zero);
            vh.AddVert(b + normal, tint, Vector2.zero); vh.AddVert(b - normal, tint, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2); vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
