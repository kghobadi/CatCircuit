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
    /// <summary>
    /// Attacks should be trigger based. 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageEnemy(other.gameObject);
    }

    void DamageEnemy(GameObject obj)
    {
        if (obj.CompareTag("Cat") || obj.CompareTag("Player"))
        {
            if (Inhabitant && Inhabitant.trackingTarget)
            {
                Inhabitant.TriggerAttack(obj);
            }
            else if (Car && Car.moving)
            {
                Car.Crash(obj);
            }
        }
        //Cars can crash into dogs too 
        else if (obj.CompareTag("Danger"))
        {
            if (Car && Car.moving)
            {
                Car.Crash(obj);
            }
        }
    }
}
