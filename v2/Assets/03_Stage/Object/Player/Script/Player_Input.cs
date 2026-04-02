using UnityEngine;
using UnityEngine.InputSystem;

namespace PupSurvivors.Stage
{
    public partial class Player
    {
        public Vector2 CurInputDir { get; private set; }
        public Vector2 LastInputDir { get; private set; }

        private const float ActButtonBuffer = 0.15f;
        private float _actButtonCounter;
        public bool IsDashButtonPressed => _actButtonCounter > 0f;


        private void InitializeInput()
        {
            CurInputDir = Vector2.zero;
            LastInputDir = Vector2.right;
        }

        private void UpdateInput()
        {
            _actButtonCounter -= Time.deltaTime;
            if (_actButtonCounter > 0f)
            {
                CurState.OnActButtonDown?.Invoke();
            }
        }

        public void ResetActButtonCounter() => _actButtonCounter = 0f;

        public void OnMove(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed)
            {
                LastInputDir = CurInputDir = callbackContext.ReadValue<Vector2>();
            }
            else if (callbackContext.canceled)
            {
                CurInputDir = Vector2.zero;
            }
        }
        public void OnAct(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.started)
            {
                _actButtonCounter = ActButtonBuffer;
            }
        }
    }
}