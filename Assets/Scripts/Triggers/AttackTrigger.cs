using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Simply triggers a given inhabitants attack method -- ie dog bite.
/// Can also be used for things like cars etc. 
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    [SerializeField] private Inhabitant Inhabitant;
    [SerializeField] private Car Car;

    [SerializeField] private bool triggersOnce;
    [SerializeField] private bool triggered;

    /// <summary>
    /// Attacks should be trigger based. 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageEnemy(other.gameObject);
    }

    /// <summary>
    /// Sometimes dogs spawn on top of things - this catches that case 
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (triggersOnce)
        {
            DamageEnemy(collision.gameObject);
        }
    }

    private void OnEnable()
    {
        triggered = false;
    }

    void DamageEnemy(GameObject obj)
    {
        //Prevent multiple triggers
        if (triggersOnce && triggered)
        {
            return;
        }

        if (obj.CompareTag("Cat") || obj.CompareTag("Player"))
        {
            if (Inhabitant && Inhabitant.trackingTarget)
            {
                Inhabitant.TriggerAttack(obj);
                triggered = true;
            }
            else if (Car && Car.moving)
            {
                Car.Crash(obj);
                triggered = true;
            }
        }
        //Cars can crash into dogs too 
        else if (obj.CompareTag("Danger"))
        {
            if (Car && Car.moving)
            {
                Car.Crash(obj);
                triggered = true;
            }
        }
    }
}
