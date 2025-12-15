using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers specifically the Mail truck to deliver mail in order to each house. 
/// </summary>
public class MailboxTrigger : MonoBehaviour
{
    [SerializeField] private House[] houses;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            Car car = other.gameObject.GetComponent<Car>();
            if (car.carType == Car.CarType.MailTruck)
            {
                car.TriggerDeliveries(houses);
            }
        }
    }
}
