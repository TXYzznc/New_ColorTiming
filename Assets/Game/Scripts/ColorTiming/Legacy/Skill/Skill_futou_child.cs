using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_futou_child : MonoBehaviour
{
    Skill_futou skill_futou;
    private void Start()
    {
        skill_futou = GetComponentInParent<Skill_futou>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        skill_futou?.ChildTrigger(collision);
    }
}
