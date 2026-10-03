using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Small articulated acting rig, evaluated by the cinematic clock. No physics or gameplay input.</summary>
    public sealed class CinematicActor : MonoBehaviour
    {
        [SerializeField] private Transform torso, head, leftArm, rightArm, leftElbow, rightElbow, leftLeg, rightLeg;
        [SerializeField] private Transform[] eyes;
        private Vector3 torsoRest;

        public void Configure(Transform body, Transform face, Transform armL, Transform armR,
            Transform elbowL, Transform elbowR, Transform legL, Transform legR, Transform[] eyeParts)
        {
            torso = body; head = face; leftArm = armL; rightArm = armR;
            leftElbow = elbowL; rightElbow = elbowR; leftLeg = legL; rightLeg = legR; eyes = eyeParts;
            torsoRest = torso.localPosition;
        }

        private void Awake() { if (torso != null) torsoRest = torso.localPosition; }

        public void Pose(float time, float walk, float reach, float recoil, float lookYaw, bool speaking = false)
        {
            float stride = Mathf.Sin(time * 7.2f) * walk;
            float breath = Mathf.Sin(time * 1.9f) * .009f;
            float gesture = speaking ? Mathf.Sin(time * 3.3f) * 6f : 0f;
            torso.localPosition = torsoRest + new Vector3(0f, breath + Mathf.Abs(stride) * .015f, -recoil * .1f);
            torso.localRotation = Quaternion.Euler(recoil * -9f + reach * 4f, stride * 3f, Mathf.Sin(time * 1.4f) * .6f);
            head.localRotation = Quaternion.Euler(reach * 8f - recoil * 9f + Mathf.Sin(time * 2.1f) * 1.3f,
                lookYaw + Mathf.Sin(time * .9f) * 2f, -recoil * 5f);
            leftArm.localRotation = Quaternion.Euler(stride * 22f - reach * 38f - recoil * 30f, -reach * 8f, -4f - recoil * 14f);
            rightArm.localRotation = Quaternion.Euler(-stride * 22f - reach * 56f - recoil * 40f - gesture,
                reach * 11f, 4f + recoil * 16f);
            leftElbow.localRotation = Quaternion.Euler(-12f - reach * 47f - recoil * 25f, 0f, 0f);
            rightElbow.localRotation = Quaternion.Euler(-10f - reach * 35f - recoil * 34f + gesture * .4f, 0f, 0f);
            leftLeg.localRotation = Quaternion.Euler(-stride * 26f + recoil * 7f, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(stride * 26f - recoil * 7f, 0f, 0f);
            float blink = Mathf.Repeat(time + .73f, 4.2f) < .12f ? .14f : 1f;
            foreach (Transform eye in eyes)
                if (eye != null) { Vector3 scale = eye.localScale; scale.y = .028f * blink; eye.localScale = scale; }
        }
    }
}
