using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace FindTheMissing.Input
{
    /// <summary>
    /// Manages user input across Touch and Mouse, dispatching Drag and Tap events.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Maximum pixel distance traveled to still be considered a tap.")]
        [SerializeField] private float _tapThresholdPixels = 15f;

        [Tooltip("Maximum duration in seconds between press and release to register as a tap.")]
        [SerializeField] private float _maxTapDuration = 0.3f;

        [Tooltip("Sensitivity multiplier for drag deltas.")]
        [SerializeField] private float _dragSensitivity = 1f;

        [Tooltip("Sensitivity multiplier for pinch zoom.")]
        [SerializeField] private float _pinchZoomSensitivity = 1f;

        [Tooltip("Minimum distance between two fingers before pinch is processed.")]
        [SerializeField] private float _minimumPinchDistance = 10f;

        // --------------------------------------------------
        // Pointer State
        // --------------------------------------------------
        private bool _isPressed;
        private bool _isDragging;
        private Vector2 _pressStartPosition;
        private Vector2 _lastPosition;
        private float _pressStartTime;
        // --------------------------------------------------
        // Pinch State
        // --------------------------------------------------

        private bool _isPinching;
        private float _lastPinchDistance;
        /// <summary>
        /// Positive value = fingers moving apart.
        /// Negative value = fingers moving together.
        /// </summary>
        public event Action<float> OnPinchZoomEvent;

        /// <summary>
        /// Mouse wheel vertical scroll.
        /// Positive = scroll up.
        /// Negative = scroll down.
        /// </summary>
        public event Action<float> OnScrollZoomEvent;

        // Public Events
        public event Action<Vector2> OnDragEvent;
        public event Action<Vector2> OnTapEvent;

        // Public Properties
        public bool IsDragging => _isDragging;
        public bool IsPressed => _isPressed;
        public bool IsPinching => _isPinching;

        private void Update()
        {
            //ProcessInput();
            ProcessTouchInput();
            ProcessMouseInput();
        }
        #region Touch
        private void ProcessTouchInput()
        {
            if (Touchscreen.current == null)
                return;

            TouchControl touch0 = Touchscreen.current.touches[0];
            TouchControl touch1 = Touchscreen.current.touches[1];

            bool touch0Pressed = touch0.press.isPressed;
            bool touch1Pressed = touch1.press.isPressed;

            // --------------------------------------------------
            // PINCH
            // --------------------------------------------------

            if (touch0Pressed && touch1Pressed)
            {
                HandlePinch(touch0, touch1);
                return;
            }

            // Nếu trước đó đang pinch nhưng một ngón đã nhả
            if (_isPinching)
            {
                _isPinching = false;
                _lastPinchDistance = 0f;

                // Reset pointer state để không biến thành Tap
                _isPressed = false;
                _isDragging = false;
            }


            // --------------------------------------------------
            // SINGLE TOUCH
            // --------------------------------------------------

            if (touch0Pressed)
            {
                Vector2 currentPosition = touch0.position.ReadValue();

                bool pointerDown = touch0.press.wasPressedThisFrame;

                if (pointerDown)
                {
                    HandlePointerDown(currentPosition);
                }
                else if (_isPressed)
                {
                    HandlePointerHeld(currentPosition);
                }
            }
            else if (_isPressed)
            {
                Vector2 currentPosition = touch0.position.ReadValue();

                HandlePointerUp(currentPosition);
            }
        }
        


        private void HandlePinch(
            TouchControl touch0,
            TouchControl touch1)
        {
            Vector2 position0 = touch0.position.ReadValue();
            Vector2 position1 = touch1.position.ReadValue();

            float currentDistance = Vector2.Distance(
                position0,
                position1
            );

            // Hai ngón vừa bắt đầu
            if (!_isPinching)
            {
                _isPinching = true;
                _isDragging = false;
                _isPressed = false;

                _lastPinchDistance = currentDistance;

                return;
            }

            if (currentDistance < _minimumPinchDistance)
                return;

            float delta = currentDistance - _lastPinchDistance;

            _lastPinchDistance = currentDistance;

            if (Mathf.Abs(delta) > 0.001f)
            {
                OnPinchZoomEvent?.Invoke(
                    delta * _pinchZoomSensitivity
                );
            }
        }
        #endregion
        #region Mouse
        private void ProcessMouseInput()
        {
            if (Mouse.current == null)
                return;

            Vector2 currentPosition =
                Mouse.current.position.ReadValue();

            bool pointerDown =
                Mouse.current.leftButton.wasPressedThisFrame;

            bool pointerHeld =
                Mouse.current.leftButton.isPressed;

            bool pointerUp =
                Mouse.current.leftButton.wasReleasedThisFrame;


            // --------------------------------------------------
            // MOUSE SCROLL
            // --------------------------------------------------

            Vector2 scroll =
                Mouse.current.scroll.ReadValue();

            if (Mathf.Abs(scroll.y) > 0.001f)
            {
                OnScrollZoomEvent?.Invoke(scroll.y);
            }


            // --------------------------------------------------
            // MOUSE POINTER
            // --------------------------------------------------

            if (pointerDown)
            {
                HandlePointerDown(currentPosition);
            }
            else if (_isPressed && pointerHeld)
            {
                HandlePointerHeld(currentPosition);
            }
            else if (_isPressed && pointerUp)
            {
                HandlePointerUp(currentPosition);
            }
        }
        #endregion

        #region MOUSE POINTER
        private void HandlePointerDown(Vector2 position)
        {
            _isPressed = true;
            _isDragging = false;

            _pressStartPosition = position;
            _lastPosition = position;

            _pressStartTime = Time.time;
        }
        private void HandlePointerHeld(Vector2 currentPosition)
        {
            Vector2 delta =
                (currentPosition - _lastPosition)
                * _dragSensitivity;

            _lastPosition = currentPosition;

            float distanceFromStart =
                Vector2.Distance(
                    currentPosition,
                    _pressStartPosition
                );

            if (distanceFromStart > _tapThresholdPixels)
            {
                _isDragging = true;
            }

            if (_isDragging && delta != Vector2.zero)
            {
                OnDragEvent?.Invoke(delta);
            }
        }
        private void HandlePointerUp(Vector2 currentPosition)
        {
            float duration =
                Time.time - _pressStartTime;

            float distanceFromStart =
                Vector2.Distance(
                    currentPosition,
                    _pressStartPosition
                );

            if (!_isDragging &&
                duration <= _maxTapDuration &&
                distanceFromStart <= _tapThresholdPixels)
            {
                OnTapEvent?.Invoke(
                    _pressStartPosition
                );
            }

            _isPressed = false;
            _isDragging = false;
        }
        #endregion
    }
}
