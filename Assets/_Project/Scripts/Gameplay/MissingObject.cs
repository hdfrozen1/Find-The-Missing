using System;
using UnityEngine;
using FindTheMissing.Feedback;
using FindTheMissing.Core;

namespace FindTheMissing.Gameplay
{
    /// <summary>
    /// Represents a target object in the scene that only exists in one viewport.
    /// Manages discovery state and triggers feedback and GameManager notifications.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MissingObject : MonoBehaviour
    {
        [Header("Object Details")]
        [Tooltip("Descriptive name of this missing item.")]
        [SerializeField] private string _objectName = "Missing Object";

        [Header("Feedback")]
        [Tooltip("Optional feedback component. If not assigned, will search on this GameObject.")]
        [SerializeField] private ObjectFeedback _feedback;

        // Private Internal State
        private bool _isFound;
        private Collider _collider;

        // Public Events
        public event Action<MissingObject> OnObjectFound;

        // Public Properties
        public string ObjectName => _objectName;
        public bool IsFound => _isFound;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            if (_feedback == null)
            {
                _feedback = GetComponent<ObjectFeedback>();
            }
        }

        /// <summary>
        /// Called when the player taps directly on this missing object.
        /// </summary>
        public void Interact()
        {
            if (_isFound)
            {
                return;
            }

            _isFound = true;

            // if (_collider != null)
            // {
            //     _collider.enabled = false;
            // }

            if (_feedback != null)
            {
                _feedback.PlayFeedback();
            }

            Audio.SoundManager.Instance?.PlayCorrectSFX();

            OnObjectFound?.Invoke(this);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterFound(this);
            }
        }
    }
}
