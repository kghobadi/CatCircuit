using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// Simple wait to throw method for Inhabitants. 
/// </summary>
public class Inhabitant : AudioHandler
{
    [SerializeField] private InhabitantType inhabitantType;
    private enum InhabitantType
    {
        FRIENDLY = 0, // for an already friendly house - you scratch to get food
        DANGEROUS = 1, // for a dangerous house - you hiss&scratch to escape the dog
    }
    
    [Header("Throw Settings")]
    [Tooltip("How long does it take for this Inhabitant to recharge?")]
    [SerializeField] private Vector2 foodCooldownWait = new Vector2(3f, 5f);
    public Vector2 FoodCooldown => foodCooldownWait;
    [Tooltip("How long does it take for this inhabitant to appear when called?")]
    [SerializeField] private Vector2 fetchWait = new Vector2(1f, 3f);
    public Vector2 FetchWait => fetchWait;
    [SerializeField] private Animator inhabitantAnim;
    [SerializeField] private Vector2 waitToThrowTimeRange = new Vector2(1f, 3f);
    [SerializeField] private Transform throwSpot;
    [SerializeField] private float throwSpace = 1f;
    [SerializeField] private GameObject foodPrefab; //todo how do we determine this ? 

    [Tooltip("These must all be within 0 - 100. They should be increasing, eg 35, 55, 75")]
    [SerializeField] private float[] randomnessIntervals;
    [SerializeField] private FoodScriptable [] foodOptions;
    [SerializeField] private Vector2Int foodCount = new Vector2Int(1, 4);
    
    [SerializeField] private AudioClip[] throwSounds;

    public int OverrideMultiplier = -1;

    private void OnValidate()
    {
        if (foodOptions != null && foodOptions.Length != 0)
        {
            if (randomnessIntervals == null)
            {
                randomnessIntervals = new float[foodOptions.Length];
            }
        }
    }

    /// <summary>
    /// Every time an inhabitant is enabled they will... 
    /// </summary>
    void OnEnable()
    {
        switch (inhabitantType)
        {
            //Friendly inhabitants give out food
            case InhabitantType.FRIENDLY:
                StartCoroutine(WaitToThrow());
                break;
            //Dangerous inhabitants attack 
            case InhabitantType.DANGEROUS: //TODO implement attacks - dog that hones in on nearest cat - redneck with gun etc 
               //  StartCoroutine(WaitToAttack());
                break;
        }
    }
    
    IEnumerator WaitToThrow()
    {
        //Random wait to throw from range 
        float randomWait = Random.Range(waitToThrowTimeRange.x, waitToThrowTimeRange.y);
        yield return new WaitForSeconds(randomWait);

        //Random # of food items thrown out 
        int randomThrows = Random.Range(foodCount.x, foodCount.y);
        if (randomThrows > 1)
        {
            int halfThrows = randomThrows / 2;
            float throwSpaceOffset = throwSpace / halfThrows;
            for (int i = 0; i < randomThrows; i++)
            {
                if (i > halfThrows)
                    throwSpaceOffset *= -1;
                ThrowItem(throwSpaceOffset * i);
            }
        }
        else
        {
            ThrowItem(0f);
        }

        yield return new WaitForEndOfFrame();
        
        //Wait until return to idle anim state 
        yield return new WaitUntil(() => inhabitantAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
        
        yield return new WaitForSeconds(0.25f);
        //disable after we are back to idle and reset multiplier 
        gameObject.SetActive(false);
        OverrideMultiplier = -1;
    }

    /// <summary>
    /// Actual throw method 
    /// </summary>
    void ThrowItem( float offset)
    {
        //Animate inhabitant and instantiate food item from prefab 
        inhabitantAnim.SetTrigger("throw");
        GameObject foodClone = Instantiate(foodPrefab);
        foodClone.transform.position = throwSpot.position + new Vector3(offset, 0, 0);
        FoodItem foodItem = foodClone.GetComponent<FoodItem>();
        
        //Assign food type from random table
        if (foodOptions.Length > 1)
        {
            float randomFood = Random.Range(0f, 100f);
            for (int i = 0; i < randomnessIntervals.Length; i++)
            {
                //Check if the chance fell below the interval 
                if (randomFood < randomnessIntervals[i])
                {
                    foodItem.AssignFoodData(foodOptions[i]); //Now assign it 
                    break;
                }
            }
        }
        //Only the one option 
        else
        {
            foodItem.AssignFoodData(foodOptions[0]); //Now assign it 
        }
        
        //Set food item score and play sound 
        if(OverrideMultiplier > 0)
            foodItem.SetScore(OverrideMultiplier);
        PlayRandomSound(throwSounds, 1f);
    }
}
