using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles tracking and displaying high scores. 
/// </summary>
public class HighScoreMenu : MonoBehaviour
{
    private Dictionary<string, int> HighScores = new Dictionary<string, int>();
    [Tooltip("Added to standard save names in case there are multiple High score menus.")]
    [SerializeField] private string saveName = "CTC_";
    //TODO set up UI elements
    //Need one ref to ScrollRectView parent 
    //Need UI prefab for HighScoreElement which plugs in name and score 
    
    /// <summary>
    /// Loads high score dictionary from previously saved PlayerPrefs
    /// </summary>
    public void LoadHighScores()
    {
        //Load prefs
        string allHighScoreNames = PlayerPrefs.GetString( saveName + "HighScoreNames");
        string allHighScores = PlayerPrefs.GetString(saveName + "HighScores");
        string[] allNames = new string[1];
        string[] allScores = new string[1];
        HighScores.Clear();

        //Get names 
        if (!string.IsNullOrEmpty(allHighScoreNames))
        {
            //split names by ,
            allNames = allHighScoreNames.Split(",");
        }
        //Get scores 
        if (!string.IsNullOrEmpty(allHighScores))
        {
            //split scores by ,
            allScores = allHighScores.Split(",");
        }
        //Assume there is a score for every name and add all to dict
        for (int i = 0; i < allNames.Length; i++)
        {
            HighScores.Add(allNames[i], int.Parse(allScores[i]));
        }
        
        UpdateUI();
    }

    //TODO input should be granted to the Winning Player index only 
    void TakeInput()
    {
        //get player id 
        int playerId = GameManager.Instance.winningPlayerIndex;
        //enable their controller map for high score entry
        
        //Vertical axis - up and down for letter select 
        //Horizontal axis - left and right for character select?
        
        //Main action button (meow? X) for choose letter, auto progress to next character. 
        //Reset Restart timer for GameOver 
    }

    /// <summary>
    /// Called by Highscore entry to save the new high score, then display it. 
    /// </summary>
    /// <param name="newName"></param>
    public void SaveNewHighScore(string newName)
    {
        //get highest score 
        HighScores.Add(newName, GameManager.Instance.currentHighScore);
        //organize dict by highest score. 
        HighScores.OrderByDescending(entry => entry.Value); // Sort by score, descending
        
        //TODO now recreate one long string of data for saving to Playerpref.
        //TODO could rework it to be a JSON string if needed. 
        
        //Now update UI
        UpdateUI();
    }

    /// <summary>
    /// Displays all currently available High score data. 
    /// </summary>
    void UpdateUI()
    {
        //should generate High score items inside a scroll rect view that has a max count? 
    }
}
