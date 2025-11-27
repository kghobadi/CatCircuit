using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple food item class. 
/// </summary>
public class FoodItem : MonoBehaviour
{
    [Header("Food Settings")]
    [SerializeField] private int pointsValue;
    public int SetScore
    {
        get => pointsValue;
        set => pointsValue = value;
    }
    [SerializeField] private AudioClip foodSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CatController cat = other.gameObject.GetComponentInParent<CatController>();
            CatConsumesMe(cat);
        }
    }

    /// <summary>
    /// Called by cat hitting my trigger. 
    /// </summary>
    /// <param name="consumer"></param>
    void CatConsumesMe(CatController consumer)
    {
        consumer.GainFood(pointsValue);
        consumer.CatAudio.PlaySoundRandomPitch(foodSound, 1f);
        
        //TODO play effect before destroy? 
        //Goodbye!
        Destroy(gameObject);
    }
}
