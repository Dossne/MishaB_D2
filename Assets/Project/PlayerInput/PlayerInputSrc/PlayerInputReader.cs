using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VacuumSorter.PlayerInput
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private JoystickView _joystickView;
        [SerializeField] private bool _enableEditorKeyboard = true;
        [SerializeField, Range(0f, 0.5f)] private float _deadZone = 0.05f;

        private Vector2 _moveInput;

        public Vector2 MoveInput => _moveInput;

        public void BindJoystick(JoystickView joystickView)
        {
            _joystickView = joystickView;
        }

        private void Update()
        {
            var joystickInput = _joystickView != null ? _joystickView.Value : Vector2.zero;
            var result = joystickInput;

#if (UNITY_EDITOR || UNITY_STANDALONE) && ENABLE_INPUT_SYSTEM
            if (_enableEditorKeyboard)
            {
                var keyboardInput = ReadKeyboardMoveInput();
                if (keyboardInput.sqrMagnitude > result.sqrMagnitude)
                {
                    result = keyboardInput;
                }
            }
#endif

            _moveInput = result.magnitude < _deadZone ? Vector2.zero : result;
        }

#if (UNITY_EDITOR || UNITY_STANDALONE) && ENABLE_INPUT_SYSTEM
        private static Vector2 ReadKeyboardMoveInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            var x = 0f;
            var y = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                y += 1f;
            }

            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }
#endif
    }
}