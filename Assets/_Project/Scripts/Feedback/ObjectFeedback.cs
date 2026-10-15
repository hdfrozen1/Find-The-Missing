using System.Collections;
using UnityEngine;

namespace FindTheMissing.Feedback
{
    /// <summary>
    /// Handles visual and audio feedback when a missing object is discovered.
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class ObjectFeedback : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Peak scale multiplier during the bounce feedback.")]
        [SerializeField] private float _punchScaleMultiplier = 1.35f;

        [Tooltip("Total duration of the scale punch animation in seconds.")]
        [SerializeField] private float _animationDuration = 0.45f;

        [Header("Highlight Settings")]
        [Tooltip("Optional MeshRenderer to highlight with emission or color.")]
        [SerializeField] private Renderer _targetRenderer;

        [Tooltip("Highlight emission color applied on discovery.")]
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.2f, 1f);

        [Header("Audio Settings")]
        [Tooltip("Optional AudioSource component to play sound effect.")]
        [SerializeField] private AudioSource _audioSource;

        [Tooltip("Audio clip played when found.")]
        [SerializeField] private AudioClip _foundSoundClip;

        [Header("Particle Settings")]
        [Tooltip("Optional particle system spawned or played when found.")]
        [SerializeField] private ParticleSystem _particleEffect;

        // Private Internal Fields
        private Vector3 _originalScale;
        private Coroutine _animationCoroutine;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _originalScale = transform.localScale;

            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponentInChildren<Renderer>();
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Triggers visual and audio feedback.
        /// </summary>
        public void PlayFeedback()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
            _animationCoroutine = StartCoroutine(BounceRoutine());

            TriggerHighlight();
            PlayAudio();

            if (_particleEffect != null)
            {
                _particleEffect.Play();
            }
        }

        private IEnumerator BounceRoutine()
        {
            float elapsed = 0f;
            Vector3 targetPunchScale = _originalScale * _punchScaleMultiplier;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _animationDuration;

                // Elastic sinusoidal bounce curve
                float curveValue = Mathf.Sin(progress * Mathf.PI);
                transform.localScale = Vector3.Lerp(_originalScale, targetPunchScale, curveValue);

                yield return null;
            }

            transform.localScale = _originalScale;
            _animationCoroutine = null;
        }

        private void TriggerHighlight()
        {
            if (_targetRenderer != null)
            {
                _targetRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_EmissionColor", _highlightColor);
                _targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void PlayAudio()
        {
            if (_audioSource != null && _foundSoundClip != null)
            {
                _audioSource.PlayOneShot(_foundSoundClip);
            }
        }
    }
}
