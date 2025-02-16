using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScratchFx : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Cat"))
        {
            //do soemething 
            Debug.Log("hit cat or player!");
        }
    }
}
