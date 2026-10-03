using NullSignal.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullSignal.Presentation
{
    /// <summary>Presentation envelopes only. Probability and amplitude remain owned by the simulation.</summary>
    public static class QuantumPresentationEffects
    {
        public static readonly Color Science = new Color(.36f, .88f, 1f);
        public static readonly Color Success = new Color(.48f, .95f, .68f);
        public static readonly Color Failure = new Color(.97f, .36f, .30f);
        public static readonly Color Redistribution = new Color(.98f, .75f, .43f);
        public static Color MeasuredColor(bool? succeeded) => succeeded.HasValue ? succeeded.Value ? Success : Failure : Science;
        public static float MotionRate(QuantumEncounter encounter)
        {
            var operation = encounter.PresentationOperations;
            return operation != null && operation.Busy && operation.Action == AnveshAbility.Lock && !operation.Committed ? .18f : 1f;
        }
        public static float Anticipation(QuantumEncounter encounter)
        {
            var operation = encounter.PresentationOperations;
            return operation != null && operation.Busy && operation.Action == AnveshAbility.Lock && !operation.Committed
                ? Mathf.Sin(Mathf.Clamp01(operation.Progress / .35f) * Mathf.PI) : 0;
        }
        public static LineRenderer Beam(Transform parent, Material material)
        {
            var line = new GameObject("Measured state / vertical quantum pulse").AddComponent<LineRenderer>();
            line.transform.SetParent(parent, false); line.sharedMaterial = material;
            line.useWorldSpace = false; line.positionCount = 2; line.enabled = false;
            line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false; line.numCapVertices = 2;
            return line;
        }
        public static void DrawBeam(LineRenderer beam, bool measured, float age, bool? succeeded = null)
        {
            beam.enabled = measured && age < .6f;
            if (!beam.enabled) return;
            float envelope = Mathf.Sin(Mathf.Clamp01(age / .6f) * Mathf.PI);
            beam.SetPosition(0, Vector3.up * .35f); beam.SetPosition(1, Vector3.up * (2f + envelope * 3f));
            beam.startWidth = .18f * envelope; beam.endWidth = .045f * envelope;
            Color tint = MeasuredColor(succeeded);
            Color start = Color.Lerp(tint, Color.white, .38f); start.a = envelope;
            tint.a = 0;
            beam.startColor = start; beam.endColor = tint;
        }
    }
}
