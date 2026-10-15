using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace FindTheMissing.UI
{
    public class LoadingStar : MonoBehaviour
    {
        [SerializeField] private Image starImage;
        private void Awake() {
            starImage = GetComponent<Image>();
            SetAlpha(0f);
        }

        public void SetAlpha(float alpha)
        {
            Color color = starImage.color;
            color.a = alpha;
            starImage.color = color;
        }

        public IEnumerator Play(float fadeInDuration, float fadeOutDuration)
        {
            // Fade In
            yield return Fade(0f, 1f, fadeInDuration);

            // Fade Out
            yield return Fade(1f, 0f, fadeOutDuration);
        }

        private IEnumerator Fade(float startAlpha, float targetAlpha, float duration)
        {
            float elapsed = 0f;

            SetAlpha(startAlpha);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));

                yield return null;
            }

            SetAlpha(targetAlpha);
        }
    }
}