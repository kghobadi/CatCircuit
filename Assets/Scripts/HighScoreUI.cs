using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Plugs in score data for High Score menu UI element.
/// </summary>
public class HighScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text indexText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;

    public void AssignIndex(int index)
    {
        //Start character at 0 when under 10 
        string txt = "";
        if (index < 10)
            txt = "0";
        indexText.text = txt + index.ToString();
    }
    
    public void SetHighScoreData(HighScoreData data)
    {
        playerNameText.text = data.playerName;
        scoreText.text = data.score.ToString();
    }

    public void SetNoScore()
    {
        playerNameText.text = "___";
        scoreText.text = "000";
    }
}
