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
    public InhabitantType InhabiType => inhabitantType;
    public enum InhabitantType
    {
        FRIENDLY = 0, // for an already friendly house - you scratch to get food
        DANGEROUS = 1, // for a dangerous house - you hiss&scratch to escape the dog
        MAILMAN = 2, // delivers mail to houses 
    }

    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    public Vector3 SpawnOffset => spawnOffset;
    
    [Header("Throw Settings")]
    [SerializeField] private Vector2Int foodAmtRange = new Vector2Int(10, 100);
    public Vector2Int FoodRange => foodAmtRange;
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

   
    private Rigidbody2D body;
    [Header("Attack Settings")] 
    [SerializeField] private Vector2 waitToAttackTimeRange = new Vector2(1f, 3f);
    [SerializeField]
    private int damageAmt;
    public CatController catTarget; //determined by who is closest at time of attack 
    [SerializeField] private float moveSpeed = 5f;
    public bool trackingTarget;
    public float catInteractDistance = 1f;
    [SerializeField] private float hissPushForce = 15f;
    [SerializeField] private AttackType attackType;
    public enum AttackType
    {
        Chase = 0, // for attack dogs
        Shoot = 1, // for a redneck shooter
    }
        
    [SerializeField] private AudioClip[] trackSounds;
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] hurtSounds;
    public int OverrideMultiplier = -1;

    private void Start()
    {
        if (inhabitantType == InhabitantType.DANGEROUS)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

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
                //Add cat listeners 
                for (int i = 0; i < GameManager.Instance.AllCats.Length; i++) 
                {
                    GameManager.Instance.AllCats[i].OnCatAction.AddListener(OnCatActionInvoked);
                }
                //return to spawn offset pos 
                transform.localPosition = spawnOffset;
                //Begin the hunt...
                StartCoroutine(WaitToAttack());
                break;
        }
    }

    private void OnDisable()
    {
        //Add cat listeners 
        if (GameManager.Instance)
        {
            for (int i = 0; i < GameManager.Instance.AllCats.Length; i++) 
            {
                GameManager.Instance.AllCats[i].OnCatAction.RemoveListener(OnCatActionInvoked);
            }
        }
    }

    #region Throw Behavior

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

    #endregion

    #region AttackBehavior

    IEnumerator WaitToAttack()
    {
        //Random wait to attack from range 
        float randomWait = Random.Range(waitToAttackTimeRange.x, waitToAttackTimeRange.y);
        yield return new WaitForSeconds(randomWait);
        
        PlayRandomSound(trackSounds, 1f);
        //get target 
        catTarget = GameManager.Instance.GetNearestCatToPoint(transform.position);
        inhabitantAnim.SetBool("tracking", true);
        trackingTarget = true;

        yield return new WaitUntil(() => !trackingTarget);
        inhabitantAnim.SetBool("tracking", false);
        
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (trackingTarget)
        {
            //moves towards cat target
            Vector3 dir = catTarget.transform.position - transform.position;
            body.AddForce(moveSpeed * dir,  ForceMode2D.Force);
            
            //TODO what to do about swinging velocity? could play with it somewhat 
            //TODO should it have legit collision with most objects?
        }
    }

    /// <summary>
    /// Attacks should be trigger based. 
    /// </summary>
    /// <param name="obj"></param>
    public void TriggerAttack(GameObject obj)
    {
        //trigger attack on said cat. 
        inhabitantAnim.SetTrigger("attack");
        //do something 
        Debug.Log("hit cat or player!");
        Animator enemyAnimator = obj.GetComponent<Animator>();
        if (enemyAnimator == null)
        {
            enemyAnimator = obj.GetComponentInChildren<Animator>();
            //try in parent
            if (enemyAnimator == null)
            {
                enemyAnimator = obj.GetComponentInParent<Animator>();
            }
        }

        if (enemyAnimator != null)
        {
            if (!enemyAnimator.GetBool("dead"))
            {
                enemyAnimator.SetTrigger("hit_scratch"); // this determines dmg 
            }
        }

        PlayRandomSound(attackSounds, 1f);
        body.velocity = Vector2.zero; //zero velocity 
        trackingTarget = false;
    }

    
    /// <summary>
    /// How to respond to this provocation??? or altercation? or friendly invitation? 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cat"></param>
    void OnCatActionInvoked(CatController.CatActions action, CatController cat)
    {
        //get dist from cat 
        float distFromCat = Vector3.Distance(cat.transform.position, transform.position);
        if (distFromCat <= catInteractDistance)
        {
            switch (action)
            {
                case CatController.CatActions.PURR:
                    //HealFromPurr(); any anim to heal? 
                    break;
                case CatController.CatActions.HISS:
                    PushFromHiss(cat);
                    break;
            }
        }
    }
    void PushFromHiss(CatController enemyCat)
    {
        //push me away!
        Vector3 dir = transform.position - enemyCat.transform.position;
        body.AddForce(hissPushForce * dir,  ForceMode2D.Impulse);
    }

    /// <summary>
    /// Calls immediate slowdown to swap up velocity when flying offscreen. 
    /// </summary>
    public void Slowdown()
    {
        body.velocity = Vector2.zero;
    }

    /// <summary>
    /// When an environmental element hurts this inhabitant - like a car crash. 
    /// </summary>
    public void DisableInhabitant()
    {
        PlayRandomSound(hurtSounds, 1f);
        gameObject.SetActive(false);
    }
    #endregion
}
