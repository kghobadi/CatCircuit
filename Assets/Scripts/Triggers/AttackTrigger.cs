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
        if (other.gameObject.CompareTag("Cat") || other.gameObject.CompareTag("Player"))
        {
            if (Inhabitant && Inhabitant.trackingTarget)
            {
                Inhabitant.TriggerAttack(other.gameObject);
            }
            else if (Car && Car.moving)
            {
                Car.Crash(other.gameObject);
            }
        }
        //Cars can crash into dogs too 
        else if (other.gameObject.CompareTag("Danger"))
        {
            if (Car && Car.moving)
            {
                Car.Crash(other.gameObject);
            }
        }
    }
}
