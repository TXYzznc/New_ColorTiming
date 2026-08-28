// 文件职责：定义 Web请求结果 数据及其状态语义。
// 所属模块：Extension / AwaitExtension。

using GameFramework;

public class WebRequestResult : IReference
{
    /// <summary>
    /// web请求 返回数据
    /// </summary>
    public byte[] Bytes { get; private set; }
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


    // 创建并初始化新的实例。
    public static WebRequestResult Create(byte[] bytes, bool isError, string errorMessage, object userData)
    {
        WebRequestResult webResult = ReferencePool.Acquire<WebRequestResult>();
        webResult.Bytes = bytes;
        webResult.IsError = isError;
        webResult.ErrorMessage = errorMessage;
        webResult.UserData = userData;
        return webResult;
    }

    // 执行Init对应的主要流程。
    public WebRequestResult Init(byte[] bytes, bool isError, string errorMessage, object userData)
    {
        this.Bytes = bytes;
        this.IsError = isError;
        this.ErrorMessage = errorMessage;
        this.UserData = userData;
        return this;
    }
    // 清空当前保存的运行时状态，使对象可安全复用。
    public void Clear()
    {
        Bytes = null;
        IsError = false;
        ErrorMessage = string.Empty;
        UserData = null;
    }
}