using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
public enum PlayerType
{
    WASD = 0,
    ARROWS = 1,
    CONTROLLER = 2, //Todo revisit with Rewired at some point - this will need to be an arcade machine 
}

public class CatController : MonoBehaviour
{
    // The Rewired player id of this character
    public int playerId = 0;
    private Player player; // The Rewired Player
    [SerializeField]
    private PlayerInputActionScriptable myPlayerInputActions;

    [SerializeField] private Color playerColor;
    public Color PlayerColor => playerColor;

    [Tooltip("Does this cat align positive? This is a temporary measurement")]
    public bool alignPositive;

    [SerializeField] private CatStates catState;
    private enum CatStates
    {
        IDLE = 0,
        MOVE = 1,
    }

    private SpriteRenderer spriteRenderer;
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
             
            if (player.GetButtonDown("Meow") && !catAudio.myAudioSource.isPlaying)
            {
                Meow();
            }
            //Must be Idle in order to purr (not moving).
            if (player.GetButtonDown("Purr") && catState == CatStates.IDLE && !catAudio.myAudioSource.isPlaying)
            {
                Purr();
            }
            if (player.GetButtonDown("Hiss"))
            {
                Hiss();
            }
            if (player.GetButtonDown("Scratch"))
            {
                Scratch();
            }
        }
        //Revert to custom inputs 
        else
        {
            //get inputs 
            horizontalMove = Input.GetAxis(myPlayerInputActions.HorizontalInput);
            verticalMove = Input.GetAxis(myPlayerInputActions.VerticalInput);

            if (Input.GetKeyDown(myPlayerInputActions.Meow) && !catAudio.myAudioSource.isPlaying)
            {
                Meow();
            }
            //Must be Idle in order to purr (not moving).
            if (Input.GetKeyDown(myPlayerInputActions.Purr) && catState == CatStates.IDLE && !catAudio.myAudioSource.isPlaying)
            {
                Purr();
            }
            if (Input.GetKeyDown(myPlayerInputActions.Hiss))
            {
                Hiss();
            }
            if (Input.GetKeyDown(myPlayerInputActions.Scratch))
            {
                Scratch();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    
    #region Movement
    private void FixedUpdate()
    {
        moveForce = new Vector2(moveSpeed * horizontalMove, moveSpeed * verticalMove);
        catBody.AddForce(moveForce, ForceMode2D.Impulse);

        CheckAnimationState();
        CheckFlipState();
    }

    void CheckFlipState()
    {
        if (moveForce.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if(moveForce.x < 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    void CheckAnimationState()
    {
        if (moveForce.magnitude > 0)
        {
            catState = CatStates.MOVE;
        }
        else if(moveForce.magnitude == 0 && catBody.velocity.magnitude < 0.1f)
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
        currentScore += amt;
        foodScoreText.text = currentScore.ToString();
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
    #endregion
}
