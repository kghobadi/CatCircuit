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
/// Handles tracking and displaying high scores. 
/// </summary>
public class HighScoreMenu : MonoBehaviour
{
    private HighScoreDataModel highScoreDatas;
    [Tooltip("Added to standard save names in case there are multiple High score menus.")]
    [SerializeField] private string saveName = "CTC_";
    [SerializeField] private CanvasFader gameOverFader;
    private Player player; // The Rewired Player
    //TODO set up UI elements
    [SerializeField] private HighScoreUI[] allHighScoreUIs;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text newHighScoreText;
    private string newHs = "New High Score!";
    private string normal = "High Scores";

    public bool isNewHighScore;

    [SerializeField] private GameObject nameInput;
    [SerializeField] private TMP_Text[] nameCharInputs;
    private int[] charIndexes;
    private int currentCharInput = 0;
    private int charMax = 2;

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
        for (int i = 0; i < allHighScoreUIs.Length; i++)
        {
            allHighScoreUIs[i].AssignIndex(i);
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
        
        //No data 
        if(string.IsNullOrEmpty(allHighScores))
            return;
        
        //Get high scores
        highScoreDatas = JsonMapper.ToObject<HighScoreDataModel>(allHighScores);

        //Check if its a new high score
        isNewHighScore = CheckNewHighScore(GameManager.Instance.currentHighScore);
        
        //Now Update UI 
        UpdateUI();
    }

    /// <summary>
    /// Checks if a given score is greater than something on the High score menu. 
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    bool CheckNewHighScore(int score)
    {
        bool isHighScore = false;
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
        //set up input 
        player = ReInput.players.GetPlayer(GameManager.Instance.winningPlayerIndex);
        
        gameOverFader.FadeIn();
        //Show winner texts 
        string winnerMessage = "Player " + (GameManager.Instance.winningPlayerIndex + 1).ToString() + " Wins!";
        winnerText.text = winnerMessage;
        winnerText.color = GameManager.Instance.AllCats[GameManager.Instance.winningPlayerIndex].PlayerColor;
        
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
        if (GameManager.Instance.IsGameOver && isNewHighScore)
        {
            TakeInput();

            if (inputTimer > 0)
            {
                inputTimer -= Time.deltaTime;
            }
        }
    }

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
        //Reset Restart timer for GameOver 
        GameManager.Instance.ResetRestart();
    }

    /// <summary>
    /// Cycles which letter the character is inputting. 
    /// </summary>
    private void CycleInputLetter()
    {
        if(inputTimer > 0)
            return;
        
        if (horizontalMove < 0)
        {
            
        }
        else
        {
            
        }
        
        //reset input time
        inputTimer = inputTime;
    }

    void CycleCurrentCharacter()
    {
        if(inputTimer > 0)
            return;
        
        if (verticalMove < 0)
        {
            
        }
        else
        {
            
        }

        //reset input time
        inputTimer = inputTime;
    }

    /// <summary>
    /// Called by Highscore entry to save the new high score, then display it. 
    /// </summary>
    /// <param name="newName"></param>
    public void SaveNewHighScore(string newName)
    {
        //get highest score 
        HighScoreData scoreData = new HighScoreData();
        scoreData.playerName = newName;
        scoreData.score = GameManager.Instance.currentHighScore;
        highScoreDatas.allHighScores.Add(scoreData);
        //organize list by highest score. 
        highScoreDatas.allHighScores.OrderByDescending(entry => entry.score);
        
        //Recreate one long string of json data for saving to Playerpref.
        string jsonScores = JsonMapper.ToJson(highScoreDatas);
        PlayerPrefs.SetString(saveName + "HighScores", jsonScores);
        //Now update UI
        UpdateUI();
    }
}
