using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the display of health and death logic. 
/// </summary>
public class HealthUI : MonoBehaviour
{
    private CatController catController;
    private float distFromPlayer;
    [SerializeField] private CanvasGroup[] uiGroups;
    [SerializeField] private float showOnHitLength = 2f;
    [SerializeField] private Animator[] healthUiAnims;
    [SerializeField] private Animator charAnimator;
    [SerializeField] private CatController linkedCat;
    [SerializeField] private bool isDead;
    public bool IsDead => isDead;

    [SerializeField] private string healthAnimParam = "Health";
    [SerializeField] private int fullHP = 3;
    [SerializeField] private int healthAmt = 3;
    [SerializeField] private int lives = 9;
    [SerializeField] private TMP_Text livesText;
    
    [SerializeField] private bool healOnPurr;
    [SerializeField] private FoodScriptable deathFood;
    [SerializeField] private float healRange = 5f;
    
    [Header("Points Anims")]
    [SerializeField] private float showOnPtsLength = 0.5f;
    [SerializeField] private CanvasFader pointsFader;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private Image plugImg;

    [SerializeField] private CanvasFader pointsFaderR;
    [SerializeField] private TMP_Text pointsTextR;
    [SerializeField] private Image plugImgR;

    private void Start()
    {
        UpdateHealth(fullHP);

        for (int i = 0; i < GameManager.Instance.AllCats.Length; i++) 
        {
            GameManager.Instance.AllCats[i].OnCatAction.AddListener(OnCatAction);
        }

        //set color to player color 
        livesText.color = linkedCat.PlayerColor;
        pointsText.color = linkedCat.PlayerColor;
        plugImg.color = linkedCat.PlayerColor;
        pointsTextR.color = linkedCat.PlayerColor;
        plugImgR.color = linkedCat.PlayerColor;
    }

    void GetDistanceFromPlayer()
    {
        distFromPlayer = Vector3.Distance(transform.position, catController.transform.position);
    }

    /// <summary>
    /// How to respond to this provocation??? or altercation? or friendly invitation? 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cat"></param>
    void OnCatAction(CatController.CatActions action, CatController cat)
    {
        catController = cat;
        if (cat != linkedCat)
        {
            switch (action)
            {
                case CatController.CatActions.PURR:
                    HealFromPurr();
                    break;
            }
        }
    }

    /// <summary>
    /// Updates health UI display to this amt. 
    /// </summary>
    /// <param name="amt"></param>
    public void UpdateHealth(int amt)
    {
        healthAmt = amt;
        SetHealthUIStates(healthAnimParam, healthAmt);
        ShowHealthUI();
        if (amt == 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Called by opponents attack animation objects. 
    /// </summary>
    /// <param name="dmg"></param>
    public void Attacked(int dmg)
    {
        if (isDead)
        {
            return;
        }

        if (healthAmt > 0)
        {
            UpdateHealth(healthAmt - dmg); 
        }
    }

    /// <summary>
    /// Called when I die from an attack or? 
    /// </summary>
    public void Die()
    {
        if (isDead)
            return;
        
        //anim set trigger die
        lives--;
        livesText.text = "X" + lives.ToString();
        charAnimator.SetTrigger("die");
        charAnimator.SetBool("dead", true);
        isDead = true;
        if (lives <= 0)
        {
            PermanentDeath();
        }

        LeanTween.delayedCall(0.1f, () =>
        {
            SubtractAndSpawnDeathFood();
        });
        
        //update cat controller link
        if (linkedCat)
        {
            linkedCat.OverrideSetLives(lives);
        }
    }

    /// <summary>
    /// Spawn food drop and subtract 1/9 of food score 
    /// </summary>
    void SubtractAndSpawnDeathFood()
    {
        GameObject foodDrop =
            Instantiate(GameManager.Instance.genericFoodPrefab, transform.position, Quaternion.identity);
        FoodItem food = foodDrop.GetComponent<FoodItem>();
        food.AssignFoodData(deathFood);
        int scoreToSubtract = linkedCat.PlayerScore / 9;
        linkedCat.LoseFood(scoreToSubtract);
        food.SetScoreDeath(scoreToSubtract);
    }

    /// <summary>
    /// Called in response to friendly purrs. 
    /// </summary>
    void HealFromPurr()
    {
        if (isDead || healthAmt >= fullHP)
        {
            return;
        }
        
        GetDistanceFromPlayer();
        if (distFromPlayer > healRange)
        {
            return;
        }
        
        UpdateHealth(healthAmt + catController.PurrHealStrength); 
    }

    private void Update()
    {
        //Did player return to idle sit? 
        // if (IsDead && charAnimator.GetCurrentAnimatorStateInfo(0).IsName("CatIdleSitting"))
        // {
        //     //Ensure resurrect 
        //     Resurrect();
        // }
    }

    /// <summary>
    /// Gets called by looping death anim. 
    /// </summary>
    public void Resurrect()
    {
        if (lives > 0)
        {
            charAnimator.SetBool("dead", false);
            charAnimator.SetTrigger("revive");
            isDead = false; 
            //anim set trigger resurrect 
            UpdateHealth(fullHP);
        }
    }

    /// <summary>
    /// Called when there are no lives left. 
    /// </summary>
    void PermanentDeath()
    {
        
        Debug.Log("You died!");
    }

    #region UI Calls

    /// <summary>
    /// Activates or deactivates a UI Canvas group by setting its alpha value. 
    /// </summary>
    /// <param name="state"></param>
    public void SetHealthObjectsActive(bool state)
    {
        for (int i = 0; i < uiGroups.Length; i++)
        {
            if (state)
            {
                uiGroups[i].alpha = 1f;
            }
            else
            {
                uiGroups[i].alpha = 0f;
            }
        }
    }
    
    /// <summary>
    /// Animates health UI objects using a float for their state in blend tree. 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="state"></param>
    void SetHealthUIStates(string stateName, int state)
    {
        for (int i = 0; i < healthUiAnims.Length; i++)
        { 
            healthUiAnims[i].SetFloat(stateName, state);
        }
    }

    /// <summary>
    /// Calls variable coroutine that shows health UI for a time. 
    /// </summary>
    void ShowHealthUI()
    {
        if (showHealthForTime != null)
        {
            StopCoroutine(showHealthForTime);
        }

        showHealthForTime = ShowHealthUIForTime();
        StartCoroutine(showHealthForTime);
    }

    private IEnumerator showHealthForTime;
    IEnumerator ShowHealthUIForTime()
    {
        SetHealthObjectsActive(true);

        yield return new WaitForSeconds(showOnHitLength);
        
        SetHealthObjectsActive(false);
    }
    
    /// <summary>
    /// Calls variable coroutine that shows Points UI for a time. 
    /// </summary>
    public void ShowPointsAnim(float amt)
    {
        if (linkedCat.IsFlipped)
        {
            pointsTextR.text = amt.ToString();
        }
        else
        {
            pointsText.text = amt.ToString();
        }
        if (showPointsForTime != null)
        {
            if(pointsFader.IsShowing)
                pointsFader.SetInstantAlpha(0);
            if(pointsFaderR.IsShowing)
                pointsFaderR.SetInstantAlpha(0);
            StopCoroutine(showPointsForTime);
        }

        showPointsForTime = ShowPointsUIForTime();
        StartCoroutine(showPointsForTime);
    }

    private IEnumerator showPointsForTime;
    IEnumerator ShowPointsUIForTime()
    {
        CanvasFader fader = pointsFader;
        if (linkedCat.IsFlipped)
        {
            fader = pointsFaderR;
        }
        fader.FadeIn();
        yield return new WaitForSeconds(showOnPtsLength);
        
        fader.FadeOut();
    }
    #endregion
}
