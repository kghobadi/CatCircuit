using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple class for package behavior. 
/// </summary>
public class Package : MonoBehaviour
{
    [SerializeField] private Transform packageDest;
    public float moveSpeed = 1f;
    public bool moving;

    [SerializeField] private House myHouse;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Vector3 posOffset = new Vector3(0, 0.2f, 0f);
    public void AssignHouse(House house)
    {
        myHouse = house;
        packageDest = myHouse.InhabPos;
        moving = true;
    }
    
    void Update()
    {
        if (moving)
        {
            transform.position = Vector3.Lerp(transform.position, packageDest.position - posOffset, moveSpeed * Time.deltaTime);
            float dist = Vector3.Distance(transform.position, packageDest.position - posOffset);
            if (dist < 0.1f)
            {
                moving = false;
            }
        }
    }

    public void DestroyPackage()
    {
        //spawn food from data table of house inhabitant and destroy this. 
        GameObject foodClone = Instantiate(foodPrefab, transform.position, Quaternion.identity);
        FoodItem foodItem = foodClone.GetComponent<FoodItem>();
        myHouse.Inhab.AssignFoodData(foodItem);
        foodItem.SetScore(myHouse.totalPrize); //set score directly based on total prize. 
            
        Destroy(gameObject);
    }
}
