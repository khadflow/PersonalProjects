using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.Rendering;


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
        /* Starter Asset Variables - Start */
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.5f;

        [Tooltip("Sprint speed of the character in m/s")]
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
        public float JumpHeight = 2.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

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

        /* Starter Asset Variables - End */


        /* Player & Round Management */
        public C_Controller opp;
        private static int NumPlayers = 0;
        private static float FirstPlayerDegrees = 90.0f;
        private static float SecondPlayerDegrees = 270.0f;
        public static int Round = 1;
        private static float MaxHealth = 4096;
        private static C_Controller[] players = new C_Controller[2];

        private float health;
        private Vector3 P1StartLocation, P2StartLocation;

        private static bool switchCondition = true; // Players are in the correct starting position
        private int PlayerNumber;
        private string PlayerType; // "Mathematician" "Electrical Engineer"
        private float _degrees;

        /* Jump Management */
        private bool backwardJump;
        private bool forwardJump;
        private bool jump;

        /* Cooldown Counters */
        private float NextJumpTime;
        private float NextAttackTime;
        private float StunEndTime;
        private float JumpCoolDownTime = 1.2f;
        private float AttackCoolDownTime = 1.5f;
        private float NextSwordSwing;
        private bool CoolDown;
        private float NextHandWeaponTime;
        private float HandWeaponTimeOut = 5.0f;
        private static bool RoundCoolDown = false;
        private static float Timer;
        private static float RoundLoadTime = 5.0f; // Adjust while debugging to play the game upon start


        /* animation IDs */
        private int _animIDSpeed;
        private int _animIDJumping;
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
        private int _animIDAttackCoolDown;
        private int _animIDHealth;

        /* Weapon Management */
        [SerializeField] GameObject WeaponHolder;
        [SerializeField] GameObject HandWeaponHolder;
        [SerializeField] GameObject Weapon;
        [SerializeField] GameObject HandWeapon;
        [SerializeField] GameObject WeaponSheath;

        private bool WeaponEquip = false;
        private bool HandWeaponEquip = false;
        private bool ProjectileFired = false;

        private GameObject[] vectors = new GameObject[3]; // Vector Dot and Cross Product
        private GameObject ActiveClk; // Clk
        private float VectorDirection = 0.1f;

        private GameObject WeaponInSheath;
        private GameObject WeaponInHand;

        /* Hand Weapon Extended Features */
        [SerializeField] GameObject ExtendedSummation; // Summation Extension

        /* Special Move - One */
        [SerializeField] GameObject VectorLeft;
        [SerializeField] GameObject VectorRight;
        [SerializeField] GameObject Clk;

        /* Melee Attack Management */
        private bool ComboStarted;
        private int KickPunch = 2; // 0 == Punch, 1 == Kick, 2 == unoccupied
        private int CurrentComboCount = 0;
        private int MaxComboCount = 3;
        private float AcceptedComboInputTime;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        /* Particle Management */
        [SerializeField] ParticleSystem _punchParticleSystem;
        [SerializeField] ParticleSystem _kickParticleSystem;
        private ParticleSystem ActiveParticles = null;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

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
        }

        private void Start()
        {
            health = MaxHealth;
            _degrees = (NumPlayers == 0 ? FirstPlayerDegrees : SecondPlayerDegrees);
            transform.rotation = Quaternion.Euler(0.0f, _degrees, 0.0f);
            players[NumPlayers] = this;
            NumPlayers++;
            PlayerNumber = NumPlayers;

            // Player Opponent Assignment
            if (NumPlayers == 2)
            {
                players[0].opp = players[1];
                players[1].opp = players[0];
                SetTimer(RoundLoadTime);
            }
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();

            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // Weapon Management
            WeaponInSheath = Instantiate(Weapon, WeaponSheath.transform);

            float dist = 3.0f;
            P1StartLocation = new Vector3(_mainCamera.transform.position.x - dist, _mainCamera.transform.position.y - 2, _mainCamera.transform.position.z + 20);
            P2StartLocation = new Vector3(_mainCamera.transform.position.x + dist, _mainCamera.transform.position.y - 2, _mainCamera.transform.position.z + 20);
        }

        private void Update()
        {
            // Set Player Type
            if (Time.time < Timer) {
                if (WeaponInSheath.name == "Integral(Clone)")
                {
                    PlayerType = "Mathematician";
                }
                else
                {
                    PlayerType = "Electrical Engineer";
                }
            }
            _hasAnimator = TryGetComponent(out _animator);

            // Reset for Next Round
            if (StunEndTime + 1.0f < Time.time && health <= 0.1f)
            {
                health = MaxHealth;
                opp.health = MaxHealth;
                _animator.SetFloat(_animIDHealth, health);
                opp._animator.SetFloat(opp._animIDHealth, opp.health);
                StunEndTime = Time.time + RoundLoadTime;
                RoundCoolDown = false;
                SetTimer(RoundLoadTime * 2.0f);
            }

            if (Time.time < NextAttackTime && !ComboStarted)
            {
                _input.punch = false;
            }


            if (Time.time < Timer)
            {
                DisableInput();
                opp.DisableInput();
                Move();
            }
            else if (Timer < Time.time)
            {

                if (health <= 0.1f)
                {
                    DisableInput();
                }

                // Only accept input if the player isn't stunned
                if (StunEndTime < Time.time)
                {
                    if (!Crouch())
                    {

                        // Character cannot perform the melee SMO with a weapon in hand
                        if (WeaponInHand == null) {
                            if (PlayerType == "Mathematician") {
                                VectorAttack();
                            }
                            else if (PlayerType == "Electrical Engineer")
                            {
                                ClkAttack();
                            }
                        }

                        // Summation Attack has started
                        if (HandWeaponEquip && ExtendedSummation != null && NextHandWeaponTime < Time.time + 4.0f && ActiveClk == null)
                        {
                            Destroy(WeaponInHand);
                            WeaponInHand = Instantiate(ExtendedSummation, HandWeaponHolder.transform);
                            WeaponInHand.tag = (PlayerNumber == 1) ? "Player" : "Player2";
                            WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                        }

                        if (HandWeaponEquip)
                        {
                            _input.equipWeapon = false;
                        }

                        // Equip/Unequip Main Weapon
                        
                        if (!HandWeaponEquip)
                        {
                            EquipWeapon();
                            JumpAndGravity();
                            if (NextJumpTime < Time.time && !WeaponEquip)
                            {
                                EquipHandWeapon();
                            }

                            if (WeaponEquip)
                            {
                                WeaponSwing();
                                WeaponJab();
                            }
                            else {
                                if (KickPunch != 1)
                                {
                                    Punch();
                                }
                                if (KickPunch != 0)
                                {
                                    Kick();
                                }
                            }
                        }
                        else
                        {
                            _input.jump = false;
                        }

                        _input.punch = false;
                        _input.kick = false;
                        _input.equipHandWeapon = false;

                        GroundedCheck();

                        if (NextAttackTime - 0.1f < Time.time && NextHandWeaponTime - 2.5f < Time.time)
                        {
                            Move();
                        }
                    }
                    else
                    {
                        DisableInput();
                    }
                }
            } else
            {
                DisableInput();
            }
        }

        private void LateUpdate()
        {
            CameraRotation();

            if (Timer < Time.time)
            {

                // Player Orientation
                bool oldSwitchCondition = switchCondition;
                if (opp != null && PlayerNumber == 1 && opp.transform.position.x < transform.position.x && switchCondition)
                {
                    switchCondition = false; // Characters on the wrong side of each other
                }
                else if (PlayerNumber == 1 && opp != null && opp.transform.position.x > transform.position.x && !switchCondition)
                {
                    switchCondition = true; // Characters on starting side of each other
                }

                // The characters have switched sides
                if (opp != null && oldSwitchCondition != switchCondition)
                {
                    _degrees = (_degrees == FirstPlayerDegrees ? SecondPlayerDegrees : FirstPlayerDegrees);
                    opp._degrees = (opp._degrees == FirstPlayerDegrees ? SecondPlayerDegrees : FirstPlayerDegrees);
                }
                _animator.SetBool(_animIDAttackCoolDown, (Time.time <= NextAttackTime) || (Time.time <= NextSwordSwing));
                _animator.SetBool(_animIDStun, false);


                /* Summation Attack Handling */
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

                // Vector Attack - Start
                // Vector Destroy
                if (NextAttackTime < Time.time && vectors[0] != null)
                {
                    Destroy(vectors[0]);
                    Destroy(vectors[1]);
                    Destroy(vectors[2]);
                }
                // Angled Vectors
                else if (Time.time < NextAttackTime && vectors[0] != null && (vectors[0].transform.rotation.z > 0.18f || vectors[0].transform.rotation.z < 0.17f))
                {
                    vectors[0].transform.position = new Vector3(vectors[0].transform.position.x + VectorDirection, vectors[0].transform.position.y + 0.15f, vectors[0].transform.position.z);
                    vectors[1].transform.position = new Vector3(vectors[1].transform.position.x + VectorDirection, vectors[1].transform.position.y + 0.15f, vectors[1].transform.position.z);
                    vectors[2].transform.position = new Vector3(vectors[2].transform.position.x + VectorDirection, vectors[2].transform.position.y + 0.15f, vectors[2].transform.position.z);
                } 
                // Normal Vectors
                else if (Time.time < NextAttackTime && vectors[0] != null && vectors[0].transform.rotation.z < 0.18f && vectors[0].transform.rotation.z > 0.17f)
                {
                    vectors[0].transform.position = new Vector3(vectors[0].transform.position.x, vectors[0].transform.position.y + 0.4f, vectors[0].transform.position.z);
                    vectors[1].transform.position = new Vector3(vectors[1].transform.position.x, vectors[1].transform.position.y + 0.4f, vectors[1].transform.position.z);
                    vectors[2].transform.position = new Vector3(vectors[2].transform.position.x, vectors[2].transform.position.y + 0.4f, vectors[2].transform.position.z);

                }
                // Vector Attack - End


                // Clk - Start
                if (NextAttackTime < Time.time && ActiveClk != null)
                {
                    Destroy(ActiveClk);
                }
                // Clk - End

                if (NextAttackTime - 1.0f < Time.time && _input.smo == true)
                {
                    _input.smo = false;
                    _animator.SetBool(_animIDsmo, false);
                }

                // EE - Weapon Projectile
                bool activeSwingAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("Swing");
                bool activeJabAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("Jab");
                if (PlayerType == "Electrical Engineer"
                        && (activeJabAnimation || activeSwingAnimation)
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

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDJumping = Animator.StringToHash("Jumping");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDCoolDown = Animator.StringToHash("CoolDown");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDForwardJump = Animator.StringToHash("forwardJump"); ;
            _animIDBackwardJump = Animator.StringToHash("backwardJump"); ;
            _animIDStepBack = Animator.StringToHash("StepBack");
            _animIDPunch = Animator.StringToHash("Punching");
            _animIDKick = Animator.StringToHash("Kicking");
            _animIDAttackCoolDown = Animator.StringToHash("AttackCoolDown");
            _animIDHealth = Animator.StringToHash("Health");
            _animIDStun = Animator.StringToHash("Stun");
            _animIDWeapon = Animator.StringToHash("WeaponEquip");
            _animIDHandWeapon = Animator.StringToHash("HandWeapon");
            _animIDCrouch = Animator.StringToHash("Crouch");
            _animIDsmo = Animator.StringToHash("SMO");

            _animator.SetFloat(_animIDHealth, health);
            _animator.SetBool(_animIDStun, false);
        }


        /* Character Movement */

        private bool Crouch()
        {
            _animator.SetBool(_animIDCrouch, _input.crouch);
            return _input.crouch;
        }

        private void JumpAndGravity()
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            if (Time.time > NextJumpTime)
            {
                CoolDown = false;
                _animator.SetBool(_animIDCoolDown, CoolDown);
                if ((jump && _input.move == Vector2.zero && _animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
                        || (forwardJump && _animator.GetCurrentAnimatorStateInfo(0).IsName("ForwardFlip"))
                        || (backwardJump && _animator.GetCurrentAnimatorStateInfo(0).IsName("Backflip")))
                {
                    NextJumpTime = Time.time + JumpCoolDownTime;

                    CoolDown = true;
                    _animator.SetBool(_animIDCoolDown, CoolDown);
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // jump timeout
                    if (_jumpTimeoutDelta >= 0.0f)
                    {
                        _jumpTimeoutDelta -= Time.deltaTime;
                    }
                    _input.jump = false;
                }
            }
            else
            {
                _input.jump = false;
                jump = false;
                forwardJump = false;
                backwardJump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void DisableInput()
        {
            _input.jump = false;
            _animator.SetBool(_animIDJumping, false);
            _input.punch = false;
            _animator.SetBool(_animIDPunch, false);
            _input.kick = false;
            _animator.SetBool(_animIDKick, false);
            _input.smo = false;
            _animator.SetBool(_animIDsmo, false);
            _input.equipWeapon = WeaponEquip;
            _input.move = Vector2.zero;
            _speed = 0.0f;
            _animator.SetFloat(_animIDSpeed, _speed);
        }

        public float Degrees
        {
            get {
                return _degrees;
            }
            set {
                _degrees = value;
            }
        }

        public void SetPlayerNumber(int num)
        {
            PlayerNumber = num;
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

        private void Move()
        {
            // Stop Movement
            if (Time.time < AcceptedComboInputTime || Time.time < NextAttackTime)
            {
                _speed = 0.0f;
                _animator.SetFloat(_animIDSpeed, 0.0f);
            }

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
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

            _speed = System.Math.Min(_speed, 2.5f);
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


            /* Player Jump Input*/

            // The direction the player is going in will dictate what type of jump
            
            jump = (_input.jump && !CoolDown);
            backwardJump = ((direction == 1) ? (jump && _input.move.x < 0.0f) : (jump && _input.move.x > 0.0f)) && _speed > 0.0f;
            forwardJump = ((direction == 1) ? (jump && _input.move.x > 0.0f) : (jump && _input.move.x < 0.0f)) && _speed > 0.0f;
            jump = jump && !backwardJump && !forwardJump && _input.move == Vector2.zero;

            _animator.SetBool(_animIDStepBack, (direction == 1) ? (_input.move.x < 0.0f) : (_input.move.x > 0.0f));


            if (jump && !backwardJump && !forwardJump)
            {
                // Jump if CoolDown is Over
                _animator.SetBool(_animIDJumping, jump);
            }
            else if (backwardJump && !jump && !forwardJump)
            {
                _animator.SetBool(_animIDBackwardJump, backwardJump);
            }
            else if (forwardJump && !jump && !backwardJump)
            {
                _animator.SetBool(_animIDForwardJump, forwardJump);
            }
            else if (CoolDown)
            {
                // Stop all jump animation during CoolDown
                _animator.SetBool(_animIDJumping, false);
                _animator.SetBool(_animIDBackwardJump, false);
                _animator.SetBool(_animIDForwardJump, false);
            }

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            
            // update animator if using character
            if (_hasAnimator && Timer < Time.time)
            {
                _animator.SetFloat(_animIDSpeed, System.Math.Min(_animationBlend, _speed));
                //_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            } else if (Time.time < Timer)
            {
                _animator.SetFloat(_animIDSpeed, 0.0f);
            }
        }

        /* Hand Attacks */

        private void Kick()
        {
            if (_input.kick && KickPunch != 0)
            {
                if (CurrentComboCount < MaxComboCount)
                {
                    KickPunch = 1;
                    CurrentComboCount++;
                }
                if (CurrentComboCount == 1)
                {
                    ComboStarted = true;
                    _animator.SetBool(_animIDKick, true);
                    _input.kick = false;
                }
                else if (CurrentComboCount == 2)
                {
                    _animator.SetBool(_animIDKick, true);
                    _input.kick = false;
                }
                else if (CurrentComboCount >= MaxComboCount)
                {
                    _animator.SetBool(_animIDKick, true);
                    _input.kick = false;
                }

            }
            else if (!_input.kick && CurrentComboCount == 1 && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Mma Kick"))
            {
                ComboStarted = false;
                CurrentComboCount = 0;
                KickPunch = 2;
                _animator.SetBool(_animIDKick, false);
            }
            else if (!_input.kick && CurrentComboCount == 2
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Illegal Elbow Punch")
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Mma Kick"))
            {
                ComboStarted = false;
                CurrentComboCount = 0;
                KickPunch = 2;
                _animator.SetBool(_animIDKick, false);
            }
            else if (CurrentComboCount >= MaxComboCount
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Illegal Elbow Punch")
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Punching")
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Mma Kick"))
            {
                ComboStarted = false;
                CurrentComboCount = 0;
                KickPunch = 2;
                _animator.SetBool(_animIDKick, false);
            }

            if ((_animator.GetCurrentAnimatorStateInfo(0).IsName("Mma Kick") && CurrentComboCount == 1 || ComboStarted)
                || (_animator.GetCurrentAnimatorStateInfo(0).IsName("Illegal Elbow Punch") && CurrentComboCount == 2)
                || (_animator.GetCurrentAnimatorStateInfo(0).IsName("Punching") && CurrentComboCount >= MaxComboCount))
            {
                ComboStarted = false;
                float oppX = opp.transform.position.x;
                float X = transform.position.x;
                if (System.Math.Abs(oppX - X) <= 2.0f)
                {
                    // Punches deal 2^8 damage
                    opp.TakeDamage(256);
                }
                //NextAttackTime = Time.time + AttackCoolDownTime;

                if (CurrentComboCount >= MaxComboCount)
                {
                    CurrentComboCount = 0;
                    KickPunch = 2;
                    _animator.SetBool(_animIDKick, false);
                    // Set Attack Time Countdown
                }
            }
        }

        private void Punch()
        {
            //NextAttackTime, AttackCoolDoiwnTime, _animator.SetBool(_animIDPunch, true);, _input.punch, CurrentComboCount
            if (_input.punch && KickPunch != 1)
            {
                if (CurrentComboCount < MaxComboCount) {
                    KickPunch = 0;
                    CurrentComboCount++;
                }
                if (CurrentComboCount == 1)
                {
                    ComboStarted = true;
                    _animator.SetBool(_animIDPunch, true);
                    _input.punch = false;
                } else if (CurrentComboCount == 2)
                {
                    _animator.SetBool(_animIDPunch, true);
                    _input.punch = false;
                } else if (CurrentComboCount >= MaxComboCount)
                {
                    _animator.SetBool(_animIDPunch, true);
                    _input.punch = false;
                }

            } 
            else if (!_input.punch && CurrentComboCount == 1 && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Punching"))
            {
                ComboStarted = false;
                CurrentComboCount = 0;
                KickPunch = 2;
                _animator.SetBool(_animIDPunch, false);
            }
            else if (!_input.punch && CurrentComboCount == 2 
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Illegal Elbow Punch") 
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Punching"))
            {
                ComboStarted = false;
                CurrentComboCount = 0;
                KickPunch = 2;
                _animator.SetBool(_animIDPunch, false);
            } 
            else if (CurrentComboCount >= MaxComboCount 
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Illegal Elbow Punch") 
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Punching") 
                && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Mma Kick"))
            {
                ComboStarted = false;
                CurrentComboCount = 0;
                KickPunch = 2;
                _animator.SetBool(_animIDPunch, false);
            }

            if ((_animator.GetCurrentAnimatorStateInfo(0).IsName("Punching") && CurrentComboCount == 1 || ComboStarted)
                || (_animator.GetCurrentAnimatorStateInfo(0).IsName("Illegal Elbow Punch") && CurrentComboCount == 2)
                || (_animator.GetCurrentAnimatorStateInfo(0).IsName("Mma Kick") && CurrentComboCount >= MaxComboCount))
            {
                ComboStarted = false;
                float oppX = opp.transform.position.x;
                float X = transform.position.x;
                if (System.Math.Abs(oppX - X) <= 2.0f)
                {
                    // Punches deal 2^6 damage
                    opp.TakeDamage(64);
                }
                //NextAttackTime = Time.time + AttackCoolDownTime;

                if (CurrentComboCount >= MaxComboCount)
                {
                    CurrentComboCount = 0;
                    KickPunch = 2;
                    _animator.SetBool(_animIDPunch, false);
                    // Set Attack Time Countdown
                }
            }
        }


        /* Weapon Attacks */

        /* Equip Primary Weapon for the Character */
        private void EquipWeapon()
        {
            if (_input.equipWeapon != WeaponEquip && _input.equipWeapon && !HandWeaponEquip) // Pull out weapon 
            {
                WeaponEquip = true;
                Destroy(WeaponInSheath);
                WeaponInHand = Instantiate(Weapon, WeaponHolder.transform);
                _animator.SetBool(_animIDWeapon, WeaponEquip);
                WeaponInHand.tag = (PlayerNumber == 1) ? "Player" : "Player2";
            }
            else if (_input.equipWeapon != WeaponEquip && !_input.equipWeapon) // Put weapon away
            {
                WeaponEquip = false;
                Destroy(WeaponInHand);
                WeaponInSheath = Instantiate(Weapon, WeaponSheath.transform);
                _animator.SetBool(_animIDWeapon, WeaponEquip);
            }
        }

        private void WeaponSwing()
        {
            bool activeSwingAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("Swing");
            bool activeJabAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("Jab");
            if (NextSwordSwing < Time.time && _input.punch && !activeSwingAnimation && !activeJabAnimation)
            {
                _input.move = Vector2.zero;
                
                if (PlayerType == "Mathematician")
                {
                    _animator.SetBool(_animIDPunch, _input.punch);
                    NextSwordSwing = Time.time + AttackCoolDownTime;
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                }
                else if (PlayerType == "Electrical Engineer")
                {
                    if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Unarmed Walk Back")
                        && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Standard Run"))
                    {
                        _animator.SetBool(_animIDPunch, _input.punch);
                        
                        ProjectileFired = false;
                    }
                }
                _input.punch = false;
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

        private void WeaponJab()
        {
            bool activeSwingAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("Swing");
            bool activeJabAnimation = _animator.GetCurrentAnimatorStateInfo(0).IsName("Jab");
            if (NextSwordSwing < Time.time && _input.kick && !activeJabAnimation && !activeSwingAnimation)
            {
                _input.move = Vector2.zero;
                if (PlayerType == "Mathematician") {
                    NextSwordSwing = Time.time + AttackCoolDownTime;
                    _animator.SetBool(_animIDKick, _input.kick);
                    WeaponInHand.GetComponent<WeaponDamageHandler>().Attack();
                } else if (PlayerType == "Electrical Engineer" && _input.move == Vector2.zero)
                {
                    
                    if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Unarmed Walk Back") && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Standard Run")) {
                        //NextSwordSwing = Time.time + AttackCoolDownTime;
                        _animator.SetBool(_animIDKick, _input.kick);
                        ProjectileFired = false;

                        /*WeaponInHand.GetComponent<WeaponDamageProjectiles>().switchCondition = switchCondition;
                        WeaponInHand.GetComponent<WeaponDamageProjectiles>().SetTag((PlayerNumber == 1) ? "Player" : "Player2");

                        WeaponInHand.GetComponent<WeaponDamageProjectiles>().Attack();*/
                    }
                    
                }
                _input.kick = false;
            }
            else
            {
                _animator.SetBool(_animIDKick, false);
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Jab"))
            {
                _input.move = Vector2.zero; // no moving while the animation is active
            }
        }


        /* Quick Hand Weapon Attack - Summation */
        private void EquipHandWeapon()
        {
            if (_input.equipHandWeapon && NextHandWeaponTime < Time.time && NextAttackTime < Time.time && !HandWeaponEquip && _input.move == Vector2.zero && ActiveClk == null)
            {

                WeaponInHand = Instantiate(HandWeapon, HandWeaponHolder.transform);
                WeaponInHand.tag = (PlayerNumber == 1) ? "Player" : "Player2";

                NextAttackTime = Time.time + AttackCoolDownTime;
                NextHandWeaponTime = Time.time + HandWeaponTimeOut;

                _input.equipHandWeapon = false;
                _input.move = Vector2.zero;
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
            }

        }

        /* Special Attacks */
        private void ClkAttack()
        {
            // !_animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialMoveOne")
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
                ActiveClk = Instantiate(Clk, new Vector3(transform.position.x + (_degrees == FirstPlayerDegrees ? 2.0f : -2.0f), transform.position.y + 1.0f, transform.position.z), Quaternion.Euler(0.0f, -_degrees, 0.0f));

                ActiveClk.GetComponent<ClkCollider>().setDegrees(_degrees);
                ActiveClk.tag = (PlayerNumber == 1) ? "Player" : "Player2";

                _input.move = Vector2.zero;
                _speed = 0.0f;
                _animator.SetFloat(_animIDSpeed, 0.0f);

                ActiveClk.GetComponent<WeaponDamageHandler>().Attack();
            }
        }
        private void VectorAttack()
        {
            if (NextAttackTime < Time.time)
            {
                if (_input.smo && NextHandWeaponTime < Time.time)
                {
                    NextAttackTime = Time.time + AttackCoolDownTime;
                    _animator.SetBool(_animIDsmo, true);
                    HandWeaponEquip = false;
                }
            }

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialMoveOne") && vectors[0] == null)
            {
                // Instantiate Vector depending on Player Direction

                float angleX = 0.0f;
                float angleY = 0.0f;
                float angleZ = 0.0f;
                float angle = 0.0f;

                float x, y, z;
                float offsetY, offsetX;
                if (_input.move != Vector2.zero) // Moving
                {
                    angleX = 0.0f;
                    angleY = 180.0f;
                    angleZ = 72.0f;
                    angle = 180.0f;
                    x = -2.0f;
                    y = -2.0f;
                    z = 0.0f;
                    offsetY = 0.5f;
                    offsetX = 0.0f;
                    _input.move = Vector2.zero;
                    _speed = 0.0f;
                    _animator.SetFloat(_animIDSpeed, 0.0f);

                }
                else // Not Moving
                {
                    angleX = 0.0f;
                    angleY = 0.0f;
                    angleZ = 20.0f;
                    angle = 0.0f;
                    x = 1.0f;
                    y = -4.0f;
                    z = 0.0f;
                    offsetY = 0.0f;
                    offsetX = 0.5f;
                }
                if (_degrees == FirstPlayerDegrees) // Left and Right Vector
                {
                    vectors[0] = Instantiate(VectorLeft, new Vector3(transform.position.x + x, y, z), Quaternion.Euler(angleX, angleY, angleZ));
                    vectors[0].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[1] = Instantiate(VectorLeft, new Vector3(transform.position.x + x + offsetX, y - offsetY, z), Quaternion.Euler(angleX, angleY, angleZ));
                    vectors[1].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[2] = Instantiate(VectorLeft, new Vector3(transform.position.x + x + 2.0f * offsetX, y - 2.0f * offsetY, z), Quaternion.Euler(angleX, angleY, angleZ));
                    vectors[2].GetComponent<WeaponDamageHandler>().Attack();

                    VectorDirection = 0.2f;
                }
                else
                {                            // Normal Vectors
                    vectors[0] = Instantiate(VectorRight, new Vector3(transform.position.x - x, y, z), Quaternion.Euler(angleX, angleY - angle, angleZ));
                    vectors[0].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[1] = Instantiate(VectorRight, new Vector3(transform.position.x - x - offsetX, y - offsetY, z), Quaternion.Euler(angleX, angleY - angle, angleZ));
                    vectors[1].GetComponent<WeaponDamageHandler>().Attack();

                    vectors[2] = Instantiate(VectorRight, new Vector3(transform.position.x - x - 2.0f * offsetX, y - 2.0f * offsetY, z), Quaternion.Euler(angleX, angleY - angle, angleZ));
                    vectors[2].GetComponent<WeaponDamageHandler>().Attack();

                    VectorDirection = -0.2f;
                }
            }

            if (NextAttackTime < Time.time)
            {
                _input.smo = false;
                _animator.SetBool(_animIDsmo, false);
            }
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

        /* Damage from Attacks */
        public void TakeDamage(float damage)
        {
            // Make sure the player can't take damage while already stunned
            if (StunEndTime < Time.time && !_input.crouch)
            {
                health -= damage;
                _animator.SetFloat(_animIDHealth, health);
                _animator.SetBool(_animIDStun, true);
                StunEndTime = Time.time + 0.35f;
            }
            if (opp.getNextAttackTime() > Time.time)
            {
                health -= damage;
                StunEndTime = Time.time + AttackCoolDownTime;
                _animator.SetFloat(_animIDHealth, health);
            }

            if (health < 0.1f && !RoundCoolDown)
            {
                Round++;
                RoundCoolDown = true;
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












        /* Starter Asset Functions - Start */
        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
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

        /* Starter Asset Functions - End */
    }
}