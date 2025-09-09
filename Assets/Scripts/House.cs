using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class House : MonoBehaviour
{
    private CatController catController;
    private float distFromPlayer;
    
    [SerializeField] private SpriteRenderer house;
    [SerializeField] private HouseType houseType;
    private enum HouseType
    {
        FRIENDLY = 0, // for an already friendly house - you scratch to get food
        DANGEROUS = 1, // for a dangerous house - you hiss&scratch to escape the dog
        INQUISITIVE = 2, // for an inquisitive house - you meow to make a friend 
    }

    private float friendshipMeter; //TODO maybe  this is like underlying state on a spectrum? 
    //could do purrs/meows needed for inquisitive to get to friend 

    //Could think more about how to assign points. 
    [SerializeField] private Vector2Int foodAmtRange = new Vector2Int(50, 100);
    [SerializeField] private bool hasFood;
    private bool catPlayerPresent;
    //over time could do something with the houses changing type?  

    [SerializeField] private HouseAudio houseAudio;
    [Header("Teleport Destination")]
    [Tooltip("What happens if the cat enters?")]
    [SerializeField] private Transform internalHousePosition;
    
    private void Awake()
    {
        //add listeners
        for (int i = 0; i < GameManager.Instance.AllCats.Length; i++)
        {
            GameManager.Instance.AllCats[i].OnCatAction.AddListener(CheckCatAction);
        }
    }
    
    
    private void OnValidate()
    {
        UpdateHouseType();
    }

    void UpdateHouseType()
    {
        switch (houseType)
        {
            case HouseType.FRIENDLY:
                house.color = new Color(0, 1, 0, 0.05f); 
                break;
            case HouseType.DANGEROUS:
                house.color =new Color(1, 0, 0, 0.05f); 
                break;
            case HouseType.INQUISITIVE:
                house.color = new Color(1, 1, 1, 0.05f); ;
                break;
        }

        transform.parent.gameObject.name = "House" + houseType;
    }

    private void Start()
    {
        hasFood = true;
    }
    
    void GetDistanceFromPlayer()
    {
        distFromPlayer = Vector3.Distance(transform.position, catController.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            catPlayerPresent = true;
        }
    }

    void CheckCatAction(CatController.CatActions action, CatController cat)
    {
        catController = cat;
        if (catPlayerPresent)
        {
            //respond to actions depending on house type
            switch (action)
            {
                case CatController.CatActions.MEOW:
                    if (houseType == HouseType.FRIENDLY)
                    {
                        GetFood();
                    }
                    else if (houseType == HouseType.INQUISITIVE)
                    {
                        //become friend
                        houseType = HouseType.FRIENDLY;
                        UpdateHouseType();
                    }
                    else if (houseType == HouseType.DANGEROUS)
                    {
                        //lose life 
                        catController.LoseLife();
                    }
                    break;
                case CatController.CatActions.PURR:
                    if (houseType == HouseType.FRIENDLY)
                    {
                        //add friendship points? 
                    }
                    else if (houseType == HouseType.INQUISITIVE)
                    {
                        //become friend
                        houseType = HouseType.FRIENDLY;
                        UpdateHouseType();
                    }
                    break;
                case CatController.CatActions.HISS:
                    if (houseType == HouseType.FRIENDLY)
                    {
                        //penalty to friendship? 
                    }
                    else if (houseType == HouseType.DANGEROUS)
                    {
                        //nothing? 
                    }
                    break;
                case CatController.CatActions.SCRATCH:
                    if (houseType == HouseType.FRIENDLY)
                    {
                        //penalty to friendship? 
                    }
                    else if (houseType == HouseType.DANGEROUS)
                    {
                        //lose a life?
                        //roll dice to see what happens? 
                    }
                    break;
            }
        }
    }

    void Update()
    {
        if (catPlayerPresent)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OpenDoor();
            }
        }
    }

    void OpenDoor()
    {
        Debug.Log("Entered " + transform.parent.gameObject.name);
        catController.TeleportCat(internalHousePosition);
        houseAudio.RandomDoorOpen();
    }

    void GetFood()
    {
        if (hasFood)
        {
            int randomFood = UnityEngine.Random.Range(foodAmtRange.x, foodAmtRange.y);
            catController.GainFood(randomFood);
            houseAudio.RandomDoorOpen();
        }
        else
        {
            //todo houses should get annoyed if you get food when they dont have any ?
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            catPlayerPresent = false;
        }
    }
}
