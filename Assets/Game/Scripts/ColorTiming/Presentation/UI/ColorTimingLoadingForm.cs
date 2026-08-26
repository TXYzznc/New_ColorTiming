using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ColorTiming.Presentation.UI
{
    /// <summary>Project-wide GF.UI form for product scene transitions.</summary>
    public sealed class ColorTimingLoadingForm : UIFormBase, IColorTimingLoadingForm
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image fadeImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.2f;

        private Tween fadeTween;
        private bool closing;

        protected override void OnOpen(object userData)
        {
            closing = false;
            fadeTween?.Kill();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = false;
            }
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0f, 0f, 0f, 1f);
            }
            SetProgress(0f);
            base.OnOpen(userData);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            fadeTween?.Kill();
            fadeTween = null;
            closing = false;
            base.OnClose(isShutdown, userData);
        }

        public void SetProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.SetValueWithoutNotify(Mathf.Clamp01(progress));
            }
        }

        public void CompleteAndClose()
        {
            if (closing)
            {
                return;
            }

            closing = true;
            if (canvasGroup == null || fadeOutDuration <= 0f)
            {
                GF.UI.CloseUIForm(Id);
                return;
            }

            canvasGroup.blocksRaycasts = false;
            fadeTween = canvasGroup.DOFade(0f, fadeOutDuration)
                .SetUpdate(true)
                .OnComplete(() => GF.UI.CloseUIForm(Id));
        }
    }
}
