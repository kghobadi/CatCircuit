using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stores references to all Player UI in the bars.
/// Handles points animation. 
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pNameText;
    [SerializeField] private CatController linkedCat;
    [SerializeField] private TMP_Text catLivesTxt;
    [SerializeField] private Image healthIcon;
    public void SetLives(string lives)
    {
        catLivesTxt.text = lives;
    }
    [SerializeField] private TMP_Text foodScoreText;

    public void SetFoodScore(string score)
    {
        foodScoreText.text = score;
    }
    [SerializeField] private CanvasFader pointAddFader;
    [SerializeField] private TMP_Text pointsAddedText;
    private Vector2 origPointsAddPos;
    [SerializeField] private Image plusImg;
    [SerializeField] private Sprite plusSprite;
    [SerializeField] private Sprite deathSprite;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        //Update name 
        if (linkedCat)
        {
            string playerString = "P" + (linkedCat.PlayerID + 1).ToString();
            gameObject.name = gameObject.name.Replace("P1", playerString);
            pNameText.text = playerString;
            pNameText.color = linkedCat.PlayerColor;
            catLivesTxt.gameObject.name = "CatLivesText" + playerString;
            healthIcon.gameObject.name = healthIcon.gameObject.name.Replace("P1", playerString);
            foodScoreText.gameObject.name = "FoodScoreText" + playerString;
            pointsAddedText.gameObject.name = pointsAddedText.gameObject.name.Replace("P1", playerString);
            pointsAddedText.color = linkedCat.PlayerColor;
            plusImg.color = linkedCat.PlayerColor;
        }
    }
#endif
    
    private void Start()
    {
        //reset score and lives 
        SetFoodScore("000");
        SetLives("x9");
        origPointsAddPos = pointAddFader.RectTransform.anchoredPosition; //todo fix null ref on restart?
    }
    
    private bool animScore;
    /// <summary>
    /// Animates points text  TODO this could be moved to a UI component. 
    /// </summary>
    public void DoPointsAnim(int amt, bool positive = true)
    {
        //Update points added 
        pointsAddedText.text = amt.ToString();
        if (positive)
        {
            plusImg.sprite = plusSprite;
            plusImg.color = pointsAddedText.color;
        }
        else
        {
            plusImg.sprite = deathSprite;
            plusImg.color = Color.white;
        }
        
        if (animScore)
        {
            LeanTween.cancel(pointAddFader.gameObject);
            foodScoreText.text = linkedCat.PlayerScore.ToString();
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
                    foodScoreText.text = linkedCat.PlayerScore.ToString();
                    pointAddFader.FadeOut();
                    animScore = false;
                });
            });
        }));
    }
}
