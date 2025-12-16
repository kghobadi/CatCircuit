using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Can slowdown things like dogs and other dangers.
/// Can also despawn or disable cars? 
/// </summary>
public class SlowdownTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Danger"))
        {
            Inhabitant inhab = other.gameObject.GetComponent<Inhabitant>();
            inhab.Slowdown();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            Car car = other.gameObject.GetComponent<Car>();
            car.CountTriggers();
        }
        else if (other.gameObject.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
        }
    }
}
