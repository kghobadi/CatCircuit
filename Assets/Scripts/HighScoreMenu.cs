using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using Rewired;
using TMPro;
using UnityEngine;

/// <summary>
/// For serializing to JSON 
/// </summary>
[Serializable]
public class HighScoreData
{
    public string playerName { get; set; }
    public int score { get; set; }
}

/// <summary>
/// For storing list of High Scores. 
/// </summary>
public class HighScoreDataModel
{
    public List<HighScoreData> allHighScores { get; set; }
}

/// <summary>
/// Handles tracking, loading, displaying, and saving high scores. 
/// </summary>
public class HighScoreMenu : MonoBehaviour
{
    private HighScoreDataModel highScoreDatas;
    [Tooltip("Added to standard save names in case there are multiple High score menus.")]
    [SerializeField] private string saveName = "CTC_";
    [SerializeField] private CanvasFader gameOverFader;
    private Player player; // The Rewired Player
  
    [SerializeField] private HighScoreUI[] allHighScoreUIs;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text newHighScoreText;
    private string newHs = "New High Score!";
    private string normal = "High Scores";

    public bool isNewHighScore;

    [SerializeField] private GameObject nameInput;
    [SerializeField] private TMP_Text[] nameCharInputs;
    [Tooltip("Arrows which show current character for Input")]
    [SerializeField] private RectTransform characterIndicator;
    private int[] charIndexes;
    private int currentCharInput = 0; // for checking current input char 

    [Tooltip("How long of an Input time offset is necessary?")]
    [SerializeField] private float inputTime = 0.25f;
    private float inputTimer; 
    [SerializeField] private List<string> allAvailableCharacters = new List<string>();

    private void Start()
    {
        SetUpScoreUI();
    }

    /// <summary>
    /// Plugs in Score UI indexes
    /// </summary>
    void SetUpScoreUI()
    {
        //Assign all score indexes 
        for (int i = 0; i < allHighScoreUIs.Length; i++)
        {
            allHighScoreUIs[i].AssignIndex(i + 1);
        }

        //set up all char indexes at start 
        charIndexes = new int[nameCharInputs.Length];
        for (int i = 0; i < nameCharInputs.Length; i++)
        {
            charIndexes[i] = allAvailableCharacters.IndexOf(nameCharInputs[i].text);
        }
    }
    
    /// <summary>
    /// Loads high score dictionary from previously saved PlayerPrefs
    /// </summary>
    public void LoadHighScores()
    {
        //Load prefs
        string allHighScores = PlayerPrefs.GetString(saveName + "HighScores");
        
        //Must have string to load data 
        if (!string.IsNullOrEmpty(allHighScores))
        {
            //Get high scores
            highScoreDatas = JsonMapper.ToObject<HighScoreDataModel>(allHighScores);
        }
        //Just create blank data model 
        else
        {
            highScoreDatas = new HighScoreDataModel();
            highScoreDatas.allHighScores = new List<HighScoreData>();
        }
        
        //Check if its a new high score
        isNewHighScore = CheckNewHighScore(GameManager.Instance.currentHighScore);
        
        //set up input 
        player = ReInput.players.GetPlayer(GameManager.Instance.winningPlayerIndex);
       
        //Show winner texts 
        string winnerMessage = "Player " + (GameManager.Instance.winningPlayerIndex + 1).ToString() + " Wins!";
        winnerText.text = winnerMessage;
        winnerText.color = GameManager.Instance.AllCats[GameManager.Instance.winningPlayerIndex].PlayerColor;
        scoreText.text = GameManager.Instance.currentHighScore.ToString();
        
        //Now Update UI 
        UpdateUI();
        //fade in 
        gameOverFader.FadeIn();
    }

    /// <summary>
    /// Checks if a given score is greater than something on the High score menu. 
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    bool CheckNewHighScore(int score)
    {
        //start false Only if we have as many high scores as there are UI elements 
        bool isHighScore = highScoreDatas.allHighScores.Count < allHighScoreUIs.Length;
        
        for (int i = 0; i < highScoreDatas.allHighScores.Count; i++)
        {
            if (highScoreDatas.allHighScores[i].score < score)
            {
                isHighScore = true;
            }
        }

        return isHighScore;
    }

    /// <summary>
    /// Displays all currently available High score data. 
    /// </summary>
    void UpdateUI()
    {
        //Show new high score view
        if (isNewHighScore)
        {
            newHighScoreText.text = newHs;
            nameInput.SetActive(true);
        }
        //Show previous high scores view 
        else
        {
            newHighScoreText.text = normal;
            nameInput.SetActive(false);
        }
        
        //should generate High score items inside a scroll rect view that has a max count? 
        for (int i = 0; i < allHighScoreUIs.Length; i++)
        {
            //Enter high score data if there is any 
            if (highScoreDatas.allHighScores.Count > i)
            {
                allHighScoreUIs[i].SetHighScoreData(highScoreDatas.allHighScores[i]);
            }
            //Otherwise default to no score... 
            else
            {
                allHighScoreUIs[i].SetNoScore();
            }
        }
    }

    private void Update()
    {
        //Only take input during game over + new high score state 
        if (GameManager.Instance.IsGameOver && isNewHighScore && gameOverFader.IsShowing)
        {
            TakeInput();

            //Prevent input spamming 
            if (inputTimer > 0)
            {
                inputTimer -= Time.deltaTime;
            }
        }
        
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
        }
#endif
    }

    //For navigating the menu 
    private float horizontalMove;
    private float verticalMove;
    /// <summary>
    /// Input is taken from winning player only for high score entry. 
    /// </summary>
    void TakeInput()
    {
        //Horizontal axis - left and right for character select?
        horizontalMove = player.GetAxis("MoveHorizontal"); // get input by name or action id
        CycleInputLetter();
        
        //Vertical axis - up and down for letter select 
        verticalMove = player.GetAxis("MoveVertical");
        CycleCurrentCharacter();
   
        //Main action button (meow? X) for choose letter, auto progress to next character. 
        if (player.GetButtonDown("Meow"))
        {
            //Move to next input
            if (currentCharInput < nameCharInputs.Length - 1)
            {
                currentCharInput++;
                UpdateCurrentCharInput();
            }
            //Save on final char 
            else
            {
                SaveNewHighScore();
            }
        }
    }

    /// <summary>
    /// Cycles which Input Text the player is editing.
    /// </summary>
    private void CycleInputLetter()
    {
        //Prevent input
        if(inputTimer > 0)
            return;
        
        //Go back if you can
        if (horizontalMove < 0)
        {
            if (currentCharInput > 0)
            {
                currentCharInput--;
            }
            UpdateCurrentCharInput();
            ResetInputTimer();
        }
        //Go forward if you can 
        else if(horizontalMove > 0)
        {
            if (currentCharInput < nameCharInputs.Length - 1)
            {
                currentCharInput++;
            }
            UpdateCurrentCharInput();
            ResetInputTimer();
        }
    }

    void UpdateCurrentCharInput()
    {
        //Update indicator 
        characterIndicator.SetParent(nameCharInputs[currentCharInput].rectTransform);
        characterIndicator.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Cycles the current displayed Text character at index. 
    /// </summary>
    void CycleCurrentCharacter()
    {
        //Prevent input
        if(inputTimer > 0)
            return;
        
        if (verticalMove < 0)
        {
            //Move up avail chars
            if (charIndexes[currentCharInput] < allAvailableCharacters.Count - 1)
            {
                charIndexes[currentCharInput]++;
            }
            else
            {
                charIndexes[currentCharInput] = 0;
            }
            //Update current char text
            UpdateCurrentCharText();
            ResetInputTimer();
        }
        else if(verticalMove > 0)
        {
            //Move down avail chars
            if (charIndexes[currentCharInput] > 0)
            {
                charIndexes[currentCharInput]--;
            }
            else
            {
                charIndexes[currentCharInput] = allAvailableCharacters.Count - 1;
            }
            
            //Update current char text
            UpdateCurrentCharText();
            ResetInputTimer();
        }
    }

    /// <summary>
    /// Updates the current character text display 
    /// </summary>
    void UpdateCurrentCharText()
    {
        nameCharInputs[currentCharInput].text = allAvailableCharacters[charIndexes[currentCharInput]];
    }

    void ResetInputTimer()
    {
        //reset input time
        inputTimer = inputTime;
        //Reset Restart timer for GameOver 
        GameManager.Instance.ResetRestart();
    }

    /// <summary>
    /// Called by Highscore entry to save the new high score, then display it. 
    /// </summary>
    public void SaveNewHighScore()
    {
        //Get player name from input texts 
        string playerName = "";
        for (int i = 0; i < nameCharInputs.Length; i++)
        {
            playerName += nameCharInputs[i].text;
        }
        
        //get highest score 
        HighScoreData scoreData = new HighScoreData();
        scoreData.playerName = playerName;
        scoreData.score = GameManager.Instance.currentHighScore;
        highScoreDatas.allHighScores.Add(scoreData);
        //organize list by highest score. 
        highScoreDatas.allHighScores = highScoreDatas.allHighScores.OrderByDescending(entry => entry.score).ToList();
        
        //Recreate one long string of json data for saving to Playerpref.
        string jsonScores = JsonMapper.ToJson(highScoreDatas);
        PlayerPrefs.SetString(saveName + "HighScores", jsonScores);
        
        //disable isnew high score 
        isNewHighScore = false;
        //Now update UI
        UpdateUI();
    }
}