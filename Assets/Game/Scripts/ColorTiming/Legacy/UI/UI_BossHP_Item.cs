
using UnityEngine;
using UnityEngine.UI;

public class UI_BossHP_Item : MonoBehaviour
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

    public void SetHpItem(ColorType color,int inx)
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
                case ColorType.hong:
                    s = hong_t;
                    break;
                case ColorType.lv:
                    s = lv_t;
                    break;
                case ColorType.zi:
                    s = zi_t;
                    break;
                case ColorType.chen:
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
                case ColorType.hong:
                    s = hong_w;
                    break;
                case ColorType.lv:
                    s = lv_w;
                    break;
                case ColorType.zi:
                    s = zi_w;
                    break;
                case ColorType.chen:
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
                case ColorType.hong:
                    s = hong;
                    break;
                case ColorType.lv:
                    s = lv;
                    break;
                case ColorType.zi:
                    s = zi;
                    break;
                case ColorType.chen:
                    s = chen;
                    break;
                default:
                    break;
            }
        }
        image.sprite = s;

    }

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

    public void HideTip()
    {
        tip1?.SetActive(false);
        tip2?.SetActive(false);
    }

    bool flip;
    float _it;
    float lerpSpeed = 3;
    private void Update()
    {
        if (index == 0 && image != null)
        {
            float _sp = flip ? -1 : 1;
            _it += (Time.deltaTime * lerpSpeed * _sp);
            //print(_it);
            //在区间浮动
            float s = Mathf.Lerp(10, 20, _it);
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
