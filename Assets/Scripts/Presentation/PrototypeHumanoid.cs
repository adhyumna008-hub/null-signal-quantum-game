using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Lightweight stride for the procedural placeholder; movement remains owned by PlayerController.</summary>
    public sealed class PrototypeHumanoid : MonoBehaviour
    {
        [SerializeField] private Transform body, leftArm, rightArm, leftLeg, rightLeg;
        private Vector3 previousPosition;
        private float gait, speed;
        private NullSignal.Player.PlayerCombat combat;
        private NullSignal.Gameplay.AnveshOperationController operations;
        private void Awake() { combat = GetComponent<NullSignal.Player.PlayerCombat>(); operations = GetComponent<NullSignal.Gameplay.AnveshOperationController>(); }
        public void Configure(Transform torso, Transform armL, Transform armR, Transform legL, Transform legR)
        { body = torso; leftArm = armL; rightArm = armR; leftLeg = legL; rightLeg = legR; }
        private void OnEnable() => previousPosition = transform.position;
        private void LateUpdate()
        {
            if (body == null) return;
            Vector3 delta = transform.position - previousPosition; delta.y = 0f;
            previousPosition = transform.position;
            float target = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            speed = Mathf.MoveTowards(speed, target, Time.deltaTime * 24f);
            gait += Time.deltaTime * Mathf.Min(speed, 6f) * 2.8f;
            float step = Mathf.Sin(gait) * Mathf.Clamp01(speed / 4f);
            leftLeg.localRotation = Quaternion.Euler(step * 27f, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(-step * 27f, 0f, 0f);
            leftArm.localRotation = Quaternion.Euler(-step * 22f - 9f, 0f, -5f);
            rightArm.localRotation = Quaternion.Euler(step * 22f, 0f, 5f);
            if (combat != null && combat.AttackPose > 0)
                rightArm.localRotation = Quaternion.Euler(-80f * combat.AttackPose, 0f, 5f);
            if (operations != null && operations.Busy)
                leftArm.localRotation = Quaternion.Euler(-62f - Mathf.Sin(operations.Progress * Mathf.PI) * 18f, 20f, -15f);
            body.localPosition = new Vector3(0f, Mathf.Abs(step) * 0.035f, 0f);
            body.localRotation = Quaternion.Euler(combat != null ? -combat.DamagePose * 9f : 0f, 0, 0);
        }
    }
}
