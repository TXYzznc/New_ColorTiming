using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HeroHPItem : MonoBehaviour
{

    public void SetHeroHPItem(int idx)
    {
        //print("wanjia hp + " + idx);
        Image image = GetComponentInChildren<Image>();
        if (image == null ) {return ;}
        if (idx % 2 == 0)
        {

        }
        else 
        {
            image.rectTransform.localPosition = new Vector3(0, -33, 0);

        }


    }
}
