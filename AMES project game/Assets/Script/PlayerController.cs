using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif
using UnityEngine.UI;

namespace AmesGame
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float RotationSpeed = 1.0f;

        [Tooltip("How fast the player accelerates")]
        public float AccelerationRate = 80f;

        [Tooltip("How fast the player decelerates")]
        public float DecelerationRate = 15f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.1f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        [Header("Health Settings")]
        public int MaxHealth = 100;
        public int CurrentHealth;

        [Header("UI")]
        public Image healthBar;

        [Header("Damage Settings")]
        public int bulletDamage = 10;

        [Header("Immunity")]
        [SerializeField]
        private bool isImmune = false;

        public bool IsImmune => isImmune;

        public void SetTemporaryImmunity(float seconds)
        {
            if (seconds <= 0f) return;
            StartCoroutine(TemporaryImmunityCoroutine(seconds));
        }

        private IEnumerator TemporaryImmunityCoroutine(float seconds)
        {
            isImmune = true;
            yield return new WaitForSeconds(seconds);
            isImmune = false;
        }

        private float _cinemachineTargetPitch;

        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private Vector3 _horizontalVelocity;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private AmesGameInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<AmesGameInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies.");
#endif

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            CurrentHealth = MaxHealth;
            UpdateHealthUI();
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            float targetSpeed = MoveSpeed;

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // ✅ MOMENTUM-BASED SPEED CHANGE (only modified section)
            float target = targetSpeed * inputMagnitude;
            float rate = (_input.move == Vector2.zero) ? DecelerationRate : AccelerationRate;

            _speed = Mathf.MoveTowards(currentHorizontalSpeed, target, rate * Time.deltaTime);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
            // ✅ END CHANGE

            Vector3 inputDirection = Vector3.zero;

            if (_input.move != Vector2.zero)
            {
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
                inputDirection.Normalize();
            }

            // If there's input → push velocity toward target direction
            if (_input.move != Vector2.zero)
            {
                Vector3 targetVelocity = inputDirection * _speed;

                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    targetVelocity,
                    AccelerationRate * Time.deltaTime
                );
            }
            else
            {
                // No input → gradually slow down (this is your momentum carry)
                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    Vector3.zero,
                    DecelerationRate * Time.deltaTime
                );
            }

            // Move using stored velocity instead of input directly
            _controller.Move(_horizontalVelocity * Time.deltaTime +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0, 1, 0, 0.35f);
            Color transparentRed = new Color(1, 0, 0, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("EnemyBullet"))
            {
                TakeDamage(bulletDamage);
                Destroy(collider.gameObject);
            }
        }

        public void TakeDamage(int damage)
        {
            if (CurrentHealth <= 0 || isImmune) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            UpdateHealthUI();

            if (CurrentHealth == 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth += amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = (float)CurrentHealth / MaxHealth;
            }
        }

        private void Die()
        {
        }
    }
}