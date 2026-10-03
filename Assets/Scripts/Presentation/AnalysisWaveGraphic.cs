using NullSignal.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    /// <summary>Interpolates presentation between captured real states; never feeds values into simulation.</summary>
    public sealed class AnalysisWaveGraphic : MaskableGraphic
    {
        [SerializeField] private AnveshOperationController operations;
        public void Configure(AnveshOperationController source) { operations = source; raycastTarget = false; }
        private void Update() => SetVerticesDirty();
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); if (operations == null || operations.Before == null) return;
            Rect rect = rectTransform.rect; double[] before = operations.Before, after = operations.After;
            float blend = after == null ? 0 : Mathf.Clamp01((Time.unscaledTime - operations.DataAt) / .28f);
            float row = rect.height / before.Length;
            for (int i = 0; i < before.Length; i++)
            {
                float amplitude = Mathf.Lerp((float)before[i], after == null ? (float)before[i] : (float)after[i], blend);
                float y = rect.yMax - (i + .5f) * row;
                Segment(mesh, new Vector2(rect.xMin, y), new Vector2(rect.xMax, y), 1, new Color(.2f, .4f, .5f, .35f));
                Color tint = amplitude < 0 ? new Color(.7f, .45f, 1f) : new Color(.3f, .9f, 1f);
                Vector2 last = Vector2.zero;
                for (int p = 0; p < 41; p++)
                {
                    float x = p / 40f;
                    Vector2 point = new Vector2(rect.xMin + rect.width * x, y + Mathf.Sin(x * Mathf.PI * 4f - Time.unscaledTime * 3f) * amplitude * row * .42f);
                    if (p > 0) Segment(mesh, last, point, 2f, tint); last = point;
                }
            }
        }
        private static void Segment(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 n = new Vector2(-(b - a).y, (b - a).x).normalized * width * .5f;
            int index = mesh.currentVertCount;
            mesh.AddVert(a - n, color, Vector2.zero); mesh.AddVert(a + n, color, Vector2.zero);
            mesh.AddVert(b + n, color, Vector2.zero); mesh.AddVert(b - n, color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
