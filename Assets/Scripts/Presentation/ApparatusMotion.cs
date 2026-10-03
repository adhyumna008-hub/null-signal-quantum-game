using UnityEngine;
using NullSignal.Gameplay;

namespace NullSignal.Presentation
{
    public sealed class ApparatusMotion : MonoBehaviour
    {
        [SerializeField] private Transform[] rings;
        private QuantumEncounter encounter;
        private Vector3[] restScales;
        public float Power { get; set; } = 1f;
        private void Awake()
        {
            if (transform.parent != null && transform.parent.parent != null)
                encounter = transform.parent.parent.GetComponentInChildren<QuantumEncounter>();
            restScales = new Vector3[rings.Length];
            for (int i = 0; i < rings.Length; i++) restScales[i] = rings[i].localScale;
        }
        public void Configure(Transform[] orbitalRings) => rings = orbitalRings;
        private void Update()
        {
            var operation = encounter != null ? encounter.PresentationOperations : null;
            float build = operation != null && operation.Busy && operation.Action == AnveshAbility.Amplify
                ? Mathf.Sin(operation.Progress * Mathf.PI) : 0f;
            for (int i = 0; i < rings.Length; i++)
                if (rings[i] != null)
                {
                    rings[i].Rotate(Vector3.up, (i % 2 == 0 ? 9f : -12f) * (1f + build * 5f) * Power * Time.deltaTime, Space.Self);
                    rings[i].localScale = restScales[i] * (1f - build * .12f) * Mathf.Lerp(.25f, 1f, Power);
                }
        }
    }
}
