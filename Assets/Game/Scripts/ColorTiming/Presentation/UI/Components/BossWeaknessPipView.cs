// 文件职责：负责 Boss弱点Pip 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / UI / Components。

using ColorTiming.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace ColorTiming.Presentation.UI.Components
{
public class BossWeaknessPipView : MonoBehaviour
{
    public Sprite hong;
    public Sprite lv;
    public Sprite zi;
    public Sprite chen;

    public Sprite hong_t;
    public Sprite lv_t;
    public Sprite zi_t;
    public Sprite chen_t;

    public Sprite hong_w;
    public Sprite lv_w;
    public Sprite zi_w;
    public Sprite chen_w;

    public GameObject tip1;
    public GameObject tip2;

    int index = 0;
    Image image;

    // 设置Hp项目，并使后续流程使用最新状态。
    public void SetHpItem(WeaponColor color,int inx)
    {
        index = inx;
        image = GetComponentInChildren<Image>();
        if (image == null)
        {
            print("未正确获取到BossItem 的 Image");
            return;
        }
        Sprite s = null;
        //分类型
        if (inx == 0)
        {
            //image.transform.localPosition = new Vector3(0,15,0);
            switch (color)
            {
                case WeaponColor.Red:
                    s = hong_t;
                    break;
                case WeaponColor.Green:
                    s = lv_t;
                    break;
                case WeaponColor.Purple:
                    s = zi_t;
                    break;
                case WeaponColor.Orange:
                    s = chen_t;
                    break;
                default:
                    break;
            }
        }
        else if (inx == 6)
        {
            switch (color)
            {
                case WeaponColor.Red:
                    s = hong_w;
                    break;
                case WeaponColor.Green:
                    s = lv_w;
                    break;
                case WeaponColor.Purple:
                    s = zi_w;
                    break;
                case WeaponColor.Orange:
                    s = chen_w;
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (color)
            {
                case WeaponColor.Red:
                    s = hong;
                    break;
                case WeaponColor.Green:
                    s = lv;
                    break;
                case WeaponColor.Purple:
                    s = zi;
                    break;
                case WeaponColor.Orange:
                    s = chen;
                    break;
                default:
                    break;
            }
        }
        image.sprite = s;

    }

    // 显示Tip并同步当前数据。
    public void ShowTip(int ty)
    {
        if (ty > 1) {
            tip2.SetActive(true);
        }
        else
        {
            tip1.SetActive(true);
        }
    }

    // 隐藏Tip并停止相关交互。
    public void HideTip()
    {
        tip1?.SetActive(false);
        tip2?.SetActive(false);
    }

    bool flip;
    float _it;
    float lerpSpeed;
    float minimumY;
    float maximumY;

    public void Configure(float speed, float minY, float maxY)
    {
        lerpSpeed = speed;
        minimumY = minY;
        maximumY = maxY;
    }
    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {
        if (index == 0 && image != null)
        {
            float _sp = flip ? -1 : 1;
            _it += (Time.deltaTime * lerpSpeed * _sp);
            //print(_it);
            //在区间浮动
            float s = Mathf.Lerp(minimumY, maximumY, _it);
            image.transform.localPosition = new Vector3(0, s, 0);

            ///print(_it);
            if (flip)
            {
                //检查
                if (_it < 0) flip = false;
            }
            else
            {
                if (_it > 1) flip = true;
            }
        }
    }
}
}
