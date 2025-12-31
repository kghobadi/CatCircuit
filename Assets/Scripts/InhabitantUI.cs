using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles house/Inhabitant UI stuff. 
/// </summary>
public class InhabitantUI : MonoBehaviour
{
    [SerializeField] private CanvasFader multiplierUI;
    public CanvasFader Fader => multiplierUI;
    [SerializeField] private Animator faceResponse;
    public Animator FaceAnim => faceResponse;
    [SerializeField] private TMP_Text multiText;

    [SerializeField] private Image backImg;
    [SerializeField] private Image timerFill;

    private void Start()
    {
        timerFill.enabled = false;
    }

    public void UpdateMultiColor(Color color)
    {
        multiText.color = color;
    }

    public void UpdateMultiText(string mult)
    {
        if(multiText != null)
            multiText.text = "X" + mult;

        //Update background image size to match text 
        LeanTween.delayedCall(0.01f, () =>
        {
            backImg.rectTransform.sizeDelta = new Vector2(32 + multiText.rectTransform.sizeDelta.x, 32f);
        });
    }

    public void ToggleMultiplier(bool active)
    {
        if (multiplierUI)
        {
            multiplierUI.gameObject.SetActive(active);
        }
    }

    public void BeginTimerCountdown(float time, Color color)
    {
        StopAllCoroutines();
        timerFill.color = color;
        StartCoroutine(TimerCountdown(time));
    }
    
    IEnumerator TimerCountdown(float time)
    {
        timerFill.enabled = true;
        float timeStarted = Time.time;
        float timeSinceStarted = Time.time - timeStarted;
        while (timeSinceStarted < time)
        {
            timeSinceStarted = Time.time - timeStarted;
            float fillAmt = timeSinceStarted / time;
            timerFill.fillAmount = 1 - fillAmt;
            yield return new WaitForEndOfFrame();
        }

        timerFill.enabled = false;
    }
}
