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
        
        //Must be another player/cat
        if ((other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Cat"))
            && other.gameObject != creator.gameObject) //Must not be the creator!
        {
            //do something 
            Animator enemyAnimator = other.gameObject.GetComponent<Animator>();
            if (enemyAnimator == null)
            {
                enemyAnimator = other.gameObject.GetComponentInChildren<Animator>();
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
