using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo1_Atk5_I : MonoBehaviour
{
    Skill_Bo1_Atk5 Skill_Bo1_Atk5;
    private void Start()
    {
        Skill_Bo1_Atk5 = GetComponentInParent<Skill_Bo1_Atk5>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Skill_Bo1_Atk5?.ChildTrigger(collision);
    }
}
