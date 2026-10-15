using UnityEngine;
using FindTheMissing.Input;
using FindTheMissing.Gameplay;
using UnityEngine.InputSystem;

namespace FindTheMissing.Interaction
{
    /// <summary>
    /// Receives Tap events from InputController, determines the targeted viewport camera,
    /// and performs Raycasts to detect MissingObject interactions.
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Input controller to subscribe to for tap gestures.")]
        [SerializeField] private InputController _inputController;

        [Tooltip("Camera assigned to the top viewport.")]
        [SerializeField] private Camera _topCamera;

        [Tooltip("Camera assigned to the bottom viewport.")]
        [SerializeField] private Camera _bottomCamera;

        [Header("Raycast Settings")]
        [Tooltip("Normalized Y split threshold dividing the top and bottom viewports (0.5 for 50/50).")]
        [SerializeField][Range(0.1f, 0.9f)] private float _viewportSplitY = 0.5f;

        [Tooltip("Maximum raycast distance.")]
        [SerializeField] private float _maxRaycastDistance = 100f;

        [Tooltip("Layer mask filtering interactive objects.")]
        [SerializeField] private LayerMask _interactionLayerMask = ~0;

        
        private void Awake()
        {
            if (_inputController == null)
            {
                _inputController = FindFirstObjectByType<InputController>();
            }
        }

        private void OnEnable()
        {
            if (_inputController != null)
            {
                _inputController.OnTapEvent += HandleTap;
                
            }
        }

        private void OnDisable()
        {
            if (_inputController != null)
            {
                _inputController.OnTapEvent -= HandleTap;
                
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            Camera targetCamera = DetermineCameraForTap(screenPosition);
            if (targetCamera == null)
            {
                return;
            }

            Ray ray = targetCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, _interactionLayerMask))
            {
                MissingObject missingObject = hit.collider.GetComponentInParent<MissingObject>();
                if (missingObject != null && !missingObject.IsFound)
                {
                    missingObject.Interact();
                }
                else if (missingObject == null)
                {
                    Audio.SoundManager.Instance?.PlayWrongSFX();
                }
            }
            else
            {
                Audio.SoundManager.Instance?.PlayWrongSFX();
            }
        }

        private Camera DetermineCameraForTap(Vector2 screenPosition)
        {
            float normalizedY = screenPosition.y / (float)Screen.height;

            if (normalizedY >= _viewportSplitY)
            {
                return _topCamera;
            }
            else
            {
                return _bottomCamera;
            }
        }
        
    }
}
