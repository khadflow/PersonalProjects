using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : InputController
	{
#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook)
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

		public void OnJab(InputValue value)
		{
			JabInput(value.isPressed);
		}

        public void OnHit(InputValue value)
        {
            HitInput(value.isPressed);
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

		public void OnBlocking(InputValue value)
		{
			if (value.isPressed)
			{
				BlockingToggle();
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

		public void JabInput(bool newJabState)
		{
			jab = newJabState;
		}

        public void HitInput(bool newHitState)
        {
            hit = newHitState;
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

		private void BlockingToggle()
		{
			isBlocking = !isBlocking;
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