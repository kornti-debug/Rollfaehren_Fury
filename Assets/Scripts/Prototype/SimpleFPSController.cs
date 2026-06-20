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
        [SerializeField] private bool animateCharacter;

        private static readonly int IsWalkingId = Animator.StringToHash("IsWalking");
        private static readonly int IsIdleId = Animator.StringToHash("IsIdle");
        private static readonly int WalkingStateId = Animator.StringToHash("Base Layer.Armature|WalkCycle");
        private static readonly int IdleStateId = Animator.StringToHash("Base Layer.Armature|Idle");
        private static readonly string WalkingStateName = "Base Layer.Armature|WalkCycle";
        private static readonly string IdleStateName = "Base Layer.Armature|Idle";

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;
        private Animator animator;
        private bool animatorHasIsWalking;
        private bool animatorHasIsIdle;
        private int activeAnimationStateId;
        private bool lastLoggedIsMoving;
        private int lastLoggedTargetStateId;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction attackAction;
        private Vector2 moveInput;
        private Vector2 pendingLookDelta;
        private bool jumpQueued;
        private bool isSprinting;

        public bool InputEnabled { get; private set; } = true;
        public Vector2 MoveInput => moveInput;
        public bool IsGrounded => controller != null && controller.isGrounded;
        public bool IsSprinting => isSprinting;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();

            if (animateCharacter)
            {
                if (animator == null)
                {
                    Debug.LogWarning($"[{name}] Character animation is enabled, but no child Animator was found.", this);
                }
                else
                {
                    ConfigureAnimator();
                    CacheAnimatorParameters();
                    Debug.Log($"[{name}] Character animation is enabled with controller '{animator.runtimeAnimatorController?.name ?? "<none>"}'. " +
                              $"States: idle='{IdleStateName}', walk='{WalkingStateName}'. Parameters: IsWalking={animatorHasIsWalking}, IsIdle={animatorHasIsIdle}.", this);
                }
            }
            else if (animator != null)
            {
                Debug.Log($"[{name}] Character animation is disabled; child Animator '{animator.name}' will stay static.", this);
                animator = null;
            }

            if (cameraRoot == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                cameraRoot = childCamera != null ? childCamera.transform : transform;
            }
        }

        private void OnEnable()
        {
            BindInputActions();
            SubscribeInputActions(true);
            SetInputActionsEnabled(true);

            if (lockCursorOnPlay)
            {
                SetCursorLocked(true);
            }

            PlayAnimationState(IdleStateId, true);
        }

        private void OnDisable()
        {
            SubscribeInputActions(false);
            SetInputActionsEnabled(false);
        }

        private void Update()
        {
            if (!InputEnabled)
            {
                pendingLookDelta = Vector2.zero;
                return;
            }

            HandleLook();
            HandleMovement();
        }

        public void SetInputEnabled(bool isEnabled)
        {
            InputEnabled = isEnabled;
            if (!isEnabled)
            {
                moveInput = Vector2.zero;
                pendingLookDelta = Vector2.zero;
                jumpQueued = false;
                isSprinting = false;
                UpdateAnimator(Vector2.zero, false);
            }

            SetCursorLocked(isEnabled && lockCursorOnPlay);
        }

        public void MoveWithPlatform(
            Vector3 previousPlatformPosition,
            Quaternion previousPlatformRotation,
            Vector3 platformPosition,
            Quaternion platformRotation)
        {
            if (controller == null)
            {
                return;
            }

            Quaternion rotationDelta = platformRotation * Quaternion.Inverse(previousPlatformRotation);
            Vector3 previousOffset = transform.position - previousPlatformPosition;
            Vector3 targetPosition = platformPosition + rotationDelta * previousOffset;
            Vector3 displacement = targetPosition - transform.position;
            controller.Move(displacement);
            transform.rotation = rotationDelta * transform.rotation;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled)
            {
                controller.enabled = false;
            }

            transform.SetPositionAndRotation(position, rotation);
            verticalVelocity = -2f;

            if (wasEnabled)
            {
                controller.enabled = true;
            }

            Physics.SyncTransforms();
        }

        private void BindInputActions()
        {
            moveAction ??= PrototypeInputActions.Find("Player/Move");
            lookAction ??= PrototypeInputActions.Find("Player/Look");
            jumpAction ??= PrototypeInputActions.Find("Player/Jump");
            sprintAction ??= PrototypeInputActions.Find("Player/Sprint");
            attackAction ??= PrototypeInputActions.Find("Player/Attack");
        }

        private void SubscribeInputActions(bool subscribe)
        {
            if (moveAction != null)
            {
                if (subscribe)
                {
                    moveAction.performed += HandleMoveInput;
                    moveAction.canceled += HandleMoveInput;
                }
                else
                {
                    moveAction.performed -= HandleMoveInput;
                    moveAction.canceled -= HandleMoveInput;
                }
            }

            if (lookAction != null)
            {
                if (subscribe)
                {
                    lookAction.performed += HandleLookInput;
                    lookAction.canceled += HandleLookInput;
                }
                else
                {
                    lookAction.performed -= HandleLookInput;
                    lookAction.canceled -= HandleLookInput;
                }
            }

            if (jumpAction != null)
            {
                if (subscribe)
                {
                    jumpAction.performed += HandleJumpInput;
                }
                else
                {
                    jumpAction.performed -= HandleJumpInput;
                }
            }

            if (sprintAction != null)
            {
                if (subscribe)
                {
                    sprintAction.performed += HandleSprintInput;
                    sprintAction.canceled += HandleSprintInput;
                }
                else
                {
                    sprintAction.performed -= HandleSprintInput;
                    sprintAction.canceled -= HandleSprintInput;
                }
            }

            if (attackAction != null)
            {
                if (subscribe)
                {
                    attackAction.performed += HandleAttackInput;
                }
                else
                {
                    attackAction.performed -= HandleAttackInput;
                }
            }
        }

        private void SetInputActionsEnabled(bool enabled)
        {
            PrototypeInputActions.SetEnabled(moveAction, enabled);
            PrototypeInputActions.SetEnabled(lookAction, enabled);
            PrototypeInputActions.SetEnabled(jumpAction, enabled);
            PrototypeInputActions.SetEnabled(sprintAction, enabled);
            PrototypeInputActions.SetEnabled(attackAction, enabled);
        }

        private void HandleMoveInput(InputAction.CallbackContext context)
        {
            moveInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
        }

        private void HandleLookInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                pendingLookDelta = Vector2.zero;
                return;
            }

            pendingLookDelta += context.ReadValue<Vector2>();
        }

        private void HandleJumpInput(InputAction.CallbackContext context)
        {
            if (InputEnabled)
            {
                jumpQueued = true;
            }
        }

        private void HandleSprintInput(InputAction.CallbackContext context)
        {
            isSprinting = !context.canceled && context.ReadValue<float>() > 0.5f;
        }

        private void HandleAttackInput(InputAction.CallbackContext context)
        {
            if (InputEnabled)
            {
                SetCursorLocked(true);
            }
        }

        private void HandleLook()
        {
            if (cameraRoot == null || Cursor.lockState != CursorLockMode.Locked)
            {
                pendingLookDelta = Vector2.zero;
                return;
            }

            Vector2 mouseDelta = pendingLookDelta * mouseSensitivity;
            pendingLookDelta = Vector2.zero;
            transform.Rotate(Vector3.up, mouseDelta.x, Space.World);

            pitch = Mathf.Clamp(pitch - mouseDelta.y, -pitchClamp, pitchClamp);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
            Vector3 movement = (transform.forward * moveInput.y + transform.right * moveInput.x) * speed;

            UpdateAnimator(moveInput, isSprinting);

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (controller.isGrounded && jumpQueued)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            jumpQueued = false;
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
            if (animatorHasIsWalking)
            {
                animator.SetBool(IsWalkingId, isMoving);
            }

            if (animatorHasIsIdle)
            {
                animator.SetBool(IsIdleId, !isMoving);
            }

            int targetStateId = isMoving ? WalkingStateId : IdleStateId;
            if (isMoving != lastLoggedIsMoving || targetStateId != lastLoggedTargetStateId)
            {
                Debug.Log($"[{name}] Animator update: moveInput={moveInput}, sprint={isSprinting}, isMoving={isMoving}, target={DescribeAnimatorState(targetStateId)}, current={DescribeCurrentAnimatorState(0)}.", this);
                lastLoggedIsMoving = isMoving;
                lastLoggedTargetStateId = targetStateId;
            }

            PlayAnimationState(targetStateId, !isMoving);
            animator.speed = isMoving && !isSprinting ? 0.85f : 1f;
        }

        private void ConfigureAnimator()
        {
            activeAnimationStateId = 0;
            lastLoggedIsMoving = false;
            lastLoggedTargetStateId = 0;

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
            animatorHasIsWalking = false;
            animatorHasIsIdle = false;

            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == IsWalkingId && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animatorHasIsWalking = true;
                }
                else if (parameter.nameHash == IsIdleId && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animatorHasIsIdle = true;
                }
            }

            Debug.Log($"[{name}] Animator parameters cached: IsWalking={animatorHasIsWalking}, IsIdle={animatorHasIsIdle}.", this);
        }

        private void PlayAnimationState(int stateId, bool forceRestart)
        {
            if (animator == null)
            {
                return;
            }

            if (!animator.HasState(0, stateId))
            {
                Debug.LogWarning($"[{name}] Animator controller '{animator.runtimeAnimatorController?.name ?? "<none>"}' is missing state '{DescribeAnimatorState(stateId)}'. Current state: {DescribeCurrentAnimatorState(0)}.", this);
                return;
            }

            if (!forceRestart && activeAnimationStateId == stateId)
            {
                return;
            }

            animator.CrossFadeInFixedTime(stateId, 0.1f);
            activeAnimationStateId = stateId;
        }

        private string DescribeAnimatorState(int stateId)
        {
            if (stateId == WalkingStateId)
            {
                return WalkingStateName;
            }

            if (stateId == IdleStateId)
            {
                return IdleStateName;
            }

            return $"hash {stateId}";
        }

        private string DescribeCurrentAnimatorState(int layerIndex)
        {
            if (animator == null)
            {
                return "<no animator>";
            }

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return $"{DescribeAnimatorState(currentState.fullPathHash)} (hash={currentState.fullPathHash}, normalized={currentState.normalizedTime:0.00}, transitioning={animator.IsInTransition(layerIndex)})";
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

    }
}
