using DG.Tweening;
using ColorTiming.Presentation.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace ColorTiming.Presentation.UI.Forms
{
    /// <summary>Project-wide GF.UI form for product scene transitions.</summary>
    public sealed class ColorTimingLoadingForm : UIFormBase, IColorTimingLoadingForm
    {
        [SerializeField] private GameObject progressRoot;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image fadeImage;
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

        private Tween fadeTween;
        private bool closing;
        private float displayedProgress;

        protected override void OnOpen(object userData)
        {
            closing = false;
            fadeTween?.Kill();
            fadeTween = null;
            displayedProgress = 0f;
            if (progressRoot != null)
            {
                progressRoot.SetActive(true);
            }
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0f, 0f, 0f, 0f);
            }
            SetProgress(0f);
            base.OnOpen(userData);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            fadeTween?.Kill();
            fadeTween = null;
            closing = false;
            displayedProgress = 0f;
            base.OnClose(isShutdown, userData);
        }

        public void SetProgress(float progress)
        {
            if (progressSlider != null)
            {
                displayedProgress = Mathf.Max(displayedProgress, Mathf.Clamp01(progress));
                progressSlider.SetValueWithoutNotify(displayedProgress);
            }
        }

        public void CompleteAndClose()
        {
            if (closing)
            {
                return;
            }

            closing = true;
            if (progressRoot != null)
            {
                progressRoot.SetActive(false);
            }
            if (fadeImage == null || fadeDuration <= 0f)
            {
                GF.UI.CloseUIForm(Id);
                return;
            }

            fadeTween = DOTween.Sequence()
                .SetUpdate(true)
                .Append(fadeImage.DOFade(1f, fadeDuration))
                .Append(fadeImage.DOFade(0f, fadeDuration))
                .OnComplete(() => GF.UI.CloseUIForm(Id));
        }
    }
}
