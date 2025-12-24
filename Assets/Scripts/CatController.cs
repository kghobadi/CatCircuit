using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum PlayerType
{
    WASD = 0,
    ARROWS = 1,
    CONTROLLER = 2, //Todo revisit with Rewired at some point - this will need to be an arcade machine 
}

public class CatController : MonoBehaviour
{
    // The Rewired player id of this character
    [SerializeField]
    private int playerId = 0;
    private Player player; // The Rewired Player
    public int PlayerID => playerId; // check for player id 
    [SerializeField]
    private PlayerInputActionScriptable myPlayerInputActions;

    private HealthUI healthUI;
    public HealthUI HealthUI => healthUI;
    [SerializeField] private Color playerColor;
    public Color PlayerColor => playerColor;

    [SerializeField] private CatStates catState;
    private enum CatStates
    {
        IDLE = 0,
        MOVE = 1,
    }

    private SpriteRenderer spriteRenderer;
    public bool IsFlipped => spriteRenderer.flipX;
    private Rigidbody2D catBody;
    private Animator catAnimator;
    [SerializeField] private float moveSpeed;
    private float horizontalMove;
    private float verticalMove;
    private Vector2 moveForce;
    [SerializeField] private float catInteractDistance = 0.5f;
    [SerializeField] private float hissPushForce = 50f;

    [Header("Action Audios")] [SerializeField]
    private CatAudio catAudio;
    public CatAudio CatAudio => catAudio;
    [SerializeField] private TMP_Text actionText;
    public UnityEvent<CatActions, CatController> OnCatAction;
    public enum CatActions
    {
        MEOW = 0,
        PURR = 1, 
        HISS = 2,
        SCRATCH =3,
    }
    
    [Header("Cat Stats")] 
    [SerializeField] private int catLives = 9;
    [SerializeField] private TMP_Text catLivesTxt;

    [SerializeField] private int currentScore = 000;
    [SerializeField] private TMP_Text foodScoreText;
    [SerializeField] private CanvasFader pointAddFader;
    [SerializeField] private TMP_Text pointsAddedText;
    [SerializeField] private Image plusImg;
    [SerializeField] private Sprite plusSprite;
    [SerializeField] private Sprite deathSprite;
    private Vector2 origPointsAddPos;
    [SerializeField] private Transform catConsumePos;
    public Transform ConsumePos => catConsumePos;
    public int PlayerScore => currentScore;

    [SerializeField] private int purrHealStrength = 1;
    public int PurrHealStrength => purrHealStrength;

    [Header("Scratch Effect")]
    [SerializeField] private Transform scratchSpawnLeft;
    [SerializeField] private Transform scratchSpawnRight;
    [SerializeField] private GameObject scratchPrefabL;
    [SerializeField] private GameObject scratchPrefabR;

    private void Awake()
    {
        // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
        player = ReInput.players.GetPlayer(playerId);
        healthUI = GetComponent<HealthUI>();
        origPointsAddPos = pointAddFader.RectTransform.anchoredPosition; //todo fix null ref on restart?
        spriteRenderer = GetComponent<SpriteRenderer>();
        catBody = GetComponent<Rigidbody2D>();
        catAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        catState = CatStates.IDLE;
        actionText.enabled = false;
        //reset score and lives 
        foodScoreText.text = "000";
        catLivesTxt.text = "x9";
        
        //Add cat listeners 
        for (int i = 0; i < GameManager.Instance.AllCats.Length; i++) 
        {
            GameManager.Instance.AllCats[i].OnCatAction.AddListener(OnCatActionInvoked);
        }
    }
    
    private void OnDestroy()
    {
        OnCatAction.RemoveAllListeners();
    }

    void Update()
    {
        //Use rewired if we can 
        if (player != null)
        {
            //get move inputs
            horizontalMove = player.GetAxis("MoveHorizontal"); // get input by name or action id
            verticalMove = player.GetAxis("MoveVertical");

            //No inputs when dead 
            if (healthUI.IsDead)
                return;
             
            if (player.GetButtonDown("Meow") && !catAudio.myAudioSource.isPlaying)
            {
                Meow();
            }
            //Must be Idle in order to purr (not moving).
            if (player.GetButtonDown("Purr") && catState == CatStates.IDLE && !catAudio.myAudioSource.isPlaying)
            {
                Purr();
            }
            if (player.GetButtonDown("Hiss") )
            {
                Hiss();
            }
            if (player.GetButtonDown("Scratch"))
            {
                Scratch();
            }

            //Input timing lets us activate AI when there hasn't been any inputs 
            if ( useAI && GameManager.Instance && !GameManager.Instance.IsGameOver)
            {
                if (player.GetAnyButton())
                {
                    inputTimer = 0;
                    AIactive = false;
                }
                else
                {
                    inputTimer += Time.deltaTime;
                    //Start up AI by thinking
                    if (inputTimer > timeToActivateAI)
                    {
                        if (!AIactive)
                        {
                            AIactive = true;
                            SwitchState(CatAiStates.Thinking);
                        }
                    }
                }
            }
        }
    }
    
    #region Movement
    private void FixedUpdate()
    {
        //TODO cats shouldn't move in death - but be moved like a respawning Pacman
        if (!AIactive)
        {
            moveForce = new Vector2(moveSpeed * horizontalMove, moveSpeed * verticalMove);
            catBody.AddForce(moveForce, ForceMode2D.Impulse);
            CheckAnimationState(moveForce);
        }
        else
        {
            StateMachine();
            CheckAnimationState(catBody.velocity);
        }

        CheckFlipState();
    }

    void CheckFlipState()
    {
        if (moveForce.x > 0)
        {
            spriteRenderer.flipX = true;
            catConsumePos.localPosition = new Vector3(catConsumePos.localPosition.x * -1, catConsumePos.localPosition.y,
                catConsumePos.localPosition.z);
        }
        else if(moveForce.x < 0)
        {
            spriteRenderer.flipX = false;
            catConsumePos.localPosition = new Vector3(catConsumePos.localPosition.x * -1, catConsumePos.localPosition.y,
                catConsumePos.localPosition.z);
        }
    }

    void CheckAnimationState(Vector2 force)
    {
        if (force.magnitude > 0)
        {
            catState = CatStates.MOVE;
        }
        else if(force.magnitude == 0 && catBody.velocity.magnitude < 0.1f)
        {
            catState = CatStates.IDLE;
        }
        SetAnimator(catState);
    }

    void SetAnimator(CatStates state)
    {
        catAnimator.SetInteger("moveState", (int)state);
    }

    #endregion

    #region Actions
    void Meow()
    {
        catAudio.RandomMeow();
        OnCatAction.Invoke(CatActions.MEOW, this);
        SetActionText("MEOW!");
    }

    void Purr()
    {
        catAudio.RandomPurr();
        OnCatAction.Invoke(CatActions.PURR, this);
        //would be cool to use a slower version of the sitting animation!
        SetActionText("PURR!");
    }

    void Hiss()
    {
        catAudio.RandomHiss();
        OnCatAction.Invoke(CatActions.HISS, this);
        SetActionText("HISS!");
    }

    void Scratch()
    {
        catAudio.RandomScratch();
        OnCatAction.Invoke(CatActions.SCRATCH, this);
        
        catAnimator.SetTrigger("scratch");
        SetActionText("SCRATCH!");
    }

    /// <summary>
    /// Called by animation flag. 
    /// </summary>
    public void SpawnScratchFx()
    {
        Vector3 spawnPoint = scratchSpawnLeft.position;
        GameObject spawnPrefab = scratchPrefabL;
        if (spriteRenderer.flipX)
        {
            spawnPoint = scratchSpawnRight.position;
            spawnPrefab = scratchPrefabR;
        }
        
        //Spawn scratch at position and set creator 
        GameObject scratch = Instantiate(spawnPrefab, spawnPoint, transform.rotation);
        ScratchFx scratchFx = scratch.GetComponent<ScratchFx>();
        scratchFx.creator = this;
    }

    void SetActionText(string message)
    {
        actionText.text = message;
        actionText.enabled = true;

        if (waitToDisableActionText != null)
        {
            StopCoroutine(waitToDisableActionText);
        }
        waitToDisableActionText = WaitToDisableActionText();
        StartCoroutine(waitToDisableActionText);
    }

    private IEnumerator waitToDisableActionText;
    IEnumerator WaitToDisableActionText()
    {
        yield return new WaitForSeconds(0.1f);
        
        while (catAudio.myAudioSource.isPlaying)
        {
            yield return null;
        }

        actionText.enabled = false;
    }

    #endregion
    
    #region Outcomes

    /// <summary>
    /// How to respond to this provocation??? or altercation? or friendly invitation? 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cat"></param>
    void OnCatActionInvoked(CatActions action, CatController cat)
    {
        //Other cat did something!!
        if (cat != this)
        {
            //get dist from cat 
            float distFromCat = Vector3.Distance(cat.transform.position, transform.position);
            if (distFromCat <= catInteractDistance)
            {
                switch (action)
                {
                    case CatActions.PURR:
                        //HealFromPurr(); any anim to heal? 
                        break;
                    case CatActions.HISS:
                        PushFromHiss(cat);
                        break;
                }
            }
        }
    }

    void PushFromHiss(CatController enemyCat)
    {
        //push me away!
        Vector3 dir = transform.position - enemyCat.transform.position;
        catBody.AddForce(hissPushForce * dir,  ForceMode2D.Impulse);
    }
    
    /// <summary>
    /// Can be called by linked Health UI. 
    /// </summary>
    /// <param name="lives"></param>
    public void OverrideSetLives(int lives)
    {
        catLives = lives;
        UpdateLivesText();
    }
    
    /// <summary>
    /// Called by dangerous houses.  
    /// </summary>
    public void LoseLife()
    {
        catLives--;
        UpdateLivesText();
    }

    void UpdateLivesText()
    {
        catLivesTxt.text = "x" + catLives.ToString();
        //Checks if dead 
        if (catLives < 0)
        {
            //Cat dies 
            Debug.Log("Player " + myPlayerInputActions.name + " has died");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gain food score. 
    /// </summary>
    /// <param name="amt"></param>
    public void GainFood(int amt)
    {
        currentScore += amt; //TODO figure out how to ensure we see all the latest food pt amt updates even if anim is active 
        pointsAddedText.text = amt.ToString();
        plusImg.sprite = plusSprite;
        plusImg.color = pointsAddedText.color;
        DoPointsAnim();
        healthUI.ShowPointsAnim(amt);
    }
    
    /// <summary>
    /// Lose food score. (on death)
    /// </summary>
    /// <param name="amt"></param>
    public void LoseFood(int amt)
    {
        currentScore -= amt; //TODO figure out how to ensure we see all the latest food pt amt updates even if anim is active 
        pointsAddedText.text = amt.ToString();
        plusImg.sprite = deathSprite;
        plusImg.color = Color.white;
        DoPointsAnim();
    }

    private bool animScore;
    /// <summary>
    /// Animates points text  TODO this could be moved to a UI component. 
    /// </summary>
    void DoPointsAnim()
    {
        if (animScore)
        {
            LeanTween.cancel(pointAddFader.gameObject);
            foodScoreText.text = currentScore.ToString();
        }

        animScore = true;
        pointAddFader.RectTransform.anchoredPosition = origPointsAddPos;
        pointAddFader.FadeIn((() =>
        {
            LeanTween.delayedCall(0.1f, () =>
            {
                LeanTween.moveY(pointAddFader.RectTransform, 0f, 0.2f).setOnComplete(() =>
                {
                    //Update score and fade out
                    foodScoreText.text = currentScore.ToString();
                    pointAddFader.FadeOut();
                    animScore = false;
                });
            });
        }));
    }

    /// <summary>
    /// Pass in transform location.
    /// </summary>
    /// <param name="place"></param>
    public void TeleportCat(Transform place) => TeleportCat(place.position);

    /// <summary>
    /// Teleports the cat. 
    /// </summary>
    /// <param name="position"></param>
    public void TeleportCat(Vector3 position)
    {
        catBody.velocity = Vector3.zero;
        transform.position = position;
    }

    public void TeleportCatWithWait(float wait,Transform place) => TeleportCatWithWait(wait, place.position);
    public void TeleportCatWithWait(float wait, Vector3 position)
    {
        StartCoroutine(TeleportWithWait(wait, position));
    }

    [Tooltip("IS this cat teleporting?")]
    public bool teleporting;
    IEnumerator TeleportWithWait(float wait, Vector3 pos)
    {
        //disable physics and visuals 
        catBody.isKinematic = true;
        spriteRenderer.enabled = false;
        teleporting = true;

        yield return new WaitForSeconds(wait);
        
        TeleportCat(pos);
        //enable physics and visuals 
        catBody.isKinematic = false;
        spriteRenderer.enabled = true;

        yield return new WaitForSeconds(0.25f);
        teleporting = false;
    }
    #endregion

    #region Cat-AI

    [Header("Cat AI")] 
    [SerializeField] private bool useAI;
    [SerializeField] private float timeToActivateAI = 5f;
    [SerializeField] private bool AIactive;
    private float inputTimer;
    [SerializeField] private House[] designatedTerritory;
    [SerializeField] private bool[] hasFood; // cat remembers if a house had food 
    private int currentHouse;
    [SerializeField] private CatAiStates currentAiState;
    private enum CatAiStates
    {
        Thinking = 0,
        Moving = 1,
        Fighting = 2, 
    }

    private Vector2 stateTimeTotal = new Vector2(1f, 3f);
    private float stateTimer;

    private Vector3 autoDir;
    private Transform dest;

    [SerializeField] private float foodCheckDistance;
    private bool hasInteracted;
    
    private CatController targetCat; // for fighting 
    [SerializeField] private float fightDist;
    [SerializeField] private float returnDist;
    [SerializeField]
    private Transform origDefendPos; // tracks pos where the fighting started - so I can return 
    private float distFromCat;
    private float distFromHouse;
    
    void StateMachine()
    {
        stateTimer -= Time.deltaTime;
        
        switch (currentAiState)
        {
            //Think about what to do next and Idle 
            case CatAiStates.Thinking:
                horizontalMove = 0;
                verticalMove = 0;
                if (stateTimer < 0)
                {
                    //find nearest cat 
                    CatController cat = GameManager.Instance.GetNearestCatToPoint(transform.position, this);
                    distFromCat = Vector2.Distance(cat.transform.position, transform.position);
                    distFromHouse = Vector2.Distance(designatedTerritory[currentHouse].InhabPos.position, transform.position);
                    //go fight
                    if (distFromCat < fightDist)
                    {
                        targetCat = cat;
                        //Reset defend pos 
                        origDefendPos.SetParent(transform);
                        origDefendPos.localPosition = Vector3.zero;
                        origDefendPos.SetParent(null);
                        SwitchState(CatAiStates.Fighting);
                    }
                    //Near house
                    else if (distFromHouse < 0.25f)
                    {
                        //Look for nearest food 
                        if (hasInteracted)
                        {
                            FoodItem next = CheckForFoodItems();
                            if (next != null)
                            {
                                dest = next.transform;
                                SwitchState(CatAiStates.Moving);
                            }
                            else
                            {
                                CycleHouses();
                            }
                        }
                        //Near next house - Meow or Purr! 
                        else if (!hasInteracted)
                        {
                            float meowOrPurr = Random.Range(0, 100);
                            if (meowOrPurr <= 50f)
                            {
                                Meow();
                            }
                            else
                            {
                                Purr();
                            }

                            hasInteracted = true;
                            SwitchState(CatAiStates.Thinking);
                        }
                    }
                    //find next house 
                    else
                    {
                        CycleHouses();
                    }
                }
                break;
            case CatAiStates.Moving:
                //Move towards point 
                autoDir = dest.transform.position - transform.position;
                catBody.AddForce(moveSpeed * autoDir,  ForceMode2D.Impulse);
                
                //TODO will need some basic form of collision detection to navigate around obstacles 
                //Stop at point 
                float distance = Vector2.Distance(dest.transform.position, transform.position);
                if (distance < 0.1f)
                {
                    catBody.velocity = Vector2.zero;
                    //Think again 
                    SwitchState(CatAiStates.Thinking);
                }

                break;
            case CatAiStates.Fighting:
                distFromCat = Vector2.Distance(targetCat.transform.position, transform.position);
                float distFromOrigDefPos = Vector2.Distance(origDefendPos.position, transform.position);
                //stop fight
                if (distFromCat > fightDist * 1.5f)
                {
                    SwitchState(CatAiStates.Thinking);
                }
                //Keep fighting within these dists 
                else if(distFromCat < fightDist * 1.5f && distFromOrigDefPos < returnDist)
                {
                    //Move towards cat and attack
                    autoDir = targetCat.transform.position - transform.position;
                    catBody.AddForce(moveSpeed * autoDir,  ForceMode2D.Impulse);
                    //Scratch the fucker 
                    if(!CatAudio.myAudioSource.isPlaying)
                        Scratch();
                }
                //Too far from defend point, go back
                else if (distFromOrigDefPos > returnDist)
                {
                    dest = origDefendPos;
                    SwitchState(CatAiStates.Moving); // move back to defend pos 
                }
                break;
        }
    }
    
    /// <summary>
    /// Checks for nearby food and returns highest value.
    /// </summary>
    /// <returns></returns>
    FoodItem CheckForFoodItems()
    {
        // Define the center point for the overlap box (current position)
        Vector2 center = new Vector2(transform.position.x, transform.position.y);
        
        // Define the size of the box (width, height)
        Vector2 size = new Vector2(foodCheckDistance , foodCheckDistance );

        // Perform the OverlapBox check
        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f);

        int highestPoints = 0;
        FoodItem best = null;
        foreach (Collider2D collider in colliders)
        {
            // Check if the item has a FoodItem component
            FoodItem foodItem = collider.GetComponent<FoodItem>();
            if (foodItem != null)
            {
                Debug.Log("Found food item: " + foodItem.name);
                // Perform any actions related to the food item here
                if (foodItem.Points > highestPoints)
                {
                    highestPoints = foodItem.Points;
                    best = foodItem;
                }
            }
        }

        return best;
    }

    /// <summary>
    /// Move through house options
    /// </summary>
    void CycleHouses()
    {
        //Increment 
        if (currentHouse < designatedTerritory.Length - 1)
        {
            currentHouse++;
        }
        else
        {
            currentHouse = 0;
        }

        //reset interacted 
        hasInteracted = false;
        //Move to house point 
        dest = designatedTerritory[currentHouse].transform;
        SwitchState(CatAiStates.Moving);
    }

    /// <summary>
    /// Switch to AI state
    /// </summary>
    /// <param name="nextState"></param>
    void SwitchState(CatAiStates nextState)
    {
        stateTimer = Random.Range(stateTimeTotal.x, stateTimeTotal.y);
        
        currentAiState = nextState;
    }
    #endregion
}
