using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Simply triggers a given inhabitants attack method -- ie dog bite. 
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    [SerializeField] private Inhabitant Inhabitant;
    /// <summary>
    /// Attacks should be trigger based. 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Inhabitant.trackingTarget && (other.gameObject.CompareTag("Cat") || other.gameObject.CompareTag("Player")))
        {
            Inhabitant.TriggerAttack(other.gameObject);
        }
    }
}
