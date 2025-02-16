using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

}