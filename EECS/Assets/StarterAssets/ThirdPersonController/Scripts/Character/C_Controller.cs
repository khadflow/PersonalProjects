using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Animations;




#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif

    public class C_Controller : MonoBehaviour
    {
        /* Shared */
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        // REMOVE
        public float MoveSpeed = 2.5f;

        [Tooltip("Sprint speed of the character in m/s")]
        // REMOVE
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        private float JumpHeight = 1.6f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        private float Gravity = -25.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Round management
        public static int Round = 1;
        private static int NumPlayers = 0;
        private static float MaxHealth = 4096;
        private static bool switchCondition = true;
        private static float FirstPlayerDegrees = 90.0f;
        private static float SecondPlayerDegrees = 270.0f;
        private static C_Controller[] players = new C_Controller[2];

        private static float Timer;
        private static float RoundLoadTime = 5.0f;
        private static bool RoundCoolDown = false;

        /* shared animation IDs */
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDCoolDown;
        private int _animIDStepBack;
        private int _animIDMotionSpeed;
        private int _animIDForwardJump;
        private int _animIDBackwardJump;
        private int _animIDHandWeapon;
        private int _animIDCrouch;
        private int _animIDsmo;

        private int _animIDStun;
        private int _animIDWeapon;
        private int _animIDPunch;
        private int _animIDKick;
        private int _animIDJab;
        private int _animIDHit;
        private int _animIDAttackCoolDown;
        private int _animIDHealth;
        private int _animIDBlock;

        /* Shared Weapon Management */
        [SerializeField] GameObject Weapon;
        [SerializeField] GameObject HandWeapon;
        [SerializeField] GameObject WeaponSheath;
        [SerializeField] GameObject WeaponHolder;
        [SerializeField] GameObject HandWeaponHolder;
        [SerializeField] GameObject ExtendedSummation;
        private GameObject WeaponInSheath;
        private GameObject WeaponInHand;


        private bool WeaponEquip = false;
        private bool HandWeaponEquip = false;

        /* Shared Melee Attack */
        private bool KickStarted;
        private bool PunchStarted;
        private bool JabStarted;
        private bool HitStarted;
        private bool AttackMadeContact;
        private float MoveCap = 2.7f;

        /* Player Management */
        public AttackTrie attackTrie;
        public float health;
        private Canvas ph;
        public Canvas p1;
        public Canvas p2;

        private float _degrees;
        public C_Controller opp;
        private int PlayerNumber;
        private string PlayerType;
        private Vector3 P1StartLocation, P2StartLocation;

        /* Shared Cooldown */
        private bool CoolDown;
        private float StunEndTime;
        private float NextJumpTime;
        private float JumpCoolDownTime = 1.3f;
        private float NextAttackTime;
        private float AttackCoolDownTime = 1.4f;
        private float NextHandWeaponTime;
        private float HandWeaponTimeOut = 5.0f;
        private float NextSMOAttackTime;

        /* Jump Management */
        private bool backwardJump;
        private bool forwardJump;
        // Required private variables for the Momentum Lock

        /* EE */
        [SerializeField] GameObject Clk;
        private GameObject ActiveClk;
        private bool ProjectileFired = false;
        //private float SMOCoolDownTime = ???f;

        /* Math */
        [SerializeField] GameObject VectorLeft;
        [SerializeField] GameObject VectorRight;
        private GameObject[] vectors = new GameObject[3];
        private float NextSwordSwing;
        private float VectorDirection = 0.1f;
        private float SMOCoolDownTime = 2.5f;


#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private InputController _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        private static IDictionary<string, KeyCode> CrouchControlScheme;
        private static IDictionary<string, KeyCode> JumpControlScheme;
        private static IDictionary<string, KeyCode> BlockControlScheme;

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

        /* Script Handling */
        private void Awake()
        {
            StunEndTime = Time.time - 1.0f;
            NextAttackTime = Time.time - 1.0f;
            NextJumpTime = Time.time - 1.0f;
            NextHandWeaponTime = Time.time - 1.0f;

            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            
            // TODO - Are these still needed / in use?
            CrouchControlScheme = new Dictionary<string, KeyCode>();
            CrouchControlScheme.Add("KeyboardMouse", KeyCode.S);
            CrouchControlScheme.Add("Gamepad", KeyCode.JoystickButton6);
            CrouchControlScheme.Add("Xbox Controller", KeyCode.JoystickButton6);
            CrouchControlScheme.Add("PS4 Controller", KeyCode.JoystickButton6);

            JumpControlScheme = new Dictionary<string, KeyCode>();
            JumpControlScheme.Add("KeyboardMouse", KeyCode.W);
            JumpControlScheme.Add("Gamepad", KeyCode.JoystickButton14);
            JumpControlScheme.Add("Xbox Controller", KeyCode.JoystickButton14);
            JumpControlScheme.Add("PS4 Controller", KeyCode.JoystickButton14);

            BlockControlScheme = new Dictionary<string, KeyCode>();
            BlockControlScheme.Add("KeyboardMouse", KeyCode.B);
            BlockControlScheme.Add("Gamepad", KeyCode.JoystickButton11);
            BlockControlScheme.Add("Xbox Controller", KeyCode.JoystickButton11);
            BlockControlScheme.Add("PS4 Controller", KeyCode.JoystickButton11);
        }
        private void Start()
        {
            _animator = GetComponent<Animator>();
            attackTrie = new AttackTrie(_animator);
            health = MaxHealth;
            _degrees = (NumPlayers == 0 ? FirstPlayerDegrees : SecondPlayerDegrees);
            transform.rotation = Quaternion.Euler(0.0f, _degrees, 0.0f);

            // Player Opponent Assignment
            WeaponInSheath = Instantiate(Weapon, WeaponSheath.transform);
            OpponentAssignment();

            HealthBarAssignment();

            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();

            _input = GetComponent<InputController>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            float dist = 3.0f;
            P1StartLocation = new Vector3(_mainCamera.transform.position.x - dist, _mainCamera.transform.position.y - 2, _mainCamera.transform.position.z + 20);
            P2StartLocation = new Vector3(_mainCamera.transform.position.x + dist, _mainCamera.transform.position.y - 2, _mainCamera.transform.position.z + 20);
        }
        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);
            GroundedCheck();
            RoundResetCheck();

            if (Time.time < Timer)
            {
                DisableInput();
                opp.DisableInput();
                Move();
            } else
            {
                bool isIdle = _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
                attackTrie.ResetCombo();
                attackTrie.EmptyQueue(isIdle);

                if (!Block())
                {
                    // Next: Cancel movement when Attack is called
                    Punch();
                    Kick();
                    Jab();
                    Hit();

                    // TODO - Add functions for 1 trigger and 2 bumper buttons
                    Aim();
                    Swing();
                    Heavy();

                    bool isCrouchingActive = Crouch();

                    if (!isCrouchingActive)
                    {
                        // C. NORMAL MOVEMENT/JUMP (Run if not Blocking AND not Crouching)
                        JumpAndGravity(); // Character can jump
                        Move();         // Character can walk/run
                    }
                    else
                    {
                        // C. CROUCHING MOVEMENT (Run if Crouching is Active)
                        // 1. Disable Jump: Do NOT call JumpAndGravity()
                        // 2. Allow Movement: Call Move()
                        Move();
                    }
                } else
                {
                    DisableInput();
                }
            }
        }
        private void LateUpdate()
        {
            CameraRotation();

            if (Timer < Time.time)
            {
                _animator.SetBool(_animIDAttackCoolDown,
                                 (Time.time < NextAttackTime)
                                 || (Time.time < NextSwordSwing));
                _animator.SetBool(_animIDStun, false);

                Orientation();

                // Do not do while jumping
                if (NextJumpTime < Time.time)
                {
                    //SummationAttackHandling();
                }

                //VectorAttackTrajectory();

                //ClkAttackTrajectory();

                //CircuitProjectileAttack();
            }
        }


        /* Round Management */
        public void RoundResetCheck()
        {
            if (StunEndTime < Time.time && health <= 0.1f)
            {
                health = MaxHealth;
                opp.health = MaxHealth;
                _animator.SetFloat(_animIDHealth, health);
                opp._animator.SetFloat(opp._animIDHealth, opp.health);

                RoundCoolDown = false;
                SetTimer(RoundLoadTime * 2.0f);

                // Reset Player Health
                GameObject.FindGameObjectWithTag("PlayerOneHealth").GetComponent<PlayerHealth>().SetHealth(MaxHealth);
                GameObject.FindGameObjectWithTag("PlayerTwoHealth").GetComponent<PlayerHealth>().SetHealth(MaxHealth);
            }
        }
        public static void ResetSwitchCondiition()
        {
            switchCondition = true;
        }
        public static void ResetRound()
        {
            Round = 1;
        }
        public static void ResetRoundCoolDown()
        {
            RoundCoolDown = false;
        }

        /* Player Logistics */
        private void OpponentAssignment()
        {
            if (WeaponInSheath.name == "Integral(Clone)")
            {
                PlayerType = "Mathematician";
            }
            else
            {
                PlayerType = "Electrical Engineer";
            }
            players[NumPlayers] = this;
            NumPlayers++;
            SetPlayerNumber(NumPlayers);
            if (NumPlayers == 2)
            {
                players[0].opp = players[1];
                players[1].opp = players[0];
                SetTimer(RoundLoadTime);
            }
        }
        public static int NumberOfPlayers
        {
            get
            {
                return NumPlayers;
            }
            set
            {
                NumPlayers = value;
            }
        }
        public void SetPlayerNumber(int num)
        {
            PlayerNumber = num;
        }
        public float Degrees
        {
            get
            {
                return _degrees;
            }
            set
            {
                _degrees = value;
            }
        }
        public void HealthBarAssignment()
        {
            if (PlayerNumber == 1)
            {
                ph = Instantiate(p1);
            }
            else
            {
                ph = Instantiate(p2);
            }
        }
        public void DestroyHealthBars()
        {
            if (PlayerNumber == 1)
            {
                Destroy(GameObject.FindGameObjectWithTag("PlayerOneHealth"));
            }
            else
            {
                Destroy(GameObject.FindGameObjectWithTag("PlayerTwoHealth"));
            }
        }
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDCoolDown = Animator.StringToHash("CoolDown");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDForwardJump = Animator.StringToHash("forwardJump"); ;
            _animIDBackwardJump = Animator.StringToHash("backwardJump"); ;
            _animIDStepBack = Animator.StringToHash("StepBack");
            _animIDPunch = Animator.StringToHash("Punching");
            _animIDKick = Animator.StringToHash("Kicking");
            _animIDJab = Animator.StringToHash("Jabing");
            _animIDHit = Animator.StringToHash("Hit");
            _animIDAttackCoolDown = Animator.StringToHash("AttackCoolDown");
            _animIDHealth = Animator.StringToHash("Health");
            _animIDStun = Animator.StringToHash("Stun");
            _animIDWeapon = Animator.StringToHash("WeaponEquip");
            _animIDHandWeapon = Animator.StringToHash("HandWeapon");
            _animIDCrouch = Animator.StringToHash("Crouch");
            _animIDsmo = Animator.StringToHash("SMO");
            _animIDBlock = Animator.StringToHash("Block");

            _animator.SetFloat(_animIDHealth, health);
            _animator.SetBool(_animIDStun, false);
        }

        /* Timers */
        private void SetTimer(float time)
        {
            Timer = Time.time + time;
        }
        public float getNextAttackTime()
        {
            return NextAttackTime;
        }

        /* Character Movement */
        private void Move()
        {
            if (Time.time < NextAttackTime || _input.isBlocking)
            {
                _speed = 0.0f;
                _animator.SetFloat(_animIDSpeed, 0.0f);
            }

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // Current facing direction of the player
            float direction = (_degrees == FirstPlayerDegrees ? 1 : -1);

            /* Deal With Input while stunned */

            if (Time.time < StunEndTime)
            {
                DisableInput();
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // TODO - No longer needed?
            /*
            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }
            */
            if (Time.time >= NextAttackTime)
            {
                // a reference to the players current horizontal velocity
                float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                float speedOffset = 0.1f;
                float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

                // accelerate or decelerate to target speed
                if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                    currentHorizontalSpeed > targetSpeed + speedOffset)
                {
                    // creates curved result rather than a linear one giving a more organic speed change
                    // note T in Lerp is clamped, so we don't need to clamp our speed
                    _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                        Time.deltaTime * SpeedChangeRate);

                    // round speed to 3 decimal places
                    _speed = Mathf.Round(_speed * 1000f) / 1000f;
                }
                else
                {
                    _speed = targetSpeed;
                }
            }

            _speed = System.Math.Min(_speed, MoveCap);
            if (Time.time < NextJumpTime - 0.5f)
            {
                _speed *= (0.9f);
            }
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // Fixed Character Facing Position
                transform.rotation = Quaternion.Euler(0.0f, _degrees, 0.0f);
            }

            if (Time.time < StunEndTime)
            {
                _speed = 1.0f;
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            /* Player Directional Jump Input and Animation Trigger */
            KeyCode value;
            JumpControlScheme.TryGetValue(_playerInput.currentControlScheme, out value);

            // Calculate jump direction flags based on input and player facing
            bool jump = Mathf.Abs(_playerInput.actions["Jump"].ReadValue<float>()) > 0.1f;
            bool stepBack = (direction == 1) ? (_input.move.x < 0.0f) : (_input.move.x > 0.0f);
            _animator.SetBool(_animIDStepBack, stepBack);

            forwardJump = !stepBack && jump;
            backwardJump = jump && !forwardJump;

            // Only set jump animation flags if the cooldown is over (NextJumpTime < Time.time)
            if (NextJumpTime < Time.time)
            {
                // Set the animator booleans directly. This is the fix for the flickering animation.
                _animator.SetBool(_animIDBackwardJump, backwardJump);
                _animator.SetBool(_animIDForwardJump, forwardJump);

                // If you need the external movement.Jump() for other logic (like VFX/SFX),
                // keep it, but it was likely causing the conflict. Using direct animator calls is usually better.
                // movement.Jump(backwardJump, forwardJump); 
            }
            else
            {
                // If we are in cooldown, ensure the animation flags are FALSE to stop flicker.
                _animator.SetBool(_animIDBackwardJump, false);
                _animator.SetBool(_animIDForwardJump, false);
            }

            // Apply movement
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator && Timer < Time.time)
            {
                _animator.SetFloat(_animIDSpeed, System.Math.Min(_animationBlend, _speed));
            }
            else if (Time.time < Timer)
            {
                _animator.SetFloat(_animIDSpeed, 0.0f);
            }
        }
        private void JumpAndGravity()
        {
            // The boolean that indicates if a jump-initiating animation is currently running
            bool isJumpingAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("ForwardFlip")
                                     || _animator.GetCurrentAnimatorStateInfo(0).IsName("Backflip");

            // 1. GROUNDED LOGIC: Reset vertical velocity if the custom Grounded variable is true
            if (Grounded) // <<< Check the Grounded boolean set in Update()
            {
                Debug.Log("Grounded");
                // Reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;
                
                // Stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    // Set vertical velocity to a small negative value to ensure consistent grounding force
                    _verticalVelocity = -2f;
                }

                // 2. JUMP INITIATION LOGIC: Check cooldown and trigger jump only when grounded
                if (NextJumpTime < Time.time)
                {
                    Debug.Log("NextJumpTime < Time.time, Grounded becomes false");
                    CoolDown = false;
                    _animator.SetBool(_animIDCoolDown, CoolDown);

                    if (isJumpingAnimation)
                    {
                        NextJumpTime = Time.time + JumpCoolDownTime;
                        Grounded = NextJumpTime < Time.time;
                        _animator.SetBool(_animIDGrounded, Grounded);

                        CoolDown = true;
                        _animator.SetBool(_animIDCoolDown, CoolDown);

                        // Set the initial jump velocity
                        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                        // Update jump timeout delta
                        if (_jumpTimeoutDelta >= 0.0f)
                        {
                            _jumpTimeoutDelta -= Time.deltaTime;
                        }
                    }
                }
                else if (Time.time < NextJumpTime && isJumpingAnimation)
                {
                    CoolDown = false;
                    _animator.SetBool(_animIDCoolDown, CoolDown);
                }
            }
            // 3. AIRBORNE LOGIC: Apply gravity only when NOT Grounded
            else
            {
                // Apply gravity over time if not at terminal velocity
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += Gravity * Time.deltaTime;
                }
            }

            // 4. Input Flag Resets (Run regardless of grounded state)
            _input.jump = false;
            forwardJump = false;
            backwardJump = false;
            _animator.SetBool(_animIDBackwardJump, false);
            _animator.SetBool(_animIDForwardJump, false);
        }
        private void DisableInput()
        {
            // If the player is currently in the BLOCKING state, clear all action inputs
            if (_input.isBlocking)
            {
                // 1. Clear Jump and Jump Animations
                _input.jump = false;
                _animator.SetBool(_animIDForwardJump, false);
                _animator.SetBool(_animIDBackwardJump, false);

                // 2. Clear all Attack/Action Inputs and their corresponding Animator Booleans
                _input.punch = false;
                _animator.SetBool(_animIDPunch, false);
                _input.kick = false;
                _animator.SetBool(_animIDKick, false);
                _input.jab = false;
                _animator.SetBool(_animIDJab, false);
                _input.hit = false;
                _animator.SetBool(_animIDHit, false);
                _input.smo = false;
                _animator.SetBool(_animIDsmo, false);
                _input.crouch = false;
                _animator.SetBool(_animIDCrouch, false);

                // Preserve _input.move (horizontal stick/key input) so that when blocking ends, 
                // the Move() function can immediately see the input and trigger the Walk/Step animation.

                // NOTE: The lines below for _input.equipWeapon and _speed/animator speed are 
                // unnecessary here if you handle speed in Move() and equipWeapon elsewhere. 
                // I've kept the other animation resets clean.
                // _input.equipWeapon = WeaponEquip; 
            }
            else
            {
                // This 'else' block assumes the function is being used for other full lockouts
                // like StunEndTime, where the character should stop moving entirely.
                _input.move = Vector2.zero;
            }
        }
        private void Orientation()
        {
            bool oldSwitchCondition = switchCondition;
            if (opp != null && PlayerNumber == 1 && opp.transform.position.x < transform.position.x && switchCondition)
            {
                switchCondition = false; // Characters on the wrong side of each other
            }
            else if (PlayerNumber == 1 && opp != null && opp.transform.position.x > transform.position.x && !switchCondition)
            {
                switchCondition = true; // Characters on starting side of each other
            }

            if (opp != null && oldSwitchCondition != switchCondition)
            {
                _degrees = (_degrees == FirstPlayerDegrees ? SecondPlayerDegrees : FirstPlayerDegrees);
                opp._degrees = (opp._degrees == FirstPlayerDegrees ? SecondPlayerDegrees : FirstPlayerDegrees);
            }
        }

        /* Attacks and Character Movements */
        private bool Crouch()
        {
            // 1. HIGH PRIORITY CHECK: BLOCK LOCKOUT
            // If the player is currently blocking, we must immediately prevent crouching.
            if (_input.isBlocking)
            {
                // Ensure the internal crouch state and animator are turned OFF
                _input.crouch = false;
                _animator.SetBool(_animIDCrouch, false);
                MoveCap = 2.7f; // Reset to standard MoveCap
                return false;
            }

            // 2. READ INPUT
            // Checks if the Crouch button/stick is actively being held
            bool keyDown = Mathf.Abs(_playerInput.actions["Crouch"].ReadValue<float>()) > 0.1f;

            // 3. SET STATE
            _input.crouch = keyDown;
            _animator.SetBool(_animIDCrouch, _input.crouch);

            // 4. APPLY MOVEMENT MODIFIER
            // This is now safe because the Block check at the top handles the priority override.
            MoveCap = _input.crouch ? 1.5f : 2.7f;

            // Note: You must also ensure your attack functions prevent attacking while _input.crouch is true.

            return _input.crouch;
        }
        private bool Block()
        {
            bool keyDown = (System.Math.Floor(_playerInput.actions["Blocking"].ReadValue<float>()) >= 0.5f);
            _input.isBlocking = keyDown;
            _animator.SetBool(_animIDBlock, _input.isBlocking);
            return _input.isBlocking;
        }
        // TODO - Add Colliders and Take damage so that the fists/legs of
        // players colliding with the opponent is what causes damage and
        // not general distance
        private void Kick()
        {
            if (_input.kick)
            {
                Debug.Log("I pushed Kick");
                attackTrie.Play("Kick");
                _input.kick = false;
                NextAttackTime = Time.time + AttackCoolDownTime;
            }
        }
        private void Punch()
        {
            if (_input.punch)
            {
                Debug.Log("I pushed Punch");
                attackTrie.Play("Punch");
                _input.punch = false;
                NextAttackTime = Time.time + AttackCoolDownTime;
            }
        }
        private void Jab()
        {
            if (_input.jab)
            {
                Debug.Log("I pushed Jab");
                attackTrie.Play("Jab");
                _input.jab = false;
                NextAttackTime = Time.time + AttackCoolDownTime;
            }
        }
        private void Hit()
        {
            if (_input.hit)
            {
                Debug.Log("I pushed Hit");
                attackTrie.Play("Hit");
                _input.hit = false;
                NextAttackTime = Time.time + AttackCoolDownTime;
            }
        }
        private void Aim()
        {
            if (_input.equipHandWeapon)
            {
                Debug.Log("I pushed Aim");
                attackTrie.Play("Aim");
                _input.equipHandWeapon = false;
                NextAttackTime = Time.time + AttackCoolDownTime;
            }
        }
        private void Swing()
        {
            if (_input.equipWeapon)
            {
                Debug.Log("I pushed 360 Swing");
                attackTrie.Play("360 Swing");
                _input.equipWeapon = false;
                NextAttackTime = Time.time + AttackCoolDownTime;
            }
        }
        private void Heavy()
        {
            if (_input.smo)
            {
                Debug.Log("I pushed Heavy");
                attackTrie.Play("Heavy");
                _input.smo = false;
                NextAttackTime = Time.time + AttackCoolDownTime;
            }
        }

        /* Damage Handling */
        public void TakeDamage(float damage, bool smo)
        {
            float oppX = opp.transform.position.x;
            float X = transform.position.x;
            if (System.Math.Abs(oppX - X) <= 1.5f || smo)
            {
                // The player can't take damage while already stunned
                if (StunEndTime < Time.time
                    && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Stun")
                    && !_input.crouch
                    && !_input.isBlocking)
                {
                    health -= damage;
                    _animator.SetFloat(_animIDHealth, health);
                    _animator.SetBool(_animIDStun, true);
                    StunEndTime = Time.time + AttackCoolDownTime + 0.1f;
                }
            }

            if (health < 0.1f && !RoundCoolDown)
            {
                Round++;
                RoundCoolDown = true;
            }
            if (PlayerNumber == 2)
            {
                GameObject.FindGameObjectWithTag("PlayerTwoHealth").GetComponent<PlayerHealth>().SetHealth(health);
            }
            else
            {
                GameObject.FindGameObjectWithTag("PlayerOneHealth").GetComponent<PlayerHealth>().SetHealth(health);
            }

        }
        public float getHealth()
        {
            return health;
        }
        public void SetHealth()
        {
            health = MaxHealth;
        }
        public static float GetMaxHealth()
        {
            return MaxHealth;
        }

        /* Starter Asset Functions - Start */
        private void GroundedCheck()
        {
            // Get the exact bottom center position of the CharacterController capsule.
            // This is more reliable than manually tuning GroundedOffset.
            Vector3 bottomCenter = _controller.transform.position + _controller.center - new Vector3(0, _controller.height / 2, 0);

            // Apply a small downward offset to ensure the sphere extends beyond the capsule base.
            Vector3 spherePosition = bottomCenter + Vector3.down * 0.05f;

            // We can also use the controller's radius for the sphere size.
            float sphereRadius = _controller.radius * 0.9f;

            // Perform the check
            Grounded = Physics.CheckSphere(spherePosition,
                                           sphereRadius,
                                           GroundLayers,
                                           QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
            Grounded = NextJumpTime < Time.time;
            _animator.SetBool(_animIDGrounded, Grounded);
        }
        // TODO - Where do we need this?
        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            /*if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
            */
        }
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }
        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        // TODO - Delete or reintroduce these items
        /* Starter Asset Functions - End */

        /* Weapon Attacks */
        /*private void EquipHandWeapon()
        {
            if (_input.equipHandWeapon
                && NextHandWeaponTime < Time.time
                && NextAttackTime < Time.time
                && !HandWeaponEquip
                && ActiveClk == null)
            {
                WeaponInHand = Instantiate(HandWeapon, HandWeaponHolder.transform);
                WeaponInHand.tag = (PlayerNumber == 1) ? "Player" : "Player2";

                NextAttackTime = Time.time + AttackCoolDownTime;
                NextHandWeaponTime = Time.time + HandWeaponTimeOut;

                _input.equipHandWeapon = false;
                HandWeaponEquip = true;
                _animator.SetBool(_animIDHandWeapon, true);
            }
            else if (Time.time < NextHandWeaponTime - 4.0f)
            {
                _input.move = Vector2.zero;
            }
            // Default to punch while the hand weapon is cooling down
            else if (_input.equipHandWeapon && NextAttackTime < Time.time && NextHandWeaponTime - 4.0f <= Time.time && Time.time < NextHandWeaponTime)
            {
                _input.punch = true;
                _input.kick = false;
                _input.jab = false;
                _input.hit = false;
            }

        }
        private void SummationAttackHandling()
        {
            if (ExtendedSummation != null)
            {
                if ((HandWeaponEquip && NextHandWeaponTime - 3.0f < Time.time))
                {
                    // Return a Extended Summation to original model
                    Destroy(WeaponInHand);
                    WeaponInHand = Instantiate(HandWeapon, HandWeaponHolder.transform);
                    WeaponInHand.tag = (PlayerNumber == 1) ? "Player" : "Player2";
                    _animator.SetBool(_animIDHandWeapon, false);
                }

                // Get rid of hand weapon after attack
                if ((HandWeaponEquip && NextHandWeaponTime - 2.0f < Time.time))
                {
                    HandWeaponEquip = false;
                    Destroy(WeaponInHand);
                }
            }
        }
        private void EquipWeapon()
        {
            bool weaponAttackAnimationActive = _animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialMoveOne") ||
                                _animator.GetCurrentAnimatorStateInfo(0).IsName("Aiming Hand Weapon") ||
                                _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttack1") ||
                                _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttack2") ||
                                _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttack3") ||
                                _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttackHW") ||
                                _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttackH");
            if (_input.equipWeapon != WeaponEquip && _input.equipWeapon && !HandWeaponEquip && NextSwordSwing < Time.time) // Pull out weapon 
            {
                WeaponEquip = true;
                Destroy(WeaponInSheath);
                WeaponInHand = Instantiate(Weapon, WeaponHolder.transform);
                _animator.SetBool(_animIDWeapon, WeaponEquip);
                WeaponInHand.tag = (PlayerNumber == 1) ? "Player" : "Player2";
            }
            else if (!weaponAttackAnimationActive && _input.equipWeapon != WeaponEquip && !_input.equipWeapon && NextSwordSwing < Time.time) // Put weapon away
            {
                WeaponEquip = false;
                Destroy(WeaponInHand);
                WeaponInSheath = Instantiate(Weapon, WeaponSheath.transform);
                _animator.SetBool(_animIDWeapon, WeaponEquip);
            } else
            {
                _input.equipWeapon = WeaponEquip;
            }
        }
        private void WeaponP()
        {
            if (NextSwordSwing < Time.time && _input.punch)
            {
                
                if (PlayerType == "Mathematician")
                {
                    _animator.SetBool(_animIDPunch, _input.punch);
                    NextSwordSwing = Time.time + AttackCoolDownTime + 0.5f;
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                }
                else if (PlayerType == "Electrical Engineer")
                {
                    _animator.SetBool(_animIDPunch, _input.punch);
                    ProjectileFired = false;
                }
                _input.punch = false;
            } else if (NextSwordSwing < Time.time && PlayerType == "Mathematician")
            {
                WeaponInHand.GetComponent<WeaponDamageHandler>().StopAttack();
            }
            else 
            {
                _animator.SetBool(_animIDPunch, false);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Swing"))
            {
                _input.move = Vector2.zero; // no moving while the animation is active
            }
        }
        private void WeaponK()
        {
            if (NextSwordSwing < Time.time && _input.kick)
            {

                if (PlayerType == "Mathematician") {
                    NextSwordSwing = Time.time + AttackCoolDownTime + 0.5f;
                    _animator.SetBool(_animIDKick, _input.kick);
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                } else if (PlayerType == "Electrical Engineer")
                {
                    _animator.SetBool(_animIDKick, _input.kick);
                    ProjectileFired = false;
                }
                _input.kick = false;
            }
            else if (NextSwordSwing < Time.time && PlayerType == "Mathematician")
            {
                WeaponInHand.GetComponent<WeaponDamageHandler>().StopAttack();
            } else
            {
                _animator.SetBool(_animIDKick, false);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Jab"))
            {
                _input.move = Vector2.zero; // no moving while the animation is active
            }
        }
        private void WeaponJ()
        {
            if (NextSwordSwing < Time.time && _input.jab)
            {

                if (PlayerType == "Mathematician")
                {
                    NextSwordSwing = Time.time + AttackCoolDownTime + 0.5f;
                    _animator.SetBool(_animIDJab, _input.jab);
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                }
                else if (PlayerType == "Electrical Engineer")
                {

                    _animator.SetBool(_animIDJab, _input.jab);
                    ProjectileFired = false;
                }
                _input.jab = false;
            }
            else if (NextSwordSwing < Time.time && PlayerType == "Mathematician")
            {
                WeaponInHand.GetComponent<WeaponDamageHandler>().StopAttack();
            }
            else
            {
                _animator.SetBool(_animIDJab, false);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Jab"))
            {
                _input.move = Vector2.zero; // no moving while the animation is active
            }
        }
        private void WeaponH()
        {
            if (NextSwordSwing < Time.time && _input.hit)
            {

                if (PlayerType == "Mathematician")
                {
                    _animator.SetBool(_animIDHit, _input.hit);
                    NextSwordSwing = Time.time + AttackCoolDownTime + 0.5f;
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                }
                else if (PlayerType == "Electrical Engineer")
                {
                    _animator.SetBool(_animIDHit, _input.hit);
                    ProjectileFired = false;
                }
                _input.hit = false;
            }
            else if (NextSwordSwing < Time.time && PlayerType == "Mathematician")
            {
                WeaponInHand.GetComponent<WeaponDamageHandler>().StopAttack();
            }
            else
            {
                _animator.SetBool(_animIDHit, false);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Swing"))
            {
                _input.move = Vector2.zero; // no moving while the animation is active
            }
        }
        private void WeaponSMO()
        {
            if (NextSwordSwing < Time.time && _input.smo)
            {

                if (PlayerType == "Mathematician")
                {
                    _animator.SetBool(_animIDsmo, _input.smo);
                    NextSwordSwing = Time.time + AttackCoolDownTime + 0.5f;
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                }
                else if (PlayerType == "Electrical Engineer")
                {
                    _animator.SetBool(_animIDsmo, _input.smo);
                    ProjectileFired = false;
                }
                _input.punch = false;
            }
            else if (NextSwordSwing < Time.time && PlayerType == "Mathematician")
            {
                WeaponInHand.GetComponent<WeaponDamageHandler>().StopAttack();
            }
            else
            {
                _animator.SetBool(_animIDsmo, false);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Swing"))
            {
                _input.move = Vector2.zero; // no moving while the animation is active
            }
        }
        private void WeaponHW()
        {
            if (NextSwordSwing < Time.time && _input.equipHandWeapon)
            {

                if (PlayerType == "Mathematician")
                {
                    _animator.SetBool(_animIDHandWeapon, _input.equipHandWeapon);
                    NextSwordSwing = Time.time + AttackCoolDownTime + 0.5f;
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                }
                else if (PlayerType == "Electrical Engineer")
                {
                    _animator.SetBool(_animIDHandWeapon, _input.equipHandWeapon);
                    ProjectileFired = false;
                }
                _input.punch = false;
            }
            else if (NextSwordSwing < Time.time && PlayerType == "Mathematician")
            {
                WeaponInHand.GetComponent<WeaponDamageHandler>().StopAttack();
            }
            else
            {
                _animator.SetBool(_animIDHandWeapon, false);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Swing"))
            {
                _input.move = Vector2.zero; // no moving while the animation is active
            }
        }*/

        /* Special Attacks */
        /*private void ClkAttack()
        {
            if (NextAttackTime < Time.time)
            {
                if (_input.smo & NextHandWeaponTime < Time.time)
                {
                    NextAttackTime = Time.time + AttackCoolDownTime;
                    _animator.SetBool(_animIDsmo, true);
                    HandWeaponEquip = false;
                }
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialMoveOne") && ActiveClk == null)
            {
                ActiveClk = Instantiate(Clk, new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z), Quaternion.Euler(0.0f, -_degrees, 0.0f));

                ActiveClk.GetComponent<ClkCollider>().setDegrees(_degrees);
                ActiveClk.tag = (PlayerNumber == 1) ? "Player" : "Player2";

                _input.move = Vector2.zero;
                _speed = 0.0f;
                _animator.SetFloat(_animIDSpeed, 0.0f);

                ActiveClk.GetComponent<WeaponDamageHandler>().Attack();
            }
        }
        private void ClkAttackTrajectory()
        {
            if (NextAttackTime < Time.time && ActiveClk != null)
            {
                Destroy(ActiveClk);
            }
            if (NextAttackTime - 1.0f < Time.time && _input.smo == true)
            {
                _input.smo = false;
                _animator.SetBool(_animIDsmo, false);
            }
        }
        private void CircuitProjectileAttack()
        {
            if (PlayerType == "Electrical Engineer" && WeaponEquip) {
                // EE - Weapon Projectile
                bool activeOneAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttack1");
                bool activeTwoAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttack2");
                bool activeThreeAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttack3");
                bool activeHWAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttackHW");
                bool activeSMOAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialMoveOne");
                bool activeHAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponAttackH");
                if ((activeOneAnimation || activeTwoAnimation || activeThreeAnimation
                     || activeHWAnimation || activeSMOAnimation || activeHAnimation)
                     && !ProjectileFired && NextSwordSwing < Time.time)
                { 
                    ProjectileFired = true;
                    NextSwordSwing = Time.time + AttackCoolDownTime;
                    WeaponInHand.GetComponent<WeaponDamageProjectiles>().switchCondition = switchCondition;
                    WeaponInHand.GetComponent<WeaponDamageProjectiles>().SetTag((PlayerNumber == 1) ? "Player" : "Player2");
                    WeaponInHand.GetComponent<WeaponDamageProjectiles>().Attack();
                }
            }
        }
        private void VectorAttack()
        {
            if (NextSMOAttackTime < Time.time)
            {
                if (_input.smo && NextHandWeaponTime < Time.time)
                {
                    NextSMOAttackTime = Time.time + SMOCoolDownTime;
                    _animator.SetBool(_animIDsmo, true);
                    HandWeaponEquip = false;
                }
            } else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialMoveOne") && vectors[0] == null)
            {
                // Instantiate Vector depending on Player Direction

                float angleX = 0.0f;
                float angleY = 0.0f;
                float angleZ = 0.0f;
                float angle = 0.0f;

                float x, y, z;
                float offsetY, offsetX;

                angleX = 0.0f;
                angleY = 0.0f;
                angleZ = 20.0f;
                angle = 0.0f;
                x = 1.0f;
                y = -4.0f;
                z = 0.0f;
                offsetY = 0.0f;
                offsetX = 0.5f;
                _input.move = Vector2.zero;
                _speed = 0.0f;
                _animator.SetFloat(_animIDSpeed, 0.0f);
               
                if (_degrees == FirstPlayerDegrees)
                {
                    vectors[0] = Instantiate(VectorLeft, new Vector3(transform.position.x + x, y, z), Quaternion.Euler(angleX, angleY, angleZ));
                    vectors[0].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[1] = Instantiate(VectorLeft, new Vector3(transform.position.x + x + offsetX, y - offsetY, z), Quaternion.Euler(angleX, angleY, angleZ));
                    vectors[1].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[2] = Instantiate(VectorLeft, new Vector3(transform.position.x + x + 2.0f * offsetX, y - 2.0f * offsetY, z), Quaternion.Euler(angleX, angleY, angleZ));
                    vectors[2].GetComponent<WeaponDamageHandler>().Attack();

                    VectorDirection = 0.3f;
                }
                else
                {
                    vectors[0] = Instantiate(VectorRight, new Vector3(transform.position.x - 1.0f, y, z), Quaternion.Euler(angleX, angleY - angle, angleZ));
                    vectors[0].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[1] = Instantiate(VectorRight, new Vector3(transform.position.x - 1.0f - offsetX, y - offsetY, z), Quaternion.Euler(angleX, angleY - angle, angleZ));
                    vectors[1].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[2] = Instantiate(VectorRight, new Vector3(transform.position.x - 1.0f - 2.0f * offsetX, y - 2.0f * offsetY, z), Quaternion.Euler(angleX, angleY - angle, angleZ));
                    vectors[2].GetComponent<WeaponDamageHandler>().Attack();

                    VectorDirection = -0.2f;
                }
            }

            if (NextSMOAttackTime < Time.time)
            {
                _input.smo = false;
                _animator.SetBool(_animIDsmo, false);
            }
        }
        private void VectorAttackTrajectory()
        {
            if (NextSMOAttackTime - 0.5f < Time.time && vectors[0] != null)
            {
                Destroy(vectors[0]);
                Destroy(vectors[1]);
                Destroy(vectors[2]);
            }
            // Angled Vectors
            else if (Time.time < NextSMOAttackTime
                    && vectors[0] != null
                    && (vectors[0].transform.rotation.z > 0.18f || vectors[0].transform.rotation.z < 0.17f))
            {
                float angle = 0.25f;
                vectors[0].transform.position = new Vector3(vectors[0].transform.position.x + VectorDirection, vectors[0].transform.position.y + angle, vectors[0].transform.position.z);
                vectors[1].transform.position = new Vector3(vectors[1].transform.position.x + VectorDirection, vectors[1].transform.position.y + angle, vectors[1].transform.position.z);
                vectors[2].transform.position = new Vector3(vectors[2].transform.position.x + VectorDirection, vectors[2].transform.position.y + angle, vectors[2].transform.position.z);
            }
            // Normal Vectors
            else if (Time.time < NextSMOAttackTime
                    && vectors[0] != null
                    && (vectors[0].transform.rotation.z < 0.18f && vectors[0].transform.rotation.z > 0.17f))
            {
                float upward = 0.8f;
                vectors[0].transform.position = new Vector3(vectors[0].transform.position.x, vectors[0].transform.position.y + upward, vectors[0].transform.position.z);
                vectors[1].transform.position = new Vector3(vectors[1].transform.position.x, vectors[1].transform.position.y + upward, vectors[1].transform.position.z);
                vectors[2].transform.position = new Vector3(vectors[2].transform.position.x, vectors[2].transform.position.y + upward, vectors[2].transform.position.z);
            }
        }*/
    }
}