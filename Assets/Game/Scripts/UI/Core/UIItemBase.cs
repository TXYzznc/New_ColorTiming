// 文件职责：定义 UI项目基类，承担 Core 模块中的对应职责。
// 所属模块：UI / Core。

using System;
using UnityEngine;
using UnityGameFramework.Runtime;

public class UIItemBase : MonoBehaviour, ISerializeFieldTool
{
    [HideInInspector][SerializeField] SerializeFieldData[] _fields;
    public SerializeFieldData[] SerializeFieldArr { get => _fields; set => _fields = value; }

    // 缓存本组件依赖，并完成不依赖外部服务的本地初始化。
    private void Awake()
    {
        Array.Clear(_fields, 0, _fields.Length);
        OnInit();
    }

    // 在 GF 对象首次初始化时建立持久引用。
    protected virtual void OnInit()
    {
        InitLocalization();
    }
    /// <summary>
    /// 更新界面中静态文本的多语言文字
    /// </summary>
    public virtual void InitLocalization()
    {
        UIStringKey[] texts = GetComponentsInChildren<UIStringKey>(true);
        foreach (var t in texts)
        {
            if (t.TryGetComponent<TMPro.TextMeshProUGUI>(out var textMeshCom))
            {
                textMeshCom.text = GF.Localization.GetString(t.Key);
            }
            else if (t.TryGetComponent<UnityEngine.UI.Text>(out var textCom))
            {
                textCom.text = GF.Localization.GetString(t.Key);
            }
        }
    }
}
