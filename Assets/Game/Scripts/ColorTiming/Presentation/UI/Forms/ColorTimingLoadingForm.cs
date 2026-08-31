// 文件职责：实现 ColorTiming加载 GF.UI 表单及其交互生命周期。
// 所属模块：ColorTiming / Presentation / UI / Forms。

using DG.Tweening;
using ColorTiming.Configuration;
using ColorTiming.Presentation.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace ColorTiming.Presentation.UI.Forms
{
    /// <summary>Project-wide GF.UI form for product scene transitions.</summary>
    public sealed class ColorTimingLoadingForm : UIFormBase, IColorTimingLoadingForm,
        IColorTimingPresentationConfigurationConsumer
    {
        [SerializeField] private GameObject progressRoot;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image fadeImage;
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

        private Tween fadeTween;
        private bool closing;
        private float displayedProgress;

        public void BindPresentationConfiguration(ColorTimingPresentationTable configuration)
        {
            fadeDuration = configuration.LoadingFadeDuration;
        }

        // 在 GF UI 表单打开时接收参数并刷新显示。
        protected override void OnOpen(object userData)
        {
            Log.Info(
                "[ColorTiming.UIFlow] action=LoadingForm.OnOpen id={0} frame={1} realtime={2:0.000}",
                Id,
                Time.frameCount,
                Time.realtimeSinceStartup);
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

        // 在 GF UI 表单关闭时停止流程并清理临时状态。
        protected override void OnClose(bool isShutdown, object userData)
        {
            Log.Info(
                "[ColorTiming.UIFlow] action=LoadingForm.OnClose id={0} isShutdown={1} frame={2} realtime={3:0.000}",
                Id,
                isShutdown,
                Time.frameCount,
                Time.realtimeSinceStartup);
            fadeTween?.Kill();
            fadeTween = null;
            closing = false;
            displayedProgress = 0f;
            base.OnClose(isShutdown, userData);
        }

        // 设置进度，并使后续流程使用最新状态。
        public void SetProgress(float progress)
        {
            if (progressSlider != null)
            {
                displayedProgress = Mathf.Max(displayedProgress, Mathf.Clamp01(progress));
                progressSlider.SetValueWithoutNotify(displayedProgress);
            }
        }

        // 执行完成AndClose对应的主要流程。
        public void CompleteAndClose()
        {
            if (closing)
            {
                Log.Info(
                    "[ColorTiming.UIFlow] action=LoadingForm.CompleteAndClose result=IgnoredAlreadyClosing id={0}",
                    Id);
                return;
            }

            closing = true;
            Log.Info(
                "[ColorTiming.UIFlow] action=LoadingForm.CompleteAndClose result=Begin id={0} progress={1:0.###} fadeDuration={2:0.###}",
                Id,
                displayedProgress,
                fadeDuration);
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
