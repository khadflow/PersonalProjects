using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.iOS;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;
using System.Threading;
using System;

namespace StarterAssets
{
    /// <summary>
    /// Represents a single attack node in the combo graph (Trie).
    /// Stores the associated button, animation value, and links to subsequent moves.
    /// </summary>
    public class Attack
    {
        private Animator _animator;
        public string _moveName;
        public bool _visited = false;
        // Dictionary linking a specific button press to the next Attack in the chain.
        public Dictionary<string, Attack> _nextMoves; // Button -> Attack Object

        public Attack(Animator animator, string moveName)
        {
            _animator = animator;
            _nextMoves = new Dictionary<string, Attack>();
            _moveName = moveName;
        }

        public Dictionary<string, Attack> GetMoves()
        {
            return _nextMoves;
        }

        /// <summary>
        /// Adds a new move (Attack node) as a child of the current move, extending the combo chain.
        /// </summary>
        public Attack AddMove(string value)
        {
            Attack newAttack = new Attack(_animator, value);
            _nextMoves.Add(value, newAttack);
            return newAttack;
        }

        /// <summary>
        /// Executes the animation associated with this attack.
        /// This is called when the attack is dequeued for playback.
        /// </summary>
        public void Play()
        {
            if (string.IsNullOrEmpty(_moveName)) return;

            Debug.Log($"Playing Animation: {_moveName}");
            // Use CrossFade for smooth blending between combo hits
            _animator.CrossFadeInFixedTime(_moveName, 0.1f);
        }

        /// <summary>
        /// Recursively traverses the combo graph (Trie) starting from this node,
        /// printing all valid combo paths (Depth First Search).
        /// </summary>
        public void PrintCombos(List<string> currentPath)
        {
            // Base Case: If this attack has no children, it's the end of a combo. Print the path.
            if (_nextMoves.Count == 0)
            {
                // Add the current attack's value if it hasn't been added yet (for the head nodes)
                
                Debug.Log("Combo Path: " + string.Join(" -> ", currentPath));
            }

            // Recursive Case: Traverse all next moves
            foreach (KeyValuePair<string, Attack> pair in _nextMoves)
            {
                Attack nextAttack = pair.Value;

                // Create a new list for the next recursive call (deep clone)
                List<string> nextPath = new List<string>(currentPath);

                // Add the current attack's value to the path before moving to the next node
                nextPath.Add(pair.Key);

                // Recursively call the next attack in the chain
                nextAttack.PrintCombos(nextPath);
            }
        }
    }

    /// <summary>
    /// Manages the entire combo structure (Trie) and handles input buffering,
    /// combo progression, anti-mashing, and timely resetting.
    /// </summary>
    public class AttackTrie
    {
        // Stores all valid starting moves for a combo (the root nodes of the trie).
        public Attack _head;
        // Pointer to the current position in the combo chain. Null when no combo is active.
        public Attack _pointer;
        private Animator _animator;
        // Maps move names ("Hit", "Kick") back to the specific ButtonControl.
        private Dictionary<string, ButtonControl> _typeToButton;
        // Timestamp used to determine when the combo window has expired.
        private float _ComboReset = Time.time;
        // Queue to buffer attacks that are input faster than the animation can play.
        private Queue<Attack> _attackQueue;
        public bool _acceptingInput = true;
        private float _lastInputTime;

        public AttackTrie(Animator animator)
        {
            _animator = animator;
            _pointer = null;
            _attackQueue = new Queue<Attack>();

            // Initialize all possible starting moves (root of the trie)
            _head = new Attack(_animator, "");
            _head.AddMove("Jab");
            _head.AddMove("Hit");
            _head.AddMove("Kick");
            _head.AddMove("Punch");

            // TODO
            //_head.Add(Gamepad.current.leftTrigger, new Attack(Gamepad.current.leftTrigger, _animator, "Heavy"));
            //_head.Add(Gamepad.current.rightTrigger, new Attack(Gamepad.current.rightTrigger, _animator, ""));
            //_head.Add(Gamepad.current.leftShoulder, new Attack(Gamepad.current.leftShoulder, _animator, "360 Swing"));
            //_head.Add(Gamepad.current.rightShoulder, new Attack(Gamepad.current.rightShoulder, _animator, "Aim"));

            //ArrayList arr = new ArrayList();
            
            // Setup reverse mapping for quick lookup from string name to button
            _typeToButton = new Dictionary<string, ButtonControl>();

            _typeToButton.Add("Jab", Gamepad.current.buttonNorth);
            _typeToButton.Add("Hit", Gamepad.current.buttonSouth);
            _typeToButton.Add("Kick", Gamepad.current.buttonEast);
            _typeToButton.Add("Punch", Gamepad.current.buttonWest);

            // TODO - Add Animations in Attack Animator
            _typeToButton.Add("Heavy", Gamepad.current.leftTrigger);
            // TODO - Fix Root Motion issue so the animation rotates
            // the character body as expected
            _typeToButton.Add("360 Swing", Gamepad.current.leftShoulder);
            _typeToButton.Add("Aim", Gamepad.current.rightShoulder);

            // Note, right Trigger is Block
            //_typeToButton.Add("", Gamepad.current.rightTrigger);

            // Example combo sequence: South (Hit) -> East (Kick) -> North (Jab)
            string[] ABYCombo = {
                "Hit",
                "Kick",
                "Jab"
            };

            // Example combo sequence: West (Punch) -> North (Jab) -> East (Kick)
            string[] XYBCombo = {
                "Punch",
                "Jab",
                "Kick",
            };

            // Example combo sequence: South (Hit) -> North (Jab) -> South (Hit)
            // Test Second combo path starting South
            string[] AYACombo = {
                "Hit",
                "Jab",
                "Hit"
            };

            // Test Weapon Combo
            string[] XYBACombo =
            {
                "Punch",
                "Jab",
                "Kick",
                "Hit"
            };

            // Add Combos to list
            addMove(ABYCombo);
            addMove(XYBCombo);
            addMove(AYACombo);
            addMove(XYBACombo);

            PrintAllCombos(); // Utility to print all defined combos
        }

        /// <summary>
        /// Utility function to build a specific combo sequence by traversing and creating nodes.
        /// </summary>
        private void addMove(string[] newCombo)
        {
            if (newCombo == null || newCombo.Length == 0)
            {
                return;
            }

            // Start from the first button's root node
            Attack _pointer;
            _head.GetMoves().TryGetValue(newCombo[0], out _pointer);

            if (_pointer == null)
            {
                _head.AddMove(newCombo[0]);
                _pointer = _head.GetMoves()[newCombo[0]];
            }

            // Iteratively build out the combo chain by adding to the _nextMoves
            for (int i = 1; i < newCombo.Length; i++)
            {
                // Add the next move to the list of the next position
                string nextButton = newCombo[i];

                // Does it exist there already? If so, continue. If not, add.
                if (!_pointer.GetMoves().Keys.Contains(nextButton))
                {
                    _pointer.AddMove(nextButton);
                }
                _pointer.GetMoves().TryGetValue(nextButton, out _pointer);
            }
        }

        /// <summary>
        /// Processes a new player input ('move') to determine if it continues a combo, 
        /// starts a new one, or should be ignored.
        /// </summary>
        public void Play(string move)
        {
            _lastInputTime = Time.time;
            // CASE A: Continuing a valid, pre-defined Combo Sequence
            if (_pointer != null && _pointer.GetMoves().ContainsKey(move))
            {
                _pointer = _pointer.GetMoves()[move];
                _attackQueue.Enqueue(_pointer);

                // ADD JUICE: Tell the animator or player script this is a "Combo Hit"
                _animator.SetTrigger("ComboLink");
                Debug.Log("<color=green>VALID COMBO LINKED!</color>");
            }
            // CASE B: Starting a fresh move (General Move)
            else if (_head.GetMoves().ContainsKey(move))
            {
                if (_attackQueue.Count == 0)
                {
                    _pointer = _head.GetMoves()[move];
                    _attackQueue.Enqueue(_pointer);
                    Debug.Log("General Move Started.");
                }
            }
        }

        /// <summary>
        /// Executes attacks from the buffer queue. Should be called repeatedly (e.g., in LateUpdate) 
        /// and only when the character is 'idle' (not locked in an animation).
        /// </summary>
        public void EmptyQueue(bool idle)
        {
            if (_attackQueue.Count == 0) return;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            // DYNAMIC THRESHOLD
            // If the next move in the queue is a valid CONTINUATION of a combo,
            // we let it play much earlier (0.5f = 50% through the current move).
            // If it's just a random button press, we make them wait (0.85f).
            float cancelThreshold = (_pointer != null) ? 0.5f : 0.85f;

            bool animationReady = stateInfo.IsName("Idle") || stateInfo.normalizedTime > cancelThreshold;

            if (animationReady && !_animator.IsInTransition(0))
            {
                Attack nextAttack = _attackQueue.Dequeue();

                // VISUAL DIFFERENTIATION
                // If we are in a combo, maybe speed up the animator slightly
                _animator.speed = (_pointer != null) ? 1.2f : 1.0f;

                nextAttack.Play();
            }
        }

        public bool isQueueEmpty()
        {
            return _attackQueue.Count == 0;
        }

        /// <summary>
        /// Resets the combo state by clearing the _pointer if the combo timing window has expired.
        /// Should be called every frame (e.g., in Update) to enforce the timeout.
        /// </summary>
        public void ResetCombo()
        {
            // Check if the silence duration has exceeded our limit (e.g., 1.2 seconds)
            if (Time.time - _lastInputTime > 1.2f)
            {
                if (_pointer != null || _attackQueue.Count > 0)
                {
                    Debug.Log("Combo Timed Out: Clearing Pointers and Queue.");

                    _pointer = null;      // Reset the Write Pointer to the start of the Trie
                    _attackQueue.Clear(); // Empty the Read Pointer so no more animations fire
                }
            }
        }

        /// <summary>
        /// Traverses and prints all possible combo sequences defined in the AttackTrie.
        /// </summary>
        public void PrintAllCombos()
        {
            Debug.Log("<color=orange>--- STARTING TRIE COMBO PATHS ---</color>");

            // _head is the root node. We need to look at its children (the starters).
            foreach (KeyValuePair<string, Attack> pair in _head.GetMoves())
            {
                Attack startAttack = pair.Value;

                // Start the path list with the name of the starter move
                List<string> initialPath = new List<string> { pair.Key };

                // Start recursion from this starter
                startAttack.PrintCombos(initialPath);
            }

            Debug.Log("<color=orange>--- ENDING TRIE COMBO PATHS ---</color>");
        }
    }

    /*
     * A, B Y, X -> Starter Buttons
     * X -> (A, B, Y)
     * * */
}