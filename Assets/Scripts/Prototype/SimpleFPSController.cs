using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SimpleFPSController : MonoBehaviour
    {
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.35f;
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float pitchClamp = 60f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float jumpHeight = 1.1f;
        [SerializeField] private bool lockCursorOnPlay = true;

        private static readonly int IsRunningId = Animator.StringToHash("IsRunning");
        private static readonly int IsIdleId = Animator.StringToHash("IsIdle");
        private static readonly int RunningStateId = Animator.StringToHash("Base Layer.Running");
        private static readonly int IdleStateId = Animator.StringToHash("Base Layer.Idle");

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;
        private Animator animator;
        private bool animatorHasIsRunning;
        private bool animatorHasIsIdle;
        private int activeAnimationStateId;

        public bool InputEnabled { get; private set; } = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            ConfigureAnimator();
            CacheAnimatorParameters();
            if (cameraRoot == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                cameraRoot = childCamera != null ? childCamera.transform : transform;
            }
        }

        private void OnEnable()
        {
            if (lockCursorOnPlay)
            {
                SetCursorLocked(true);
            }

            PlayAnimationState(IdleStateId, true);
        }

        private void Update()
        {
            HandleCursorToggle();

            if (!InputEnabled)
            {
                return;
            }

            HandleLook();
            HandleMovement();
        }

        public void SetInputEnabled(bool isEnabled)
        {
            InputEnabled = isEnabled;
            SetCursorLocked(isEnabled && lockCursorOnPlay);
        }

        private void HandleCursorToggle()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && InputEnabled)
            {
                SetCursorLocked(true);
            }
        }

        private void HandleLook()
        {
            if (Mouse.current == null || cameraRoot == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(Vector3.up, mouseDelta.x, Space.World);

            pitch = Mathf.Clamp(pitch - mouseDelta.y, -pitchClamp, pitchClamp);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed)
            {
                moveInput.y += 1f;
            }
            if (Keyboard.current.sKey.isPressed)
            {
                moveInput.y -= 1f;
            }
            if (Keyboard.current.dKey.isPressed)
            {
                moveInput.x += 1f;
            }
            if (Keyboard.current.aKey.isPressed)
            {
                moveInput.x -= 1f;
            }

            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
            bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
            float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
            Vector3 movement = (transform.forward * moveInput.y + transform.right * moveInput.x) * speed;

            UpdateAnimator(moveInput, isSprinting);

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (controller.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;
            movement.y = verticalVelocity;

            controller.Move(movement * Time.deltaTime);
        }

        private void UpdateAnimator(Vector2 moveInput, bool isSprinting)
        {
            if (animator == null)
            {
                return;
            }

            bool isMoving = moveInput.sqrMagnitude > 0.001f;
            if (animatorHasIsRunning)
            {
                animator.SetBool(IsRunningId, isMoving);
            }

            if (animatorHasIsIdle)
            {
                animator.SetBool(IsIdleId, !isMoving);
            }

            int targetStateId = isMoving ? RunningStateId : IdleStateId;
            PlayAnimationState(targetStateId, !isMoving);
            animator.speed = isMoving && !isSprinting ? 0.85f : 1f;
        }

        private void ConfigureAnimator()
        {
            activeAnimationStateId = 0;

            if (animator == null)
            {
                return;
            }

            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.Rebind();
            animator.Update(0f);
        }

        private void CacheAnimatorParameters()
        {
            animatorHasIsRunning = false;
            animatorHasIsIdle = false;

            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == IsRunningId && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animatorHasIsRunning = true;
                }
                else if (parameter.nameHash == IsIdleId && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animatorHasIsIdle = true;
                }
            }
        }

        private void PlayAnimationState(int stateId, bool forceRestart)
        {
            if (animator == null)
            {
                return;
            }

            if (!forceRestart && activeAnimationStateId == stateId)
            {
                return;
            }

            animator.CrossFadeInFixedTime(stateId, 0.1f);
            activeAnimationStateId = stateId;
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
