using System;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Blizzard
{
    public class PlayerInput : MonoBehaviour
    {
        public PlayerInputActions inputActions = null;
        [HideInInspector] public bool inputReady = false;

        public event Action OnInputReady;

        private void Awake()
        {
            InitPlayerInput();
        }

        private void InitPlayerInput()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable(); // Enabled by default
            inputReady = true;
            OnInputReady?.Invoke();
        }
    }

}
