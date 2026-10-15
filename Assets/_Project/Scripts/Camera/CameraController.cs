using UnityEngine;
using FindTheMissing.Input;
using System;
namespace FindTheMissing.Cameras
{
    public class CameraController : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField] private Camera _topCamera;
        [SerializeField] private Camera _bottomCamera;
        [Header("Zoom Settings")]

        [SerializeField] private bool _zoomBothCameras = true;

        [SerializeField] private float _zoomSensitivity = 0.05f;

        [SerializeField] private float _minFieldOfView = 30f;

        [SerializeField] private float _maxFieldOfView = 70f;

        [Header("Input")]
        [SerializeField] private InputController _inputController;

        [Header("Rotation Settings")]
        [SerializeField] private float _rotationSpeed = 0.2f;
        [SerializeField] private float _smoothTime = 0.08f;

        [SerializeField] private bool _invertYaw = false;
        [SerializeField] private bool _invertRoll = false;
        [SerializeField] private float _minAngle;
        [SerializeField] private float _maxAngle;
        
        private float _targetYaw;
        private float _targetRoll;

        private float _currentYaw;
        private float _currentRoll;

        private float _yawVelocity;
        private float _rollVelocity;

        private float _fixedPitch;

        private void Awake()
        {
            // Lưu X ban đầu của CameraRoot.
            // X sẽ không bao giờ bị thay đổi bởi controller.
            Vector3 currentEuler = transform.localEulerAngles;

            _fixedPitch = currentEuler.x;

            _currentYaw = currentEuler.y;
            _currentRoll = currentEuler.z;

            _targetYaw = _currentYaw;
            _targetRoll = _currentRoll;

            // Tự tìm InputController nếu chưa được gán.
            if (_inputController == null)
            {
                _inputController = FindFirstObjectByType<InputController>();
            }

            ConfigureViewports();
            
        }

        private void OnEnable()
        {
            if (_inputController != null)
            {
                _inputController.OnDragEvent += HandleDrag;
                _inputController.OnPinchZoomEvent += HandlePinchZoom;
                _inputController.OnScrollZoomEvent += HandleScrollZoom;
            }
        }

        private void OnDisable()
        {
            if (_inputController != null)
            {
                _inputController.OnDragEvent -= HandleDrag;
            }
        }

        private void Update()
        {
            ApplyRotation();
            
        }

        private void HandleDrag(Vector2 delta)
        {
            float yawSign = _invertYaw ? -1f : 1f;
            float rollSign = _invertRoll ? -1f : 1f;

            // Kéo ngang → xoay Y.
            _targetYaw += delta.x * _rotationSpeed * yawSign;

            // Kéo dọc → xoay Z.
            _targetRoll += delta.y * _rotationSpeed * rollSign;
        }

        private void ApplyRotation()
        {
            _currentYaw = Mathf.SmoothDampAngle(
                _currentYaw,
                _targetYaw,
                ref _yawVelocity,
                _smoothTime
            );

            _currentRoll = Mathf.SmoothDampAngle(
                _currentRoll,
                _targetRoll,
                ref _rollVelocity,
                _smoothTime
            );

            // X giữ nguyên giá trị ban đầu.
            // Chỉ Y và Z được thay đổi.
            transform.localRotation = Quaternion.Euler(
                _fixedPitch,
                _currentYaw,
                Math.Clamp(_currentRoll, _minAngle, _maxAngle)
            );
        }

        private void ConfigureViewports()
        {
            if (_topCamera == null || _bottomCamera == null)
            {
                return;
            }

            // Camera trên.
            _topCamera.rect = new Rect(
                0f,
                0.5f,
                1f,
                0.5f
            );

            // Camera dưới.
            _bottomCamera.rect = new Rect(
                0f,
                0f,
                1f,
                0.5f
            );
        }
        private void HandlePinchZoom(float delta)
        {
            ApplyZoom(delta);
        }
        private void HandleScrollZoom(float delta)
        {
            ApplyZoom(delta);
        }
        
        private void ApplyZoom(float delta)
        {
            float zoomAmount = delta * _zoomSensitivity;

            if (_zoomBothCameras)
            {
                ZoomCamera(_topCamera, zoomAmount);
                ZoomCamera(_bottomCamera, zoomAmount);
            }
        }
        private void ZoomCamera(Camera camera, float amount)
        {
            if (camera == null)
                return;

            camera.fieldOfView -= amount;

            camera.fieldOfView = Mathf.Clamp(
                camera.fieldOfView,
                _minFieldOfView,
                _maxFieldOfView
            );
        }
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            _smoothTime = Mathf.Max(0.001f, _smoothTime);
        }

#endif
    }
}