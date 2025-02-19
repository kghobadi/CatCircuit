using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    [SerializeField] private bool isDead;

    [SerializeField] private string healthAnimParam = "Health";
    [SerializeField] private int fullHP = 3;
    [SerializeField] private int healthAmt = 3;
    [SerializeField] private int lives = 9;
    [SerializeField] private TMP_Text livesText;

    [SerializeField] private bool healOnPurr;
    [SerializeField] private float healRange = 5f;

    private void Awake()
    {
        catController = FindObjectOfType<CatController>();
    }

    private void Start()
    {
        UpdateHealth(fullHP);
        
        catController.OnCatAction.AddListener(OnCatAction);
    }

    private void OnDestroy()
    {
        catController.OnCatAction.RemoveListener(OnCatAction);
    }

    void GetDistanceFromPlayer()
    {
        distFromPlayer = Vector3.Distance(transform.position, catController.transform.position);
    }
    
    /// <summary>
    /// How to respond to this provocation??? or altercation? or friendly invitation? 
    /// </summary>
    /// <param name="action"></param>
    void OnCatAction(CatController.CatActions action)
    {
        switch (action)
        {
            case CatController.CatActions.PURR:
                HealFromPurr();
                break;
        }
    }

    /// <summary>
    /// Updates health UI display to this amt. 
    /// </summary>
    /// <param name="amt"></param>
    void UpdateHealth(int amt)
    {
        healthAmt = amt;
        SetHealthUIStates(healthAnimParam, healthAmt);
        ShowHealthUI();
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
        
        if (healthAmt <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Called when I die from an attack or? 
    /// </summary>
    void Die()
    {
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

    #endregion
   
}
