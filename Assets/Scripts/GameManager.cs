using System;
using System.Collections;
using System.Collections.Generic;
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
    public CountdownTimer mainTimer;
    public bool beginOnStart;
    public bool IsGameOver => mainTimer.TimeLeft <= 0;
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
    [SerializeField] private GameObject gameoverUi;
    [SerializeField] private TMP_Text winnerText;
    void Start()
    {
        if (beginOnStart)
        {
            BeginNewGame();
        }
    }

    public void BeginNewGame()
    {
        gameoverUi.SetActive(false);
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
    /// Would divide alignment sectors according to players. Right now there's only 2. 
    /// </summary>
    void DivideAlignmentSectors()
    {
        
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

    private void Update()
    {
        //Reload the game scene to restart 
        if (Input.GetKeyDown(KeyCode.R) && IsGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGameEnded()
    {
        //compare all player scores and set winner text 
        float highestScore = 0;
        int winningPlayerIndex = 0;
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i].PlayerScore > highestScore)
            {
                highestScore = allPlayers[i].PlayerScore;
                winningPlayerIndex = i;
            }
        }

        //set game over message 
        gameoverUi.SetActive(true);
        string winnerMessage = "Player " + (winningPlayerIndex + 1).ToString() + " Wins!";
        winnerText.text = winnerMessage;
    }

}
