// 文件职责：延后场景切换，确保 Loading UI 有一个可见帧。
// 所属模块：ColorTiming / Bootstrap。

using System;
using System.Collections;
using UnityEngine;

namespace ColorTiming.Bootstrap
{
    internal sealed class ColorTimingTransitionScheduler : MonoBehaviour
    {
        private Coroutine pendingDispatch;

        // 调度一次在下一可见帧执行的场景派发，防止同一转换重复入队。
        internal void Schedule(Action dispatch)
        {
            if (dispatch == null || pendingDispatch != null)
            {
                return;
            }

            pendingDispatch = StartCoroutine(DispatchAfterPresentation(dispatch));
        }

        private IEnumerator DispatchAfterPresentation(Action dispatch)
        {
            if (UnityEngine.Application.isBatchMode)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }

            pendingDispatch = null;
            dispatch();
        }
    }
}
