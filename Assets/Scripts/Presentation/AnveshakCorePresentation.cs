using UnityEngine;

namespace NullSignal.Presentation
{
    public sealed class AnveshakCorePresentation : MonoBehaviour
    {
        [SerializeField] private ApparatusMotion motion;
        [SerializeField] private Renderer[] energy;
        [SerializeField] private Transform[] anomalies;
        [SerializeField] private Light[] practicals;
        [SerializeField] private GameObject emergencyStrips;
        private Vector3[] scales;
        private MaterialPropertyBlock properties;
        public void Configure(ApparatusMotion apparatus, Renderer[] emissives, Transform[] fragments, Light[] lights, GameObject emergency)
        { motion = apparatus; energy = emissives; anomalies = fragments; practicals = lights; emergencyStrips = emergency; }
        private void Awake()
        {
            properties = new MaterialPropertyBlock(); scales = new Vector3[anomalies.Length];
            for (int i = 0; i < anomalies.Length; i++) scales[i] = anomalies[i].localScale;
            emergencyStrips.SetActive(false);
        }
        public void Shutdown(float progress)
        {
            float power = 1f - Mathf.SmoothStep(0f, 1f, progress);
            if (motion != null) motion.Power = power;
            foreach (Renderer part in energy)
            {
                Color color = part.sharedMaterial.HasProperty("_BaseColor") ? part.sharedMaterial.GetColor("_BaseColor") : Color.cyan;
                color.a *= power;
                properties.SetColor("_BaseColor", color * Mathf.Lerp(.03f, 1f, power));
                properties.SetColor("_EmissionColor", color * power * 1.6f); part.SetPropertyBlock(properties);
                if (part is LineRenderer line)
                { Color faded = new Color(.4f, .8f, 1f, power * .55f); line.startColor = line.endColor = faded; }
            }
            for (int i = 0; i < anomalies.Length; i++) anomalies[i].localScale = scales[i] * power;
            foreach (Light light in practicals)
            {
                light.color = Color.Lerp(new Color(1f, .38f, .08f), new Color(.42f, .6f, 1f), power);
                light.intensity = Mathf.Lerp(.65f, 2f, power) * (progress > .45f && progress < .7f ? .12f : 1f);
            }
            emergencyStrips.SetActive(progress > .65f);
        }
    }
}
