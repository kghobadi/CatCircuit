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
    [SerializeField] private SpriteRenderer houseZone;

    [Tooltip("Stat for alignment to players")]
    public float Alignment = 0;
    public CatController.CatActions favoriteAction;
    public CatController.CatActions hatedAction;
    private float friendshipMeter; //TODO maybe  this is like underlying state on a spectrum? 

    //Could think more about how to assign points. 
    [SerializeField] private Vector2Int foodAmtRange = new Vector2Int(10, 100);
    public int totalPrize; //determined when randomized
    [SerializeField] private bool hasFood;
    private bool catPlayerPresent;
    //over time could do something with the houses changing type?  
 
    [SerializeField] private HouseAudio houseAudio;
    [Header("Inhabitants")] 
    public GameObject inhabitantPrefab;//todo make this a random list 
    private Inhabitant myInhabitant;
    public bool foodCooldown;
    public bool fetchingFood;

    [Tooltip("What happens if the cat meows?")]
    [SerializeField] private Transform inhabitantPosition;
    
    private void Awake()
    {
        //add listeners
        for (int i = 0; i < GameManager.Instance.AllCats.Length; i++)
        {
            GameManager.Instance.AllCats[i].OnCatAction.AddListener(CheckCatAction);
        }
        //Reset house to neutral color. 
        houseZone.color = new Color(1, 1, 1, 0.05f); 
        //inhabitant set up
        myInhabitant = inhabitantPrefab.GetComponent<Inhabitant>();
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
            houseZone.color = new Color(0, 0, 1,  Mathf.Abs(Alignment) / 10f); 
        }
        //Neutral 
        else if (Alignment == 0)
        {
            //Reset house to neutral color. 
            houseZone.color = new Color(1, 1, 1, 0.05f); 
        }
        //Red Team
        else
        {
            houseZone.color = new Color(1, 0, 0, Mathf.Abs(Alignment) / 10f); 
        }
    }
    
    /// <summary>
    /// Determine food amt from alignment * total and call forth inhabitant if possible. 
    /// </summary>
    /// <param name="wasFaveAction"></param>
    void GetFood(bool wasFaveAction)
    {
        //Can't get food during wait time 
        if (fetchingFood || foodCooldown)
        {
            return;
        }
        
        int foodPts = Mathf.RoundToInt(Mathf.Abs(Alignment / 10) * totalPrize);
        //if its house fave, you get double. 
        if (wasFaveAction)
        {
            foodPts *= 2;
        }
        
        //catController.GainFood(foodPts); GIVE food directly to player
        StartCoroutine(WaitToSpawnInhabitant(foodPts));
    }

    IEnumerator WaitToSpawnInhabitant(int multiplier)
    {
        fetchingFood = true;
        float randomFetchWait = UnityEngine.Random.Range(myInhabitant.FetchWait.x, myInhabitant.FetchWait.y);

        yield return new WaitForSeconds(randomFetchWait);
        myInhabitant.OverrideMultiplier = multiplier;
        inhabitantPrefab.SetActive(true);
        houseAudio.RandomDoorOpen();

        yield return new WaitUntil(() => !inhabitantPrefab.gameObject.activeSelf);
        fetchingFood = false;

        foodCooldown = true;
        float randomCooldown = UnityEngine.Random.Range(myInhabitant.FoodCooldown.x, myInhabitant.FoodCooldown.y);

        yield return new WaitForSeconds(randomCooldown);
        foodCooldown = false;
    }
}
