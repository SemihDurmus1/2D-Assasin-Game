using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    //To do: Ölünce Box collider triggera falan donusmeli, karakterimizi engelliyor cunku

    [SerializeField] private float health = 100;
    private bool isDeath = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void GetDamage(int damageAmount)
    {
        if (isDeath) { return; }//This check, if death anim started already,
                                //returns null to prevent start death again 
        health -= damageAmount;
        if (health <= 0)
        {
            isDeath = true;
            Die();
        }
        else
        {
            animator.SetTrigger("getDamage");
        }
    }

    private void Die()
    {
        animator.SetTrigger("death");
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    public void EventDestroyEnemy()//Animation Event!!!
    {
        Destroy(gameObject);
    }
}
