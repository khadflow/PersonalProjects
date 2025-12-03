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

namespace StarterAssets
{
    /// <summary>
    /// Represents a single attack node in the combo graph (Trie).
    /// Stores the associated button, animation value, and links to subsequent moves.
    /// </summary>
    public class Attack
    {
        private Animator _animator;
        public ButtonControl _button;
        public string _value;

        public bool _visited = false;
        // Dictionary linking a specific button press to the next Attack in the chain.
        public Dictionary<ButtonControl, Attack> _nextMoves; // Button -> Attack Object
        private float _timing = 1.0f; // always 1 second for now

        public Attack(ButtonControl button, Animator animator, string value)
        {
            _button = button;
            _animator = animator;
            _nextMoves = new Dictionary<ButtonControl, Attack>();
            _value = value;
        }

        /// <summary>
        /// Adds a new move (Attack node) as a child of the current move, extending the combo chain.
        /// </summary>
        public Attack AddMove(ButtonControl button, string value)
        {
            Attack newAttack = new Attack(button, _animator, value);
            _nextMoves.Add(button, newAttack);
            return newAttack;
        }

        /// <summary>
        /// Executes the animation associated with this attack.
        /// This is called when the attack is dequeued for playback.
        /// </summary>
        public void Play(string val)
        {
            Debug.Log("Play animation");
            _animator.CrossFade(val, 0.05f);
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
                if (!currentPath.Contains(_value))
                {
                    currentPath.Add(_value);
                }
                Debug.Log("Combo Path: " + string.Join(" -> ", currentPath));
            }

            // Recursive Case: Traverse all next moves
            foreach (KeyValuePair<ButtonControl, Attack> pair in _nextMoves)
            {
                Attack nextAttack = pair.Value;

                // Create a new list for the next recursive call (deep clone)
                List<string> nextPath = new List<string>(currentPath);

                // Add the current attack's value to the path before moving to the next node
                if (!nextPath.Contains(_value))
                {
                    nextPath.Add(_value);
                }

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
        public Dictionary<ButtonControl, Attack> _head;
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

        public AttackTrie(ButtonControl button, Animator animator)
        {
            _animator = animator;
            _pointer = null;
            _attackQueue = new Queue<Attack>();

            // Initialize all possible starting moves (root of the trie)
            _head = new Dictionary<ButtonControl, Attack>();
            _head.Add(Gamepad.current.buttonNorth, new Attack(Gamepad.current.buttonNorth, _animator, "Jab"));
            _head.Add(Gamepad.current.buttonSouth, new Attack(Gamepad.current.buttonSouth, _animator, "Hit"));
            _head.Add(Gamepad.current.buttonEast, new Attack(Gamepad.current.buttonEast, _animator, "Kick"));
            _head.Add(Gamepad.current.buttonWest, new Attack(Gamepad.current.buttonWest, _animator, "Punch"));

            // TODO
            _head.Add(Gamepad.current.leftTrigger, new Attack(Gamepad.current.leftTrigger, _animator, "Heavy"));
            //_head.Add(Gamepad.current.rightTrigger, new Attack(Gamepad.current.rightTrigger, _animator, ""));
            _head.Add(Gamepad.current.leftShoulder, new Attack(Gamepad.current.leftShoulder, _animator, "360 Swing"));
            _head.Add(Gamepad.current.rightShoulder, new Attack(Gamepad.current.rightShoulder, _animator, "Aim"));

            // Example combo sequence: South (Hit) -> East (Kick) -> North (Jab)
            ButtonControl[] ABYCombo = {
                Gamepad.current.buttonSouth,
                Gamepad.current.buttonEast,
                Gamepad.current.buttonNorth
            };

            // Example combo sequence: West (Punch) -> North (Jab) -> East (Kick)
            ButtonControl[] XYBCombo = {
                Gamepad.current.buttonWest,
                Gamepad.current.buttonNorth,
                Gamepad.current.buttonEast,
            };

            // Example combo sequence: South (Hit) -> North (Jab) -> South (Hit)
            // Test Second combo path starting South
            ButtonControl[] AYACombo = {
                Gamepad.current.buttonSouth,
                Gamepad.current.buttonNorth,
                Gamepad.current.buttonSouth
            };

            // Add Combos to list
            addMove(ABYCombo);
            addMove(XYBCombo);
            addMove(AYACombo);

            ArrayList arr = new ArrayList();
            PrintAllCombos(); // Utility to print all defined combos

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
        }

        /// <summary>
        /// Processes a new player input ('move') to determine if it continues a combo, 
        /// starts a new one, or should be ignored.
        /// </summary>
        public void Play(string move)
        {
            // Anti-Mashing Check 1: If no combo is active (_pointer is null), block input
            // if the queue is still busy playing a previous attack.
            if (_pointer == null && _attackQueue.Count > 0)
            {
                Debug.Log("Input ignored: Queue is not empty and not in a combo.");
                return;
            }

            // Check if we are currently in a combo chain.
            if (_pointer != null)
            {
                // Check 2: SUCCESS PATH - Is the input the VALID NEXT MOVE in the current chain?
                if (_pointer._nextMoves.ContainsKey(_typeToButton[move]))
                {
                    // Valid continuation: Advance the pointer and enqueue the move for playback.
                    // This bypasses the queue check, enabling combo buffering.
                    _pointer = _pointer._nextMoves[_typeToButton[move]];
                    _attackQueue.Enqueue(_pointer);
                }
                else // Check 3: FAILURE PATH - Input was pressed, but it was NOT the correct next combo move.
                {
                    // Anti-Mashing Check 2: If the input was wrong AND the queue is busy, ignore it.
                    // This prevents spamming random buttons while an animation is playing.
                    if (_attackQueue.Count > 0)
                    {
                        Debug.Log("Input ignored: Queue is not empty.");
                        return;
                    }

                    // Input failed the combo sequence while the queue was free.
                    // Clear the queue and reset the combo pointer.
                    _attackQueue.Clear();
                    _ComboReset = Time.time - 1.0f; // Force reset immediately
                    ResetCombo();

                    // Now, try to start a new combo with the failed input button.
                    if (_head.ContainsKey(_typeToButton[move]))
                    {
                        _pointer = _head[_typeToButton[move]];
                        Debug.Log("_pointer is " + _pointer._value + " (Restarted Combo)");
                        _attackQueue.Enqueue(_pointer);
                    }
                }
            }
            // Check 4: START NEW COMBO - Is this button a valid start of ANY combo?
            else if (_head.ContainsKey(_typeToButton[move]))
            {
                // Start a new combo: Set the pointer to the root node and enqueue the move.
                _pointer = _head[_typeToButton[move]];
                Debug.Log("_pointer is " + _pointer._value);
                _attackQueue.Enqueue(_pointer);
            }
        }

        /// <summary>
        /// Executes attacks from the buffer queue. Should be called repeatedly (e.g., in LateUpdate) 
        /// and only when the character is 'idle' (not locked in an animation).
        /// </summary>
        public void EmptyQueue(bool idle)
        {
            // Execute the next attack if the queue is not empty and the character is idle.
            while (_attackQueue.Count > 0 && idle)
            {
                Attack nextAttack = _attackQueue.Dequeue();
                nextAttack.Play(nextAttack._value);
                Debug.Log("Empty Queue: " + nextAttack._value);
                _ComboReset = Time.time + 2.5f; // Extend the combo window after execution

                // If the executed move was the end of a sequence (leaf node in trie)
                // AND the queue is now empty, reset the combo pointer immediately.
                if (nextAttack._nextMoves.Count == 0 && _pointer != null && _attackQueue.Count == 0)
                {
                    _pointer = null;
                    Debug.Log("Combo FINISHED! Pointer reset after animation.");
                }
                break; // Only execute one attack per frame to spread out animation playback
            }
        }

        /// <summary>
        /// Utility function to build a specific combo sequence by traversing and creating nodes.
        /// </summary>
        private void addMove(ButtonControl[] newCombo)
        {
            if (newCombo == null || newCombo.Length == 0)
            {
                return;
            }

            // Start from the first button's root node
            Attack _pointer = _head[newCombo[0]];
            for (int i = 1; i < newCombo.Length; i++)
            {
                // If the next move already exists in the tree, just move the pointer
                if (_pointer._nextMoves.ContainsKey(newCombo[i]))
                {
                    _pointer = _pointer._nextMoves[newCombo[i]];
                }
                else // If the next move doesn't exist, create and add the new node
                {
                    // Use the animation value from the root node corresponding to this button
                    _pointer = _pointer.AddMove(newCombo[i], _head[newCombo[i]]._value);
                }
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
            if (_ComboReset < Time.time)
            {
                // Reset the combo pointer to null, indicating no active combo chain.
                _pointer = null;
            }
        }

        /// <summary>
        /// Traverses and prints all possible combo sequences defined in the AttackTrie.
        /// </summary>
        public void PrintAllCombos()
        {
            Debug.Log("--- STARTING TRIE COMBO PATHS ---");

            // Iterate over all possible starting moves
            foreach (KeyValuePair<ButtonControl, Attack> pair in _head)
            {
                Attack startAttack = pair.Value;

                // Start the path list for the recursive function
                List<string> initialPath = new List<string> { startAttack._value };

                // Start recursion from the root attack
                startAttack.PrintCombos(initialPath);
            }

            Debug.Log("--- ENDING TRIE COMBO PATHS ---");
        }
    }

    /*
     * A, B Y, X -> Starter Buttons
     * X -> (A, B, Y)
     * * */
}