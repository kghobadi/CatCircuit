using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;
using UnityEngine.SceneManagement;

public class GameManager : NonInstantiatingSingleton<GameManager>
{
    protected override GameManager GetInstance()
    {
        return this;
    }
    public CountdownTimer mainTimer;
    public bool IsGameOver => mainTimer.TimeLeft <= 0;
    public float totalGameTime = 180f;
    [SerializeField] private CatController[] allPlayers;
    public CatController[] AllCats => allPlayers;
    [SerializeField] private Vector2 alignmentRange = new Vector2(-10f, 10f);
    public Vector2 AlignmentRange => alignmentRange;
    [SerializeField] private House[] allHouses;
    public House[] AllHouses => allHouses;
    [SerializeField] private GameObject gameoverUi;
    [SerializeField] private TMP_Text winnerText;
    void Start()
    {
        gameoverUi.SetActive(false);
        mainTimer.SetCountdown((int)totalGameTime);
        mainTimer.OnTimerFinished += OnGameEnded;
        RandomizeHousePools();
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        mainTimer.OnTimerFinished -= OnGameEnded;
        
        //remove all player listeners
        for (int i = 0; i < allPlayers.Length; i++)
        {
            allPlayers[i].OnCatAction.RemoveAllListeners();
        }
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
