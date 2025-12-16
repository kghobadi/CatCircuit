using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bullet zooms in its forward direction. 
/// </summary>
public class Bullet : MonoBehaviour
{
    private Rigidbody2D bullet;
    [SerializeField] private float bulletForce;
    public Transform target;
    private Vector2 dest;
    private Vector2 dir;
    void Awake()
    {
        bullet = GetComponent<Rigidbody2D>();
    }

    public void Fire(Transform _target)
    {
        target = _target;
        dest = target.position;
        dir = dest - (Vector2)transform.position;
        bullet.AddForce(dir * bulletForce, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Cat"))
        {
            Debug.Log("hit cat or player!");
            DamageCat(other.gameObject);
        }
    }

    /// <summary>
    /// Damage the player. 
    /// </summary>
    /// <param name="cat"></param>
    void DamageCat(GameObject cat)
    {
        Animator enemyAnimator = cat.GetComponent<Animator>();
        if (enemyAnimator == null)
        {
            enemyAnimator = cat.GetComponentInChildren<Animator>();
            //try in parent
            if (enemyAnimator == null)
            {
                enemyAnimator = cat.GetComponentInParent<Animator>();
            }
        }

        if (enemyAnimator != null)
        {
            if (!enemyAnimator.GetBool("dead"))
            {
                enemyAnimator.SetTrigger("hit_scratch"); // this determines dmg 
            }
        }
        
        //Destroy me! 
        Destroy(gameObject);
    }
}
