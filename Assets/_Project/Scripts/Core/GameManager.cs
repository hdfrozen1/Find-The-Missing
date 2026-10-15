using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FindTheMissing.Gameplay;
using System.Collections;
using FindTheMissing.UI;
using System.Linq;

namespace FindTheMissing.Core
{
    /// <summary>
    /// Core Game Manager managing level lifecycle, progress tracking, and win condition.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup loadingUI;
        [SerializeField] private CanvasGroup uiInGame;
        [Header("Hint Setting")]
        [SerializeField] private Color _outLineColor;
        [SerializeField] private float _outLineWidth;
        [SerializeField] private Light _directionalLight;
        [SerializeField] private Material _hintSkybox;
        [SerializeField] private Material _normalSkybox;
        [SerializeField] private float _normalLightIntensity = 1f;
        [SerializeField] private float _hintLightIntensity = 0.25f;

        [SerializeField] private float _lightTransitionDuration = 0.5f;

        [Header("All Levels")]
        [SerializeField] private GameObject[] allLevels;
        [SerializeField] private int currentLevel;
        public bool IsLastLevel
        {
            get
            {
                return currentLevel == allLevels.Count() - 1;
            }
        }
        private GameObject currentLevelPrefab;
        [Header("Level Configuration")]
        [Tooltip("If true, automatically detects all MissingObject components in the scene on Start.")]
        [SerializeField] private bool _autoDetectMissingObjects = true;

        [Tooltip("Manually specified missing objects in the scene.")]
        [SerializeField] private List<MissingObject> _configuredMissingObjects = new List<MissingObject>();
        // Private Internal State
        private int _totalMissing;
        private int _foundMissing;
        private bool _isLevelCompleted;
        [SerializeField] private List<MissingObject> _activeMissingObjects = new List<MissingObject>();

        // Public Static Instance
        public static GameManager Instance { get; private set; }

        // Public Events
        public event Action<int, int> OnProgressChanged;
        public event Action OnLevelCompleted;

        // Public Properties
        public int TotalMissing => _totalMissing;
        public int FoundMissing => _foundMissing;
        public bool IsLevelCompleted => _isLevelCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            allLevels = Resources.LoadAll<GameObject>("Prefabs");
        }

        private void Start()
        {

            _normalLightIntensity = _directionalLight.intensity;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeLevel()
        {
            _foundMissing = 0;
            _isLevelCompleted = false;
            _activeMissingObjects.Clear();

            if (_autoDetectMissingObjects)
            {
                MissingObject[] foundInScene = FindObjectsByType<MissingObject>(FindObjectsSortMode.None);
                _activeMissingObjects.AddRange(foundInScene);
                foreach (MissingObject missingObject in foundInScene)
                {
                    Outline outline = missingObject.gameObject.AddComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }
            else
            {
                _activeMissingObjects.AddRange(_configuredMissingObjects);
            }

            _totalMissing = _activeMissingObjects.Count;
            OnProgressChanged?.Invoke(_foundMissing, _totalMissing);
        }

        /// <summary>
        /// Registers a found missing object and updates level progress.
        /// </summary>
        public void RegisterFound(MissingObject foundObject)
        {
            if (_isLevelCompleted)
            {
                return;
            }
            TurnOffHint();
            _foundMissing++;
            foundObject.GetComponent<Outline>().enabled = false;
            _activeMissingObjects.Remove(foundObject);
            OnProgressChanged?.Invoke(_foundMissing, _totalMissing);

            if (_foundMissing >= _totalMissing && _totalMissing > 0)
            {
                CompleteLevel();
            }
        }

        private void CompleteLevel()
        {
            _isLevelCompleted = true;
            OnLevelCompleted?.Invoke();
        }

        /// <summary>
        /// Reloads the current active scene to restart the level.
        /// </summary>
        public void RestartLevel()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
        public void NextLevel()
        {
            currentLevel += 1;
            if (currentLevel >= allLevels.Count())
            {
                return;
            }
            RestartOrPlayGame();
        }
        public void RestartOrPlayGame()
        {
            if (currentLevelPrefab != null)
            {
                Destroy(currentLevelPrefab);
            }
            StartCoroutine(LoadLevelCoroutine());
            IEnumerator LoadLevelCoroutine()
            {
                LoadingAnimation loadingAnimation = loadingUI.gameObject.GetComponent<LoadingAnimation>();

                // Hiện UI Loading
                loadingUI.alpha = 1;
                loadingUI.blocksRaycasts = true;
                loadingUI.interactable = true;
                loadingAnimation.StartAnimation();


                yield return null;

                // Instantiate level tại vị trí (0, 0, 0)
                if (allLevels.Length != 0 && allLevels[currentLevel] != null)
                {

                    currentLevelPrefab = Instantiate(
                        allLevels[currentLevel],
                        Vector3.zero,
                        Quaternion.identity
                    );
                }

                // Khởi tạo lại dữ liệu level
                InitializeLevel();

                // Chờ thêm 1 frame để Unity hoàn tất việc render/khởi tạo
                yield return new WaitForSeconds(2);

                // Ẩn UI Loading
                loadingAnimation.StopAnimation();
                loadingUI.alpha = 0;
                loadingUI.blocksRaycasts = false;
                loadingUI.interactable = false;

                uiInGame.alpha = 1;
                uiInGame.blocksRaycasts = true;
                uiInGame.interactable = true;

            }
        }
        public void ReturnHome()
        {
            StartCoroutine(ReturnHomeCoroutine());
            IEnumerator ReturnHomeCoroutine()
            {
                // Hiện UI Loading


                yield return null;


                Destroy(currentLevelPrefab);

                // Chờ thêm 1 frame để Unity hoàn tất việc render/khởi tạo
                yield return null;

                // Ẩn UI Loading và hiện UI home
                // LoadingUI.SetActive(false);
            }
        }
        public void GetHint()
        {
            // Lấy ngẫu nhiên missing object từ _activeMissingObjects rồi hiện hiệu ứng
            int random = UnityEngine.Random.Range(0, _activeMissingObjects.Count);
            Outline outline = _activeMissingObjects[random].GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineColor = _outLineColor;
                outline.OutlineWidth = _outLineWidth;
            }
            // Đổi Skybox
            RenderSettings.skybox = _hintSkybox;

            // Giảm ánh sáng từ từ
            if (_directionalLight != null)
            {
                StartCoroutine(ChangeLightIntensity());
            }

            DynamicGI.UpdateEnvironment();
        }
        private IEnumerator ChangeLightIntensity()
        {
            float startIntensity = _directionalLight.intensity;

            float elapsed = 0f;

            while (elapsed < _lightTransitionDuration)
            {
                elapsed += Time.deltaTime;

                float t = elapsed / _lightTransitionDuration;

                _directionalLight.intensity = Mathf.Lerp(
                    startIntensity,
                    _hintLightIntensity,
                    t
                );

                yield return null;
            }

            // Đảm bảo giá trị cuối chính xác
            _directionalLight.intensity = _hintLightIntensity;
        }
        private void TurnOffHint()
        {
            // Đổi Skybox
            RenderSettings.skybox = _normalSkybox;

            // Giảm ánh sáng từ từ
            if (_directionalLight != null)
            {
                _directionalLight.intensity = _normalLightIntensity;
            }

            DynamicGI.UpdateEnvironment();
        }
    }
}
