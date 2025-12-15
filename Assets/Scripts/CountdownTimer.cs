using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
/// <summary>
/// Handles the countdown timer behavior in the UI.
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text[] timeText;
    [SerializeField]
    private bool timing;
    private bool hasFinished;
    [SerializeField]
    private int countdownTime;

    private int quarterInterval => countdownTime / 4;
    private int currentQuarter = 0;
    private float currentTime;
    [SerializeField] private AudioSource timerSource;
    public AudioSource TimerSource => timerSource;
    [SerializeField]
    private bool useMinutesSeconds;
    [SerializeField]
    [Tooltip("Replaces the 0 in the countdown")]
    private string zeroString = "GO!";

    [SerializeField]
    string emptyText = string.Empty;

    public bool IsTiming => timing;
    public bool HasFinished => hasFinished;

    public Action OnTimerFinished;

    public float TimeLeft => currentTime;
    private bool paused;

    void Update()
    {
        if (timing)
        {
            if (paused)
            {
                return;
            }
            currentTime -= Time.deltaTime;
            CheckQuarters();
            
            //set time to string value 
            string timeVal = GetTimeInMinutesSeconds(currentTime);
            if (!useMinutesSeconds)
            {
                int num = (int)currentTime;
                if(num == 0)
                {
                    timeVal = zeroString;   
                }
                else
                {
                    timeVal = num.ToString();
                }
            }
            SetTexts(timeVal);

            if (currentTime < 0)
            {
                FinishTimer();
            }
        }
    }

    /// <summary>
    /// Checks quarters to send out events from GM. 
    /// </summary>
    void CheckQuarters()
    {
        float dif = countdownTime - currentTime;
        if (dif > currentQuarter * quarterInterval)
        {
            GameManager.Instance.OnQuarterEvent?.Invoke(currentQuarter);
            currentQuarter++;
        }
    }

    void FinishTimer()
    {
        if(timerSource)
            timerSource.Play();
        timing = false;
        hasFinished = true;
        OnTimerFinished?.Invoke();
    }

    public void SetPause(bool pause)
    {
        paused = pause;
    }

    /// <summary>
    /// Returns time in minutes : seconds format 
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public string GetTimeInMinutesSeconds(float time)
    {
        int roundedSeconds = Mathf.RoundToInt(time);
        string minutesSeconds = Mathf.Floor(roundedSeconds / 60).ToString("0") + ":" + (roundedSeconds % 60).ToString("00");
        return minutesSeconds;
    }

    public void SetCountdown(int count)
    {
        countdownTime = count;
        currentTime = count;
        currentQuarter = 1;
        SetTexts(count.ToString());
        ToggleTextsEnabled(true);
        timing = true;
        hasFinished = false;
        paused = false;
    }

    /// <summary>
    /// Sets texts to message 
    /// </summary>
    /// <param name="msg"></param>
    public void SetTexts(string msg)
    {
        for (int i = 0; i < timeText.Length; i++)
        {
            timeText[i].text = msg;
        }
    }

    public void ToggleTextsEnabled(bool state)
    {
        for (int i = 0; i < timeText.Length; i++)
        {
            timeText[i].enabled = state;
        }
    }

    public void DisableTimer()
    {
        SetTexts(emptyText);
        if(timerSource)
            timerSource.Stop();
        hasFinished = false;
    }
}
