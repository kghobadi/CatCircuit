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

    /// <summary>
    /// Allows Inhabitant to determine what the house appears as. 
    /// </summary>
    /// <param name="houseSpr"></param>
    void UpdateHouseSprite(Sprite houseSpr)
    {
        if(houseSpr != null)
            house.sprite = houseSpr;
    }

    [Tooltip("Stat for alignment to players")]
    public float[] Alignments;
    private int currentAlignment;
    public CatController.CatActions favoriteAction;
    public CatController.CatActions hatedAction;

    //Could think more about how to assign points. 
    public int totalPrize; //determined when randomized
    private bool[] catPlayersPresent;
    //over time could do something with the houses changing type?  
    [SerializeField] private HouseAudio houseAudio;
    [Header("Inhabitants")] 
    [Tooltip("This is the list of possible inhabitants")]
    public GameObject[] inhabitantPrefabs;//todo make this a random list 
    [Tooltip("These must all be within 0 - 100. They should be increasing, eg 35, 55, 75")]
    [SerializeField] private float[] randomnessIntervals;
    [Tooltip("This is the spawned inhabitant at start who lives in the house")]
    public GameObject inhabitantClone;
    private Inhabitant myInhabitant;
    public Inhabitant Inhab => myInhabitant;
    public bool foodCooldown;
    public bool fetchingFood;

    [Tooltip("Spawn pos for inhabitants?")]
    [SerializeField] private Transform inhabitantPosition;
    [Tooltip("Aspirational position for AI cats?")]
    [SerializeField] private Transform catPosition;
    public Transform InhabPos => inhabitantPosition;
    public Transform CatPos => catPosition;
    private void Awake()
    {
        if (Alignments == null)
        {
            Alignments = new float[4];
        }

        catPlayersPresent = new bool[Alignments.Length];
        //add listeners
        foreach (var cat in GameManager.Instance.AllCats)
        {
            cat.OnCatAction.AddListener(CheckCatAction);
        }
        //Reset house to neutral color. 
        houseZone.color = new Color(1, 1, 1, 0.05f); 
        RandomizeInhabitant();
    }

    /// <summary>
    /// Spawns a random inhabitant from the array. 
    /// </summary>
    void RandomizeInhabitant()
    {
        //Assign food type from random table
        if (inhabitantPrefabs.Length > 1)
        {
            float randomInhab = UnityEngine.Random.Range(0f, 100f);
            for (int i = 0; i < randomnessIntervals.Length; i++)
            {
                //Check if the chance fell below the interval 
                if (randomInhab < randomnessIntervals[i])
                {
                    inhabitantClone = Instantiate(inhabitantPrefabs[i], inhabitantPosition);
                    break;
                }
            }
        }
        //Only the one option 
        else
        {
            inhabitantClone = Instantiate(inhabitantPrefabs[0], inhabitantPosition);
        }
        
        //inhabitant set up
        myInhabitant = inhabitantClone.GetComponent<Inhabitant>();
        myInhabitant.transform.localPosition = myInhabitant.SpawnOffset;
        myInhabitant.Home = this;
        UpdateHouseSprite(myInhabitant.HomeData.homeSprite);
        inhabitantClone.SetActive(false);
        //rename house for ease 
        transform.parent.gameObject.name = 
            transform.parent.gameObject.name + "_" + house.sprite.name + "_" + inhabitantClone.gameObject.name;
        //update territory zone scale 
        if(myInhabitant.HomeData.zoneScale != Vector3.zero)
            transform.localScale = myInhabitant.HomeData.zoneScale;
        if (myInhabitant.HomeData.zonePos != Vector3.zero)
        {
            transform.localPosition = myInhabitant.HomeData.zonePos;
        }
        
        //Attach UI to house rather than myself
        if (myInhabitant.InhabitantUI)
        {
            myInhabitant.InhabitantUI.transform.SetParent(transform.parent);
        }
    }

    /// <summary>
    /// Called at start to randomize prize amt from the house. Also chooses favorite action. 
    /// </summary>
    public void RandomizePrize()
    {
        totalPrize = UnityEngine.Random.Range(myInhabitant.FoodRange.x, myInhabitant.FoodRange.y);
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
            CatController cat = other.gameObject.GetComponent<CatController>();
            if (cat)
            {
                catPlayersPresent[cat.PlayerID] = true;
            }
            
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CatController cat = other.gameObject.GetComponent<CatController>();
            if (cat)
            {
                catPlayersPresent[cat.PlayerID] = false;
            }
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
        if (catPlayersPresent[cat.PlayerID] && GetDistanceFromPlayer(cat) < 1f)
        {
            float align = 0; 
            bool goodInteraction = false;
            switch (action)
            {
                case CatController.CatActions.MEOW:
                    //Increase alignment
                    align = 1;
                    goodInteraction = true;
                    break;
                case CatController.CatActions.PURR:
                    //Increase alignment
                    align = 1;
                    goodInteraction = true;
                    break;
                case CatController.CatActions.HISS:
                    //Decrease alignment
                    align = -1;
                    break;
                case CatController.CatActions.SCRATCH:
                    //Decrease alignment
                    align = -1;
                    break;
            }

            //Friendly houses update alignment then give food 
            if (myInhabitant.InhabiType == Inhabitant.InhabitantType.FRIENDLY)
            {
                //Alignment can be multiplied in either direction 
                if (favoriteAction == action || action == hatedAction)
                {
                    align *= 2;
                }
                //calc new alignment for this house/cat
                if (goodInteraction && !foodCooldown)
                {
                    UpdateAlignment(cat.PlayerID, align);
                    //Bring down current top alignment if not me 
                    if (currentAlignment != cat.PlayerID)
                    {
                        UpdateAlignment(currentAlignment, -align);
                    }
                }
                //For bad interactions, Inhabitant must be enabled to see it. 
                else
                {
                    if (inhabitantClone.activeSelf)
                    {
                        UpdateAlignment(cat.PlayerID, align);
                        //Increase current top alignment if not me 
                        if (currentAlignment != cat.PlayerID)
                        {
                            UpdateAlignment(currentAlignment, -align);
                        }
                    }
                }
                
                //Update all alignments to this house, then visuals  
                CheckGreatestAlignment();
                UpdateHouseVisuals();
            
                //Cats get food rewards ONLY for good actions. 
                if (goodInteraction && !foodCooldown)
                {
                    //base food amt on 
                    GetFood(favoriteAction == action, cat);
                }
            }
            //Dangerous houses show inhabitant no matter what 
            else if (myInhabitant.InhabiType == Inhabitant.InhabitantType.DANGEROUS && !foodCooldown)
            {
                GetAttacked();
            }
        }
    }

    /// <summary>
    /// Updates a cat alignment at ID by adding given value. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="val"></param>
    public void UpdateAlignment(int id, float val)
    {
        Alignments[id] += val;
        //Clamp to global alignment range 
        Alignments[id] = Mathf.Clamp(Alignments[id], GameManager.Instance.AlignmentRange.x,
            GameManager.Instance.AlignmentRange.y);
    }

    /// <summary>
    /// Checks current greatest Alignment index. 
    /// </summary>
    void CheckGreatestAlignment()
    {
        float greatestAlignment = 0;
        for (int i = 0; i < Alignments.Length; i++)
        {
            if (Alignments[i] > greatestAlignment)
            {
                greatestAlignment = Alignments[i];
                currentAlignment = i;
            }
        }
    }
    
    /// <summary>
    /// Updates house according to global cat alignments. 
    /// </summary>
    void UpdateHouseVisuals()
    {
        //Neutral at 0 - No alignment
        if (Alignments[currentAlignment] == 0)
        {
            //Reset house to neutral color. 
            houseZone.color = new Color(1, 1, 1, 0.05f); 
        }
        //We're moving towards that Player color 
        else if (Alignments[currentAlignment] > 0)
        {
            //Pull from player color 
            Color playerColor = GameManager.Instance.AllCats[currentAlignment].PlayerColor;
            houseZone.color = new Color(playerColor.r, playerColor.g, playerColor.b,  Mathf.Abs(Alignments[currentAlignment]) / 10f); 
        }
    }

    /// <summary>
    /// Determine food amt from alignment * total and call forth inhabitant if possible. 
    /// </summary>
    /// <param name="wasFaveAction"></param>
    /// <param name="recipient"></param>
    void GetFood(bool wasFaveAction, CatController recipient)
    {
        //Can't get food during wait time 
        if (fetchingFood || foodCooldown)
        {
            return;
        }
        
        int foodPts = Mathf.RoundToInt(Mathf.Abs(Alignments[recipient.PlayerID] / 10) * totalPrize);
        //if its house fave, you get double. 
        if (wasFaveAction)
        {
            foodPts *= 2;
        }
        myInhabitant.InhabitantUI.UpdateMultiColor(recipient.PlayerColor);
        //catController.GainFood(foodPts); GIVE food directly to player
        StartCoroutine(WaitToShowInhabitant(foodPts));
    }

    /// <summary>
    /// For determined action while inhabitant awaits. 
    /// </summary>
    /// <param name="recipient"></param>
    /// <param name="act"></param>
    public void UpdateOverrideMultiplier(CatController recipient, CatController.CatActions act)
    {
        int foodPts = Mathf.RoundToInt(Mathf.Abs(Alignments[recipient.PlayerID] / 10) * totalPrize);
        //if its house fave, you get double. 
        if (act == favoriteAction)
        {
            foodPts *= 2;
        }
        //set override multiplier again for new recipient 
        myInhabitant.OverrideMultiplier = foodPts;
        myInhabitant.InhabitantUI.UpdateMultiColor(recipient.PlayerColor);
    }

    /// <summary>
    /// Starts attack 
    /// </summary>
    void GetAttacked()
    {
        StartCoroutine(WaitToShowInhabitant(0));
    }

    IEnumerator WaitToShowInhabitant(int multiplier)
    {
        fetchingFood = true;
        foodCooldown = true;
        float randomFetchWait = UnityEngine.Random.Range(myInhabitant.FetchWait.x, myInhabitant.FetchWait.y);
       
        //Show timer and multi?
        //start timer 
        if (myInhabitant.InhabitantUI)
        {
            myInhabitant.InhabitantUI.Fader.FadeIn();
            myInhabitant.InhabitantUI.FaceAnim.SetFloat("Face", 3f); //timer 
            myInhabitant.InhabitantUI.BeginTimerCountdown(randomFetchWait, catController.PlayerColor);
        }

        yield return new WaitForSeconds(randomFetchWait);
        myInhabitant.OverrideMultiplier = multiplier;
        myInhabitant.gameObject.SetActive(true);
        houseAudio.RandomDoorOpen();

        yield return new WaitUntil(() => ! myInhabitant.gameObject.activeSelf);
        if (myInhabitant.InhabitantUI)
        {
            myInhabitant.InhabitantUI.Fader.FadeOut();
        }
        fetchingFood = false;

        float randomCooldown = UnityEngine.Random.Range(myInhabitant.FoodCooldown.x, myInhabitant.FoodCooldown.y);

        yield return new WaitForSeconds(randomCooldown);
        foodCooldown = false;
    }

}
