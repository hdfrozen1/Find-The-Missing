using UnityEngine;

namespace FindTheMissing.Audio
{
    /// <summary>
    /// Singleton audio manager managing background music (BGM) and sound effects (SFX)
    /// including feedback when clicking correct or wrong objects.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [Tooltip("Audio source dedicated to playing looping background music.")]
        [SerializeField] private AudioSource _bgmAudioSource;

        [Tooltip("Audio source dedicated to playing one-shot sound effects.")]
        [SerializeField] private AudioSource _sfxAudioSource;

        [Header("Background Music")]
        [Tooltip("Default background music clip.")]
        [SerializeField] private AudioClip _defaultBgmClip;

        [Tooltip("Auto-play background music on Start.")]
        [SerializeField] private bool _autoPlayBgmOnStart = true;

        [Tooltip("Background music volume.")]
        [Range(0f, 1f)]
        [SerializeField] private float _bgmVolume = 0.5f;

        [Header("Gameplay Sound Effects")]
        [Tooltip("Sound effect played when player taps a correct missing object.")]
        [SerializeField] private AudioClip _correctSfxClip;

        [Tooltip("Sound effect played when player taps an incorrect object or misses.")]
        [SerializeField] private AudioClip _wrongSfxClip;

        [Tooltip("Sound effect volume.")]
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 1f;

        [Header("Persistence")]
        [Tooltip("Whether this GameObject should persist across scene loads.")]
        [SerializeField] private bool _dontDestroyOnLoad = false;

        // Private Internal Fields
        private bool _isBgmMuted;
        private bool _isSfxMuted;

        // Public Static Instance
        public static SoundManager Instance { get; private set; }

        // Public Properties
        public bool IsBgmMuted => _isBgmMuted;
        public bool IsSfxMuted => _isSfxMuted;
        public float BgmVolume => _bgmVolume;
        public float SfxVolume => _sfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeAudioSources();
        }

        private void Start()
        {
            if (_autoPlayBgmOnStart && _defaultBgmClip != null)
            {
                PlayBGM(_defaultBgmClip);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeAudioSources()
        {
            if (_bgmAudioSource == null)
            {
                _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }

            _bgmAudioSource.loop = true;
            _bgmAudioSource.playOnAwake = false;
            _bgmAudioSource.volume = _bgmVolume;

            if (_sfxAudioSource == null)
            {
                _sfxAudioSource = gameObject.AddComponent<AudioSource>();
            }

            _sfxAudioSource.loop = false;
            _sfxAudioSource.playOnAwake = false;
            _sfxAudioSource.volume = _sfxVolume;
        }

        #region Background Music (BGM) Methods

        /// <summary>
        /// Plays background music. If no clip is provided, uses the default clip.
        /// </summary>
        public void PlayBGM(AudioClip clip = null)
        {
            AudioClip targetClip = clip != null ? clip : _defaultBgmClip;
            if (targetClip == null || _bgmAudioSource == null)
            {
                return;
            }

            if (_bgmAudioSource.clip == targetClip && _bgmAudioSource.isPlaying)
            {
                return;
            }

            _bgmAudioSource.clip = targetClip;
            _bgmAudioSource.volume = _isBgmMuted ? 0f : _bgmVolume;
            _bgmAudioSource.Play();
        }

        /// <summary>
        /// Stops background music playback.
        /// </summary>
        public void StopBGM()
        {
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.Stop();
            }
        }

        /// <summary>
        /// Pauses background music playback.
        /// </summary>
        public void PauseBGM()
        {
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.Pause();
            }
        }

        /// <summary>
        /// Resumes background music playback.
        /// </summary>
        public void ResumeBGM()
        {
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.UnPause();
            }
        }

        /// <summary>
        /// Sets background music volume (0.0 to 1.0).
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmAudioSource != null && !_isBgmMuted)
            {
                _bgmAudioSource.volume = _bgmVolume;
            }
        }

        /// <summary>
        /// Mutes or unmutes background music.
        /// </summary>
        public void SetBGMMute(bool isMuted)
        {
            _isBgmMuted = isMuted;
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.volume = _isBgmMuted ? 0f : _bgmVolume;
            }
        }

        /// <summary>
        /// Toggles background music mute state.
        /// </summary>
        public void ToggleBGMMute()
        {
            SetBGMMute(!_isBgmMuted);
        }

        #endregion

        #region Sound Effects (SFX) Methods

        /// <summary>
        /// Plays the correct object discovery sound effect.
        /// </summary>
        public void PlayCorrectSFX()
        {
            Debug.Log("correct sfx");
            PlaySFX(_correctSfxClip);
        }

        /// <summary>
        /// Plays the incorrect tap / wrong object sound effect.
        /// </summary>
        public void PlayWrongSFX()
        {
            Debug.Log("wrong sfx");
            PlaySFX(_wrongSfxClip);
        }

        /// <summary>
        /// Plays an arbitrary sound effect clip.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || _sfxAudioSource == null || _isSfxMuted)
            {
                return;
            }

            _sfxAudioSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>
        /// Sets sound effects volume (0.0 to 1.0).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            if (_sfxAudioSource != null)
            {
                _sfxAudioSource.volume = _sfxVolume;
            }
        }

        /// <summary>
        /// Mutes or unmutes sound effects.
        /// </summary>
        public void SetSFXMute(bool isMuted)
        {
            _isSfxMuted = isMuted;
        }

        /// <summary>
        /// Toggles sound effects mute state.
        /// </summary>
        public void ToggleSFXMute()
        {
            SetSFXMute(!_isSfxMuted);
        }

        #endregion
    }
}
