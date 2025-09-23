using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Action Audios")] [SerializeField]
    private CatAudio catAudio;
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
    }

    void Update()
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
        
        //Spawn scratch at position
        GameObject scratch = Instantiate(spawnPrefab, spawnPoint, transform.rotation);
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
