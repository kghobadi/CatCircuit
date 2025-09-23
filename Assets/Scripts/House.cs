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

    [Tooltip("Stat for alignment to players")]
    public float Alignment = 0;
    public CatController.CatActions favoriteAction;
    public CatController.CatActions hatedAction;
    private float friendshipMeter; //TODO maybe  this is like underlying state on a spectrum? 
    //could do purrs/meows needed for inquisitive to get to friend 

    //Could think more about how to assign points. 
    [SerializeField] private Vector2Int foodAmtRange = new Vector2Int(10, 100);
    public int totalPrize; //determined when randomized
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
        //Reset house to neutral color. 
        house.color = new Color(1, 1, 1, 0.05f); 
    }

    /// <summary>
    /// Called at start to randomize prize amt from the house. Also chooses favorite action. 
    /// </summary>
    public void RandomizePrize()
    {
        totalPrize = UnityEngine.Random.Range(foodAmtRange.x, foodAmtRange.y);
        float randomFave = UnityEngine.Random.Range(0, 100);
        if (randomFave > 50)
        {
            favoriteAction = CatController.CatActions.PURR;
            hatedAction = CatController.CatActions.HISS;
        }
        else
        {
            favoriteAction = CatController.CatActions.MEOW;
            hatedAction = CatController.CatActions.SCRATCH;
        }
    }
    
    private void OnValidate()
    {
        //UpdateHouseType();
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
                house.color = new Color(1, 1, 1, 0.05f); 
                break;
        }

        transform.parent.gameObject.name = "House" + houseType;
    }

    private void Start()
    {
        hasFood = true;
    }

    #region Trigger Logic

    float GetDistanceFromPlayer(CatController cat)
    {
        distFromPlayer = Vector3.Distance(transform.position, cat.transform.position);
        return distFromPlayer;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            catPlayerPresent = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            catPlayerPresent = false;
        }
    }

    #endregion


    /// <summary>
    /// Receives input from a player cat action.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cat"></param>
    void CheckCatAction(CatController.CatActions action, CatController cat)
    {
        catController = cat;
        if (catPlayerPresent && GetDistanceFromPlayer(cat) < 1f)
        {
            float align = 0; 
            bool goodInteraction = false;
            switch (action)
            {
                case CatController.CatActions.MEOW:
                    //Increase alignment
                    if (cat.alignPositive)
                    {
                        align = 1;
                    }
                    else
                    {
                        align = -1;
                    }
                    goodInteraction = true;
                    break;
                case CatController.CatActions.PURR:
                    //Increase alignment
                    if (cat.alignPositive)
                    {
                        align = 1;
                    }
                    else
                    {
                        align = -1;
                    }
                    goodInteraction = true;
                    break;
                case CatController.CatActions.HISS:
                    //Decrease alignment
                    if (cat.alignPositive)
                    {
                        align = -1;
                    }
                    else
                    {
                        align = 1;
                    }
                    break;
                case CatController.CatActions.SCRATCH:
                    //Decrease alignment
                    if (cat.alignPositive)
                    {
                        align = -1;
                    }
                    else
                    {
                        align = 1;
                    }
                    break;
            }

            //Alignment can be multiplied in either direction 
            if (favoriteAction == action || action == hatedAction)
            {
                align *= 2;
            }
            //calc new alignment for this house
            Alignment += align;
            //Clamp to global alignment range 
            Alignment = Mathf.Clamp(Alignment, GameManager.Instance.AlignmentRange.x,
                GameManager.Instance.AlignmentRange.y);
            UpdateHouseVisuals();
            
            //Cats get food rewards ONLY for good actions. 
            if (goodInteraction)
            {
                //base food amt on 
                GetFood(favoriteAction == action);
            }
        }
    }

    /// <summary>
    /// Updates house according to global cat alignments. 
    /// </summary>
    void UpdateHouseVisuals()
    {
        //We're moving towards blue
        if (Alignment > 0)
        {
            house.color = new Color(0, 0, 1,  Mathf.Abs(Alignment) / 10f); 
        }
        //Neutral 
        else if (Alignment == 0)
        {
            //Reset house to neutral color. 
            house.color = new Color(1, 1, 1, 0.05f); 
        }
        //Red Team
        else
        {
            house.color =new Color(1, 0, 0, Mathf.Abs(Alignment) / 10f); 
        }
    }
    
    /// <summary>
    /// Determine food amt from alignment * total. 
    /// </summary>
    /// <param name="wasFaveAction"></param>
    void GetFood(bool wasFaveAction)
    {
        int foodPts = Mathf.RoundToInt(Mathf.Abs(Alignment / 10) * totalPrize);
        //if its house fave, you get double. 
        if (wasFaveAction)
        {
            foodPts *= 2;
        }
        
        catController.GainFood(foodPts);
        houseAudio.RandomDoorOpen();
    }

    #region Single Player Tests

    // void Update()
    // {
    //     if (catPlayerPresent)
    //     {
    //         if (Input.GetKeyDown(KeyCode.Space))
    //         {
    //             OpenDoor();
    //         }
    //     }
    // }

    /// <summary>
    /// Respond to actions depending on house type
    /// </summary>
    /// <param name="action"></param>
    /*void SinglePlayerEval(CatController.CatActions action)
    {
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
    }*/
    
    void OpenDoor()
    {
        Debug.Log("Entered " + transform.parent.gameObject.name);
        catController.TeleportCat(internalHousePosition);
        houseAudio.RandomDoorOpen();
    }

    #endregion
}
