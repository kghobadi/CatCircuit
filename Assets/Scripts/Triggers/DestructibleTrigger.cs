using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For objects that can be destroyed by attacks. 
/// </summary>
public class DestructibleTrigger : MonoBehaviour
{
    [SerializeField] private Package myPackage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Packages can be destroyed by attacks. 
        if (other.gameObject.CompareTag("Attack"))
        {
            if (myPackage)
            {
                myPackage.DestroyPackage();
            }
        }
        //trigger cat to attack if AI 
        if (other.gameObject.CompareTag("Cat") || other.gameObject.CompareTag("Player"))
        {
            CatController cat = other.gameObject.GetComponentInParent<CatController>();
            if (cat)
            {
                if (cat.IsAiEnabled)
                {
                    cat.TriggeredScratch();
                }
            }
        }
    }
}
