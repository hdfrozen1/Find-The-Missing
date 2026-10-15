using System.Collections;
using UnityEngine;
namespace FindTheMissing.UI
{
    public class LoadingAnimation : MonoBehaviour
    {
        [Header("Stars")]
        [SerializeField] private LoadingStar[] stars;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.35f;
        [SerializeField] private float delayBetweenStars = 0.05f;

        private Coroutine loadingCoroutine;

        private void Awake()
        {
            //ResetStars();
        }
        void Start()
        {
            StartAnimation();
        }
        public void StartAnimation()
        {
            if (loadingCoroutine != null)
                return;

            loadingCoroutine = StartCoroutine(PlayLoop());
        }

        public void StopAnimation()
        {
            if (loadingCoroutine == null)
                return;

            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;

            ResetStars();
        }

        private IEnumerator PlayLoop()
        {
            while (true)
            {
                foreach (LoadingStar star in stars)
                {
                    yield return StartCoroutine(
                        star.Play(
                            fadeInDuration,
                            fadeOutDuration
                        )
                    );

                    yield return new WaitForSeconds(delayBetweenStars);
                }
            }
        }

        private void ResetStars()
        {
            foreach (LoadingStar star in stars)
            {
                star.SetAlpha(0f);
            }
        }
    }
}