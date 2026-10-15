using UnityEngine;
using FindTheMissing.Core;
using TMPro;
namespace FindTheMissing.UI
{
    public class UIGamePlay : MonoBehaviour
    {
        [Header("HUD Elements")]
        [Tooltip("Text displaying current progress (found / total).")]
        [SerializeField] private TextMeshProUGUI _progressText;

        [Tooltip("Format string for progress text.")]
        [SerializeField] private string _progressFormat = "Found: {0} / {1}";

        [Header("Level Complete Modal")]
        [Tooltip("Container panel activated upon completing the level.")]
        [SerializeField] private CanvasGroup _winPanel;
        [SerializeField] private CanvasGroup _completeGamePanel;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProgressChanged += UpdateProgress;
                GameManager.Instance.OnLevelCompleted += HandleLevelCompleted;
                UpdateProgress(GameManager.Instance.FoundMissing, GameManager.Instance.TotalMissing);
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
            if (GameManager.Instance.IsLastLevel)
            {
                if (_completeGamePanel != null)
                {
                    _completeGamePanel.alpha = 1;
                    _completeGamePanel.blocksRaycasts = true;
                    _completeGamePanel.interactable = true;
                }
            }
            else if (_winPanel != null)
            {
                _winPanel.alpha = 1;
                _winPanel.blocksRaycasts = true;
                _winPanel.interactable = true;
            }
        }
    }
}
