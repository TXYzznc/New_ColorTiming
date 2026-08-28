// 文件职责：定义 本地化文本Mesh，承担 Common 模块中的对应职责。
// 所属模块：Common。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationTextMesh : MonoBehaviour
{
    [SerializeField] string mKey;
    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {
        var txtMesh = GetComponent<UnityEngine.TextMesh>();
        if (txtMesh != null)
        {
            txtMesh.text = GF.Localization.GetText(mKey);//.Replace("\\n", "\n");
        }
    }
}
