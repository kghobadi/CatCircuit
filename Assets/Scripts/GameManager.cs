using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// Overall Game manager
/// </summary>
public class GameManager : NonInstantiatingSingleton<GameManager>
{
    protected override GameManager GetInstance()
    {
        return this;
    }
    public int [] allPlayerIds ;
    private Player[] playerInputs; // The Rewired Player
    
    public CountdownTimer mainTimer;
    public bool beginOnStart;
    public bool IsGameOver => mainTimer.TimeLeft <= 0 && gameOver;
    private bool gameOver;
    public bool mailManFavorsRight; //decided at start 
    public float totalGameTime = 180f;
    public UnityEvent <int> OnQuarterEvent;//Sends out current quarter 
    [SerializeField] private CatController[] allPlayers;
    public CatController[] AllCats => allPlayers;
    [SerializeField] private Vector2 alignmentRange = new Vector2(-10f, 10f);
    public Vector2 AlignmentRange => alignmentRange;
    [SerializeField] private House[] allHouses;

    public GameObject genericFoodPrefab;
    public House[] AllHouses => allHouses;
    
    //Restart to title timer
    [SerializeField] private float RestartTime = 30f;
    private float restartTimer;

    //For high score menu 
    [SerializeField] private HighScoreMenu highScoreMenu;
    public int currentHighScore; // set each round for highest score at end 
    public int winningPlayerIndex;
    void Start()
    {
        //Get player ids
        playerInputs = new Player[allPlayerIds.Length];
        for (int i = 0; i < allPlayerIds.Length; i++)
        {
            playerInputs[i] = ReInput.players.GetPlayer(allPlayerIds[i]);
        }
        
        //Should the game begin when start is called? 
        if (beginOnStart)
        {
            BeginNewGame();
        }
    }

    /// <summary>
    /// Starts a new game. 
    /// </summary>
    public void BeginNewGame()
    {
        // Use the current time to seed the random generator
        Random.InitState((int)DateTime.Now.Ticks);
        mainTimer.SetCountdown((int)totalGameTime);
        mainTimer.OnTimerFinished += OnGameEnded;
        RandomizeHousePools();
    }

    private void OnEnable()
    {
        float randomChance = Random.Range(0f, 100f);
        if (randomChance <= 50f)
        {
            mailManFavorsRight = false;
        }
        else
        {
            mailManFavorsRight = true;
        }
    }

    private void OnValidate()
    {
        if (allHouses == null || allHouses.Length == 0)
        {
            allHouses = FindObjectsOfType<House>();
        }
    }

    /// <summary>
    /// Randomizes resource allotments of every house. 
    /// </summary>
    void RandomizeHousePools()
    {
        for (int i = 0; i < allHouses.Length; i++)
        {
            allHouses[i].RandomizePrize();
        }
    }

    /// <summary>
    /// Returns nearest cat to a point. 
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public CatController GetNearestCatToPoint(Vector3 point)
    {
        CatController cat = null;
        float closest = Mathf.Infinity;

        for (int i = 0; i < AllCats.Length; i++)
        {
            //Is this closest?
            float dist = Vector3.Distance(AllCats[i].transform.position, point);
            if (dist < closest)
            {
                cat = AllCats[i];
                closest = dist;
            }
        }

        return cat;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        mainTimer.OnTimerFinished -= OnGameEnded;
        
        //remove all player listeners
        for (int i = 0; i < allPlayers.Length; i++)
        {
            allPlayers[i].OnCatAction.RemoveAllListeners();
        }
        //Remove all quarter event listeners 
        OnQuarterEvent.RemoveAllListeners();
    }
    
    /// <summary>
    /// Called when Countdown timer runs out... 
    /// Compare all player scores and set winner texts for game over message. 
    /// </summary>
    void OnGameEnded()
    {
        //Get highest score 
        currentHighScore = 0;
        winningPlayerIndex = 0;
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i].PlayerScore > currentHighScore)
            {
                currentHighScore = allPlayers[i].PlayerScore;
                winningPlayerIndex = i;
            }
        }

        //Set high score menu
        highScoreMenu.LoadHighScores();
        restartTimer = RestartTime;
        gameOver = true;
    }

    private void Update()
    {
        //While game over
        if (IsGameOver)
        {
            //Reload the game scene to restart? 
            for (int i = 0; i < playerInputs.Length; i++)
            {
                if ( playerInputs[i].GetButton("Restart"))
                {
                    Restart();
                }
            }

            //Return to game title screen when timer runs out with no Restart input 
            restartTimer -= Time.deltaTime;
            if (restartTimer < 0)
            {
                ReturnToTitle();
            }
        }
    }

    /// <summary>
    /// Reset restart timer when player is entering high score data. 
    /// </summary>
    public void ResetRestart()
    {
        restartTimer = RestartTime;
    }
    
    /// <summary>
    /// Restarts gameplay scene. 
    /// </summary>
    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Returns to title screen scene. 
    /// </summary>
    void ReturnToTitle()
    {
        SceneManager.LoadScene(0);
    }
}
