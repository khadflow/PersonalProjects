using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class InputController : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool punch;
        public bool kick;
        public bool jab;
        public bool hit;
        public bool crouch = false;
        public bool equipWeapon = false;
        public bool equipHandWeapon = false;
        public bool smo = false;
        public bool isBlocking = false;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        // ===================================
        // INPUT CALLBACKS (Used by PlayerInput component)
        // ===================================

        public void OnMove(InputValue value)
        {
            move = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                look = value.Get<Vector2>();
            }
        }

        public void OnJump(InputValue value)
        {
            jump = value.isPressed;
        }

        public void OnSprint(InputValue value)
        {
            sprint = value.isPressed;
        }

        public void OnPunch(InputValue value)
        {
            punch = value.isPressed;
        }

        public void OnKick(InputValue value)
        {
            kick = value.isPressed;
        }

        public void OnJab(InputValue value)
        {
            jab = value.isPressed;
        }

        public void OnHit(InputValue value)
        {
            hit = value.isPressed;
        }

        public void OnCrouch(InputValue value)
        {
            // Assuming Crouch is an action you press and hold, not a toggle
            crouch = value.isPressed;
        }

        public void OnBlocking(InputValue value)
        {
            // Assuming Block is an action you press and hold
            isBlocking = value.isPressed;
        }

        // Toggles
        public void OnEquipWeapon(InputValue value)
        {
            if (value.isPressed)
            {
                equipWeapon = !equipWeapon;
            }
        }

        public void OnEquipHandWeapon(InputValue value)
        {
            if (value.isPressed)
            {
                equipHandWeapon = !equipHandWeapon;
            }
        }

        public void OnSmo(InputValue value)
        {
            if (value.isPressed)
            {
                smo = true;
            }
        }
    }
}