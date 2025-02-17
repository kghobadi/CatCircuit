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
    [SerializeField] private GameObject[] uiObjects;
    [SerializeField] private float showOnHitLength = 2f;
    [SerializeField] private Animator[] healthUiAnims;
    [SerializeField] private Animator charAnimator;
    [SerializeField] private bool isDead;

    [SerializeField] private string healthAnimParam = "Health";
    [SerializeField] private int fullHP = 3;
    [SerializeField] private int healthAmt = 3;
    [SerializeField] private int lives = 9;
    [SerializeField] private TMP_Text livesText;

    private void Start()
    {
        UpdateHealth(fullHP);
    }

    //TODO something wrong with how this updates -maybe from the blend tree? 
    void UpdateHealth(int amt)
    {
        healthAmt = amt;
        SetHealthUIStates(healthAnimParam, healthAmt);
        ShowHealthUI();
    }

    public void Attacked(int dmg)
    {
        if (isDead)
        {
            return;
        }
        
        UpdateHealth(healthAmt - dmg); 
        if (healthAmt <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        //anim set trigger die
        lives--;
        livesText.text = "X" + lives.ToString();
        charAnimator.SetTrigger("die");
        isDead = true;
        if (lives <= 0)
        {
            PermanentDeath();
        }
    }

    /// <summary>
    /// Gets called by looping death anim. 
    /// </summary>
    public void Resurrect()
    {
        if (lives > 0)
        {
            charAnimator.SetTrigger("revive");
            isDead = false; 
            //anim set trigger resurrect 
            UpdateHealth(fullHP);
        }
    }

    void PermanentDeath()
    {
        
        Debug.Log("You died!");
    }

    #region UI Calls

    public void SetHealthObjectsActive(bool state)
    {
        for (int i = 0; i < uiObjects.Length; i++)
        { 
            if(uiObjects[i].activeSelf != state)
                uiObjects[i].SetActive(state);
        }
    }
    
    void SetHealthUIStates(string stateName, int state)
    {
        for (int i = 0; i < healthUiAnims.Length; i++)
        { 
            healthUiAnims[i].SetFloat(stateName, state);
        }
    }

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
