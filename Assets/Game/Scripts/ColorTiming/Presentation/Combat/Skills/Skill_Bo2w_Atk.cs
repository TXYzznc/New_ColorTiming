// 文件职责：实现战斗技能 Skill_Bo2w_Atk 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo2w_Atk : Skill_base
{

    public string boneName = "Zhen3";
    // Start is called before the first frame update
    SkeletonAnimation w2;

    // 绑定尾部依赖或事件监听。
    public void BindTail(SkeletonAnimation tail)
    {
        w2 = tail;
    }

    // 执行ChildUpdate对应的主要流程。
    protected override void ChildUpdate()
    {
        if (w2 != null)
        {
            Skeleton skeleton = w2.skeleton;
            Bone bone = skeleton.FindBone(boneName);

            if (bone == null)
            {
                Debug.LogError($"Boss2 tail bone '{boneName}' was not found.", this);
                return;
            }

            Vector2 boneWorldPosition = w2.transform.TransformPoint(bone.WorldX, bone.WorldY, 0);

            Vector2 _p = Quaternion.AngleAxis(bone.Rotation, Vector3.forward) * new Vector3(0, -2.08f, 0);
            transform.position = boneWorldPosition + _p;
        }
    }

}
