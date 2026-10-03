using NullSignal.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullSignal.Presentation
{
    [DefaultExecutionOrder(110)]
    public sealed class AnveshOperationPulse : MonoBehaviour
    {
        [SerializeField] private AnveshOperationController operations;
        [SerializeField] private LineRenderer wavefront;
        [SerializeField] private Renderer wrist;
        private MaterialPropertyBlock properties;
        private StationCamera cameraRig;
        private StationFeedbackAudio audioFeedback;
        private double[] started, released;
        private QuantumEncounter lastEncounter;
        private Transform apparatus;
        private LineRenderer innerPulse;
        private AnveshController controller;
        private MaterialPropertyBlock wristRest;
        private bool wristLit;
        public void Configure(AnveshOperationController source, LineRenderer ring, Renderer device) { operations = source; wavefront = ring; wrist = device; }
        private void Awake()
        {
            properties = new MaterialPropertyBlock(); wristRest = new MaterialPropertyBlock();
            if (wrist != null) wrist.GetPropertyBlock(wristRest);
            controller = GetComponent<AnveshController>(); audioFeedback = GetComponentInParent<StationFeedbackAudio>();
        }
        private void Start()
        {
            if (Camera.main != null) cameraRig = Camera.main.GetComponent<StationCamera>();
            if (wavefront == null) return;
            innerPulse = new GameObject("ANVESH / phase and redistribution echo").AddComponent<LineRenderer>();
            innerPulse.transform.SetParent(transform, false); innerPulse.sharedMaterial = wavefront.sharedMaterial;
            innerPulse.useWorldSpace = true; innerPulse.positionCount = 49; innerPulse.startWidth = innerPulse.endWidth = .025f;
            innerPulse.shadowCastingMode = ShadowCastingMode.Off; innerPulse.receiveShadows = false; innerPulse.enabled = false;
        }
        private void OnEnable()
        {
            if (properties == null) Awake();
            if (operations != null) { operations.Changed -= OnOperation; operations.Changed += OnOperation; }
        }
        private void OnDisable()
        {
            if (operations != null) operations.Changed -= OnOperation;
            if (cameraRig != null) cameraRig.QuantumPush(0);
            if (wavefront != null) wavefront.enabled = false;
            if (innerPulse != null) innerPulse.enabled = false;
            if (wrist != null && wristRest != null) wrist.SetPropertyBlock(wristRest);
            wristLit = false;
        }
        private void OnOperation()
        {
            if (operations.Action == AnveshAbility.None) { started = released = null; return; }
            if (!ReferenceEquals(started, operations.Before))
            {
                started = operations.Before;
                if (operations.Action == AnveshAbility.Amplify || operations.Action == AnveshAbility.Lock) audioFeedback?.Charge();
                else if (operations.Action == AnveshAbility.Scan) audioFeedback?.Scan();
                else if (operations.Action == AnveshAbility.Mark) audioFeedback?.Mark();
            }
            if (operations.After == null || ReferenceEquals(released, operations.Before)) return;
            released = operations.Before;
            if (operations.Overshot) { audioFeedback?.Warning(); cameraRig?.Impulse(.035f); }
            else if (operations.Action == AnveshAbility.Amplify || operations.Action == AnveshAbility.Lock)
            {
                if (operations.Action == AnveshAbility.Lock) audioFeedback?.Lock(); else audioFeedback?.Amplify();
                cameraRig?.Impulse(operations.Action == AnveshAbility.Lock ? .075f : .045f);
            }
        }
        private void LateUpdate()
        {
            if (operations == null || wavefront == null) return;
            if (properties == null) Awake();
            wavefront.enabled = operations.Busy;
            if (innerPulse != null) innerPulse.enabled = operations.Busy && operations.Action != AnveshAbility.Scan;
            cameraRig?.QuantumPush(operations.Busy ? Mathf.Sin(operations.Progress * Mathf.PI) * .13f : 0f);
            if (!operations.Busy)
            {
                if (wristLit && wrist != null) { wrist.SetPropertyBlock(wristRest); wristLit = false; }
                return;
            }
            float t = operations.Progress;
            QuantumEncounter encounter = controller != null ? controller.Encounter : null;
            Color color = operations.Action == AnveshAbility.Mark || operations.Overshot ? new Color(.72f, .42f, .95f)
                : operations.Action == AnveshAbility.Amplify ? QuantumPresentationEffects.Redistribution
                : operations.Action == AnveshAbility.Lock && operations.Committed && encounter != null
                    ? QuantumPresentationEffects.MeasuredColor(encounter.MeasurementSucceeded) : QuantumPresentationEffects.Science;
            color.a = Mathf.Sin(t * Mathf.PI) * .65f; wavefront.startColor = wavefront.endColor = color;
            if (encounter != lastEncounter)
            {
                lastEncounter = encounter; apparatus = null;
                if (encounter != null)
                {
                    var motion = encounter.transform.parent.GetComponentInChildren<ApparatusMotion>();
                    if (motion != null) apparatus = motion.transform.parent;
                }
            }
            Vector3 center = transform.position;
            if (encounter != null && operations.Action != AnveshAbility.Scan) center = encounter.transform.parent.position;
            if (apparatus != null && operations.Action == AnveshAbility.Amplify) center = apparatus.position;
            float radius = operations.Action == AnveshAbility.Lock ? Mathf.Lerp(8f, .25f, Mathf.InverseLerp(.18f, .8f, t))
                : operations.Action == AnveshAbility.Amplify ? t < .45f ? Mathf.Lerp(1.3f, .3f, t / .45f) : Mathf.Lerp(.3f, 10f, (t - .45f) / .55f)
                : Mathf.Lerp(.2f, operations.Action == AnveshAbility.Scan ? 19f : 7f, t);
            for (int i = 0; i < wavefront.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / (wavefront.positionCount - 1);
                wavefront.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, .1f, Mathf.Sin(angle) * radius));
            }
            if (innerPulse != null && innerPulse.enabled)
            {
                Color echo = Color.Lerp(color, Color.white, .25f); echo.a = color.a * .38f;
                innerPulse.startColor = innerPulse.endColor = echo;
                for (int i = 0; i < innerPulse.positionCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / (innerPulse.positionCount - 1);
                    float ripple = operations.Action == AnveshAbility.Mark ? Mathf.Sin(angle * 6f - t * 12f) * .08f : 0f;
                    float echoRadius = Mathf.Max(.1f, radius - .22f + ripple);
                    innerPulse.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * echoRadius, .14f, Mathf.Sin(angle) * echoRadius));
                }
            }
            if (wrist != null) { properties.SetColor("_EmissionColor", color * (2f + Mathf.Sin(t * Mathf.PI) * 3f)); wrist.SetPropertyBlock(properties); wristLit = true; }
        }
    }
}
