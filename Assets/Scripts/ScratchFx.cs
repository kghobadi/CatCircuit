using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScratchFx : MonoBehaviour
{
    [SerializeField] private string animToTrigger = "hit_scratch";
    [SerializeField] private bool damangedEnemy;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Cat"))
        {
            //do something 
            Debug.Log("hit cat or player!");
            Animator enemyAnimator = other.gameObject.GetComponent<Animator>();
            if (enemyAnimator == null)
            {
                enemyAnimator = other.gameObject.GetComponentInChildren<Animator>();
            }

            if (enemyAnimator != null && !damangedEnemy)
            {
                enemyAnimator.SetTrigger(animToTrigger);
                damangedEnemy = true;
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
