using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NullSignal.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController), typeof(PlayerDodge))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Min(0f)] private float moveSpeed = 4.5f;
        [SerializeField, Min(0f)] private float acceleration = 28f;
        [SerializeField, Min(0f)] private float deceleration = 35f;
        [SerializeField, Min(0f)] private float turnSpeed = 900f;
        [SerializeField] private float gravity = -24f;

        private CharacterController motor;
        private PlayerDodge dodge;
        private Vector3 planarVelocity;
        private float verticalVelocity;

        public void SetCamera(Transform view) => cameraTransform = view;

        private void Awake()
        {
            motor = GetComponent<CharacterController>();
            dodge = GetComponent<PlayerDodge>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (!motor.enabled)
                return;

            float deltaTime = Time.deltaTime;
            dodge.Tick(deltaTime);
            Vector2 input = ReadMovement();
            Vector3 desiredDirection = CameraRelativeDirection(input);
            if (DodgePressed() && deltaTime > 0f)
                dodge.TryDodge(desiredDirection.sqrMagnitude > 0.001f ? desiredDirection : transform.forward);

            Vector3 desiredVelocity = desiredDirection * moveSpeed;
            float rate = input.sqrMagnitude > 0f ? acceleration : deceleration;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desiredVelocity, rate * deltaTime);
            Vector3 motion = dodge.IsDodging ? dodge.Velocity : planarVelocity;

            if (motor.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            verticalVelocity += gravity * deltaTime;
            CollisionFlags collisions = motor.Move((motion + Vector3.up * verticalVelocity) * deltaTime);
            if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;

            if (motion.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(motion, Vector3.up), turnSpeed * deltaTime);
        }

        private Vector3 CameraRelativeDirection(Vector2 input)
        {
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            return Vector3.ClampMagnitude(right * input.x + forward * input.y, 1f);
        }

        private static Vector2 ReadMovement()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;
            float x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
#else
            return Vector2.zero;
#endif
        }

        private static bool DodgePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                ClearMotion();
        }

        private void OnDisable() => ClearMotion();

        private void ClearMotion()
        {
            planarVelocity = Vector3.zero;
            verticalVelocity = 0f;
            if (dodge != null)
                dodge.Cancel();
        }
    }
}
