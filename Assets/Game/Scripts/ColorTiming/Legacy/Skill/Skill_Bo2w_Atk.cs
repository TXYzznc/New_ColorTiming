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

    public void BindTail(SkeletonAnimation tail)
    {
        w2 = tail;
    }

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
