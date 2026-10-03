using UnityEngine;

namespace NullSignal.Presentation
{
    public sealed class HologramPerformance : MonoBehaviour
    {
        [SerializeField] private Transform body, arm, head;
        [SerializeField] private SubtitleController subtitles;
        [SerializeField] private string speaker;
        private Renderer[] surfaces;
        private MaterialPropertyBlock surfaceProperties;
        private Vector3 bodyRest;
        private float gesture, signal = 1f;
        public Transform Body => body;
        public Transform Head => head;
        public Transform Arm => arm;
        public string Speaker => speaker;
        public void Configure(Transform torso, Transform hand, Transform face, SubtitleController dialogue, string name)
        { body = torso; arm = hand; head = face; subtitles = dialogue; speaker = name; }
        private void Awake()
        {
            if (body != null) { bodyRest = body.localPosition; surfaces = body.GetComponentsInChildren<Renderer>(); }
            surfaceProperties = new MaterialPropertyBlock();
        }
        private void OnEnable() { if (surfaceProperties == null) Awake(); }
        private void Update()
        {
            if (body == null || arm == null || head == null) return;
            float t = Time.time; bool speaking = subtitles != null && subtitles.Speaker == speaker;
            gesture = Mathf.MoveTowards(gesture, speaking ? 1f : 0f, Time.deltaTime * 3.4f);
            body.localPosition = bodyRest + Vector3.up * (.035f + Mathf.Sin(t * 1.4f) * .014f);
            body.localRotation = Quaternion.Euler(gesture * -2f, Mathf.Sin(t * .6f) * 4f, Mathf.Sin(t * .75f) * .7f);
            arm.localRotation = Quaternion.Euler(-12f - gesture * (22f + Mathf.Sin(t * 2f) * 10f), gesture * 9f, -12f - gesture * 4f);
            head.localRotation = Quaternion.Euler(Mathf.Sin(t * 1.7f) * Mathf.Lerp(1f, 4f, gesture), Mathf.Sin(t * .7f) * 7f, gesture * -2f);
            // Tiny signal loss keeps the scientist identifiable; it never blanks the recording.
            float dropout = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * .71f + transform.position.x)), 40f);
            signal = Mathf.MoveTowards(signal, 1f - dropout * .12f, Time.deltaTime * 2f);
            if (surfaces == null) return;
            foreach (Renderer surface in surfaces)
            {
                if (surface == null) continue;
                surface.GetPropertyBlock(surfaceProperties);
                surfaceProperties.SetFloat("_SignalStrength", signal);
                surfaceProperties.SetFloat("_Distortion", .008f + dropout * .009f);
                surface.SetPropertyBlock(surfaceProperties);
            }
        }
    }
}
