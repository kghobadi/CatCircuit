using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Simple wait to throw method for Inhabitants. 
/// </summary>
public class Inhabitant : AudioHandler
{
    private House myHouse;

    public House Home
    {
        get => myHouse;
        set => myHouse = value;
    }
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

    [Tooltip("The chance this inhabitant receives mail when the truck passes.")]
    [SerializeField] private float mailChance = 50f;
    public float MailChance => mailChance;

    private bool determiningResponse;
    private FriendlyResponse friendlyResponse;
    private enum FriendlyResponse
    {
        Neutral = 0,
        Happy = 1,
        Mad = 2,
    }
    
    [SerializeField] private CanvasFader multiplierUI;
    [SerializeField] private Animator faceResponse;
    [SerializeField] private TMP_Text multiText;

    public void UpdateMultiColor(Color color)
    {
        multiText.color = color;
    }
    
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

    public AttackType _AttackType => attackType;

    [SerializeField] private Vector2 aimingTimeRange = new Vector2(3f, 5f);
    [SerializeField] private float aimingTime;
    [SerializeField] private GameObject aimingTarget;
    
    [Tooltip("Barks for dogs, Reloads for shooters")]
    [SerializeField] private AudioClip[] trackSounds; // barks reloads
    [Tooltip("Bites, shots")]
    [SerializeField] private AudioClip[] attackSounds; // bites. shoots
    [SerializeField] private AudioClip[] hurtSounds;

    private int multiplier = -1;
    public int OverrideMultiplier
    {
        get => multiplier;
        set
        {
            multiplier = value;
            if(multiText != null)
                multiText.text = "X" + multiplier.ToString();
        }
    }

    private int enabledCounter = 0;

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
        //Add listeners while enabled 
        for (int i = 0; i < GameManager.Instance.AllCats.Length; i++) 
        {
            GameManager.Instance.AllCats[i].OnCatAction.AddListener(OnCatActionInvoked);
        }

        //Only do this on repeat enables 
        if (enabledCounter > 0)
        {
            switch (inhabitantType)
            {
                //Friendly inhabitants give out food
                case InhabitantType.FRIENDLY:
                    StartCoroutine(WaitToThrow());
                    break;
                //Dangerous inhabitants attack 
                case InhabitantType.DANGEROUS: 
                    //disable ui
                    if (multiplierUI)
                    {
                        multiplierUI.gameObject.SetActive(false);
                    }
                 
                    //return to spawn offset pos 
                    transform.localPosition = spawnOffset;
                    //Begin the hunt...
                    StartCoroutine(WaitToAttack());
                    break;
            }
        }
        
        enabledCounter++;
    }
    
    private void FixedUpdate()
    {
        //For friendlys
        if (determiningResponse)
        {
            //todo check for cat action? 
        }
        
        //For attackers
        if (trackingTarget)
        {
            //moves towards cat target
            if (attackType == AttackType.Chase)
            {
                GuidedMove(catTarget.transform.position);
            }
            else if (attackType == AttackType.Shoot)
            {
                aimingTime -= Time.fixedDeltaTime;
                aimingTarget.transform.position = catTarget.transform.position;
                if (aimingTime < 0)
                {
                    ShootBullet();
                }
            }
        }
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
                case CatController.CatActions.MEOW:
                    CheckSetResponse(FriendlyResponse.Happy, cat, action);
                    break;
                case CatController.CatActions.PURR:
                    CheckSetResponse(FriendlyResponse.Happy, cat, action);
                    break;
                case CatController.CatActions.HISS:
                    CheckSetResponse(FriendlyResponse.Mad, cat, action);
                    
                    if(inhabitantType == InhabitantType.DANGEROUS && attackType == AttackType.Chase) 
                        PushFromHiss(cat);
                    break;
                case CatController.CatActions.SCRATCH:
                    CheckSetResponse(FriendlyResponse.Mad, cat, action);
                    break;
            }
        }
    }

    /// <summary>
    /// Given a response do something. 
    /// </summary>
    /// <param name="response"></param>
    /// <param name="cat"></param>
    /// <param name="act"></param>
    void CheckSetResponse(FriendlyResponse response, CatController cat, CatController.CatActions act)
    {
        if (determiningResponse)
        {
            friendlyResponse = response;
            
            //Set face response
            faceResponse.SetFloat("Face",  (float)friendlyResponse);
            
            //Update alignment for this cat 
            if (response == FriendlyResponse.Happy)
            {
                myHouse.UpdateAlignment(cat.PlayerID, 1f);
            }
            else //mad 
            {
                myHouse.UpdateAlignment(cat.PlayerID, -1f);
            }
            //Update mult
            myHouse.UpdateOverrideMultiplier(cat, act);
            
            determiningResponse = false;
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
        //All friendlies besides mailman 
        if (inhabitantType == InhabitantType.FRIENDLY)
        {
            determiningResponse = true;
            multiplierUI.FadeIn();
            faceResponse.SetFloat("Face", 0f);
        }
        
        //Random wait to throw from range 
        float randomWait = Random.Range(waitToThrowTimeRange.x, waitToThrowTimeRange.y);

        yield return new WaitForSeconds(randomWait);
        
        //Are they mad from a bad cat? 
        if (inhabitantType == InhabitantType.FRIENDLY
        && friendlyResponse == FriendlyResponse.Mad)
        {
            yield return new WaitForSeconds(0.5f);
            
            //disable after we are back to idle and reset multiplier 
            gameObject.SetActive(false);
            OverrideMultiplier = -1;
            yield break;
        }

        //Otherwise, proceed to throw Random # of food items out 
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
    void ThrowItem(float offset)
    {
        //Animate inhabitant and instantiate food item from prefab 
        inhabitantAnim.SetTrigger("throw");
        GameObject foodClone = Instantiate(foodPrefab);
        foodClone.transform.position = throwSpot.position + new Vector3(offset, -offset / 4, 0);
        FoodItem foodItem = foodClone.GetComponent<FoodItem>();
        
        AssignFoodData(foodItem);
        
        PlayRandomSound(throwSounds, 1f);
    }

    /// <summary>
    /// Assigns a food data to a food item according to my lists. 
    /// </summary>
    /// <param name="item"></param>
    public void AssignFoodData(FoodItem item)
    {
        //Assign food type from random table
        if (foodOptions.Length > 1)
        {
            float randomFood = Random.Range(0f, 100f);
            for (int i = 0; i < randomnessIntervals.Length; i++)
            {
                //Check if the chance fell below the interval 
                if (randomFood < randomnessIntervals[i])
                {
                    item.AssignFoodData(foodOptions[i]); //Now assign it 
                    break;
                }
            }
        }
        //Only the one option 
        else
        {
            item.AssignFoodData(foodOptions[0]); //Now assign it 
        }
        
        //Set food item score and play sound 
        if(OverrideMultiplier > 0)
            item.SetScore(OverrideMultiplier);
    }

    /// <summary>
    /// Mailman throws package this way. 
    /// </summary>
    /// <param name="house"></param>
    public void ThrowDelivery( House house)
    {
        //Animate inhabitant and instantiate food item from prefab 
        inhabitantAnim.SetTrigger("throw");
        GameObject foodClone = Instantiate(foodPrefab);
        foodClone.transform.position = throwSpot.position ;
        //This is a package.Its insides will be determined by the Inhabitant of the house. 
        Package package = foodClone.GetComponent<Package>();
        package.AssignHouse(house);
        
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
        trackingTarget = true;
        //randomize aim - shoot time 
        if (attackType == AttackType.Shoot)
        {
            aimingTime = Random.Range(aimingTimeRange.x, aimingTimeRange.y);
            aimingTarget.SetActive(true);
        }
        
        inhabitantAnim.SetBool("tracking", true);

        yield return new WaitUntil(() => !trackingTarget);
        inhabitantAnim.SetBool("tracking", false);
        if(aimingTarget)
            aimingTarget.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
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
    /// Spawn bullet with given direction and speed 
    /// </summary>
    void ShootBullet()
    {     
        //trigger attack on said cat. 
        inhabitantAnim.SetTrigger("attack");

        //spawn bullet and assign dir
        GameObject bullet = Instantiate(foodPrefab, throwSpot);
        bullet.transform.SetParent(null);
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.Fire(catTarget.transform);
        
        //sound and bool 
        PlayRandomSound(attackSounds, 1f);
        trackingTarget = false;
    }

    private Vector2 autoDir;
    /// <summary>
    /// Move towards position 
    /// </summary>
    /// <param name="pos"></param>
    void GuidedMove(Vector3 pos, bool useGuidance = true)
    {
        autoDir = (Vector2)pos - (Vector2)transform.position;
        //If we will collide with something, redirect us 
        if (useGuidance)
            CollisionCheck(autoDir);
        body.AddForce(moveSpeed * autoDir, ForceMode2D.Force);
    }

    [Tooltip("What tags should the AI avoid?")]
    [SerializeField] private string[] collisionTags;

    [SerializeField] private LayerMask colMask;
    [Tooltip("How big a box should I use to check with?")]
    [SerializeField] private Vector2 colBoxSize = new Vector2(0.25f, 0.25f);
    [Tooltip("Angle threshold to determine direction adjustment")]
    [SerializeField] private float angleThreshold = 30f; // Angle threshold to determine direction adjustment
    [SerializeField] private float colCheckDist = 0.1f;
    [SerializeField] private float steeringForce = 5f;

    /// <summary>
    /// Are we colliding with that dir?
    /// Then steer around it using perpendicular force * steering 
    /// </summary>
    /// <returns></returns>
    bool CollisionCheck(Vector2 dir)
    {
        // Cast a ray in a direction
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, colCheckDist, colMask);

        bool willCollide = false;
        // Define the center point for the box cast (current position)
        // Vector2 origin = new Vector2(transform.position.x, transform.position.y) +
        //                  (dir.normalized * (boxCollider2D.size.x / 2 + colBoxSize.x));

        // Perform the BoxCastAll
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, colBoxSize, 0f, dir, colCheckDist, colMask);

        // If it hits something other than this...
        foreach (RaycastHit2D hit in hits)
        {
            //Must not be me...
            if (hit && hit.transform != transform
                && !hit.collider.isTrigger) //Must not be trigger collider 
            {
                //Check if it's something we should avoid 
                foreach (var tag in collisionTags)
                {
                    if (hit.transform.gameObject.CompareTag(tag))
                    {
                        // Get the normal of the hit object
                        Vector2 hitNormal = hit.normal;

                        // Calculate steering direction (perpendicular to the hitNormal)
                        Vector2 steeringDirection = Vector2.Perpendicular(hitNormal).normalized;

                        // Calculate the angle between the move direction and the hit normal
                        float angle = Vector2.Angle(dir, hitNormal);

                        // Adjust steering direction based on the angle
                        if (angle < angleThreshold)
                        {
                            // Move in the opposite direction of the steering
                            steeringDirection = -steeringDirection;
                        }

                        // Apply steering force to move around the object
                        body.AddForce(steeringDirection * steeringForce, ForceMode2D.Force);

                        willCollide = true;
                        break;
                    }
                }
            }
        }

        return willCollide;
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
