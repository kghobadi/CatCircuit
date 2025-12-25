using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Controls the behavior of the scratch effect. 
/// </summary>
public class ScratchFx : MonoBehaviour
{
    public CatController creator;
    [SerializeField] private string animToTrigger = "hit_scratch";
    [SerializeField] private bool damagedEnemy;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (damagedEnemy)
            return;
        
        DamageEnemy(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (damagedEnemy)
            return;
        
        DamageEnemy(other.gameObject);
    }

    void DamageEnemy(GameObject obj)
    {
        //Must be another player/cat
        if ((obj.CompareTag("Player") || obj.CompareTag("Cat"))
            && obj != creator.gameObject) //Must not be the creator!
        {
            //do something 
            Animator enemyAnimator = obj.GetComponent<Animator>();
            if (enemyAnimator == null)
            {
                enemyAnimator = obj.GetComponentInChildren<Animator>();
            }

            if (enemyAnimator != null && !damagedEnemy)
            {
                if (!enemyAnimator.GetBool("dead"))
                {
                    Debug.Log("hit cat or player!");
                    enemyAnimator.SetTrigger(animToTrigger);
                    damagedEnemy = true;
                }
            }
        }
        //Can also send dogs packing 
        else if (obj.CompareTag("Danger"))
        {
            Inhabitant inhab = obj.GetComponent<Inhabitant>();
            if (inhab)
            {
                if (inhab.InhabiType == Inhabitant.InhabitantType.DANGEROUS
                    && inhab._AttackType != Inhabitant.AttackType.Shoot)
                {
                    Debug.Log("hit dog or danger!");
                    inhab.DisableInhabitant();
                    damagedEnemy = true;
                }
            }
        }
    }

    public void DestroyEffect()
    {
        Destroy(gameObject);
    }

    public void RecycleEffect()
    {
        //return to pool if there is one 
    }
}
