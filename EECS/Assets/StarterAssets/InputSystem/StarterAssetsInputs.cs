using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool punch;
		public bool kick;
		public bool crouch = false;
		public bool equipWeapon = false;
		public bool equipHandWeapon = false;
		public bool smo = false;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnPunch(InputValue value) {
			PunchInput(value.isPressed);
		}

		public void OnKick(InputValue value) {
			KickInput(value.isPressed);
		}

		public void OnWeapon(InputValue value) {
			if (value.isPressed) {
				WeaponInput();
			}
		}

		public void OnHandWeapon(InputValue value) {
			if (value.isPressed) {
				HandWeaponInput();
			}
		}

		public void OnCrouch(InputValue value) 
		{
			if (value.isPressed) {
				CrouchToggle();
			}
		}

		public void OnSpecialMoveOne(InputValue value) 
		{
			if (value.isPressed) {
				SMO();
			}
		}
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void PunchInput(bool newPunchState)
		{
			punch = newPunchState;
		}

		public void KickInput(bool newKickState)
		{
			kick = newKickState;
		}

		public void WeaponInput()
		{
            equipWeapon = !equipWeapon; // flip boolean
        }

		public void HandWeaponInput()
		{
			equipHandWeapon = !equipHandWeapon;
		}

		private void CrouchToggle()
		{
			crouch = !crouch;
		}

		private void SMO()
		{
			smo = true;
		}
		///

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}