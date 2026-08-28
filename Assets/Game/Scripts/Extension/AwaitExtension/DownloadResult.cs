// 文件职责：定义 下载结果 数据及其状态语义。
// 所属模块：Extension / AwaitExtension。

using System;
using GameFramework;

/// <summary>
/// DownLoad 结果
/// </summary>
public class DownloadResult : IReference
{
    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool IsError { get; private set; }
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; private set; }
    /// <summary>
    /// 自定义数据
    /// </summary>
    public object UserData { get; private set; }

    public static DownloadResult Create(bool isError, string errorMessage, object userData)
    {
        DownloadResult downLoadResult = ReferencePool.Acquire<DownloadResult>();
        downLoadResult.IsError = isError;
        downLoadResult.ErrorMessage = errorMessage;
        downLoadResult.UserData = userData;
        return downLoadResult;
    }

    // 清空当前保存的运行时状态，使对象可安全复用。
    public void Clear()
    {
        IsError = false;
        ErrorMessage = string.Empty;
        UserData = null;
    }
}