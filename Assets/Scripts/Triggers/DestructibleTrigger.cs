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
    }
}
