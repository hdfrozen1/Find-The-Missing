using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FindTheMissing.Core;

namespace FindTheMissing.UI
{
    /// <summary>
    /// Manages the in-game HUD, tracking found missing objects and displaying completion state.
    /// </summary>
    public class GameplayUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [Tooltip("Text displaying current progress (found / total).")]
        [SerializeField] private TextMeshProUGUI _progressText;

        [Tooltip("Format string for progress text.")]
        [SerializeField] private string _progressFormat = "Found: {0} / {1}";

        [Header("Level Complete Modal")]
        [Tooltip("Container panel activated upon completing the level.")]
        [SerializeField] private GameObject _completePanel;

        [Tooltip("Restart button on the complete panel.")]
        [SerializeField] private Button _restartButton;

        private void Awake()
        {
            if (_completePanel != null)
            {
                _completePanel.SetActive(false);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClicked);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProgressChanged += UpdateProgress;
                GameManager.Instance.OnLevelCompleted += HandleLevelCompleted;
                UpdateProgress(GameManager.Instance.FoundMissing, GameManager.Instance.TotalMissing);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProgressChanged -= UpdateProgress;
                GameManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
        }

        private void UpdateProgress(int found, int total)
        {
            if (_progressText != null)
            {
                _progressText.text = string.Format(_progressFormat, found, total);
            }
        }

        private void HandleLevelCompleted()
        {
            if (_completePanel != null)
            {
                _completePanel.SetActive(true);
            }
        }

        private void HandleRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }
    }
}
