using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows editor based calls to fade this game object's canvas group. 
/// </summary>
public class CanvasFader : MonoBehaviour
{
    private RectTransform myRect;

    public RectTransform RectTransform
    {
        get
        {
            if (myRect == null)
            {
                myRect = GetComponent<RectTransform>();
            }

            return myRect;
        }
    }
    private CanvasGroup myCanvasGroup;
    public CanvasGroup CG => myCanvasGroup;
    [SerializeField] private bool shownAtStart;
    [SerializeField] private bool fadeOutAtStart;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float fadeInAmt = 1f;
    [SerializeField] private float fadeOutAmt = 0f;
    [SerializeField] private float delayIn =0f;
    [SerializeField] private float delayOut =0f;
    [SerializeField] private bool disableInteractivity;
    public bool IsShowing => myCanvasGroup.alpha >= fadeInAmt;
    private Action OnFadeInComplete;
    private Action OnFadeOutComplete;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        myCanvasGroup = GetComponent<CanvasGroup>();
        if (shownAtStart)
        {
            myCanvasGroup.alpha = fadeInAmt;
        }
        else
        {
            myCanvasGroup.alpha = fadeOutAmt;
            if (disableInteractivity)
            {
                CG.interactable = CG.blocksRaycasts = false; // allows interaction with rest of screen
            }
        }
    }

    private void Start()
    {
        if (fadeOutAtStart)
        {
            FadeOut();
        }
    }

    public void SetFadeDuration(float amt)
    {
        fadeDuration = amt;
    }
    
    public void SetFadeIn(float amt)
    {
        fadeInAmt = amt;
    }
    
    public void SetFadeOut(float amt)
    {
        fadeOutAmt = amt;
    }
    
    public void FadeIn(Action OnComplete = null)
    {
        OnFadeInComplete = OnComplete;
        LeanTween.cancel(gameObject);
        if (delayIn > 0)
        {
            LeanTween.delayedCall(delayIn, () => LeanTween.alphaCanvas(myCanvasGroup, fadeInAmt, fadeDuration).setOnComplete(FadeInComplete));
        }
        else
        {
            LeanTween.alphaCanvas(myCanvasGroup, fadeInAmt, fadeDuration).setOnComplete(FadeInComplete);
        }
    }

    void FadeInComplete()
    {
        OnFadeInComplete?.Invoke();
        if (disableInteractivity)
        {
            CG.interactable = CG.blocksRaycasts = true; // allows interaction with rest of screen
        }
    }

    public void FadeOut(Action OnComplete = null)
    {
        OnFadeOutComplete = OnComplete;
        LeanTween.cancel(gameObject);
        if (delayOut > 0)
        {
            LeanTween.delayedCall(delayOut, () =>   LeanTween.alphaCanvas(myCanvasGroup, fadeOutAmt, fadeDuration).setOnComplete(FadeOutComplete));
        }
        else
        {
            LeanTween.alphaCanvas(myCanvasGroup, fadeOutAmt, fadeDuration).setOnComplete(FadeOutComplete);
        }
    }
    
    void FadeOutComplete()
    {
        OnFadeOutComplete?.Invoke();
        if (disableInteractivity)
        {
            CG.interactable = CG.blocksRaycasts = false; // disallows interaction with rest of screen
        }
    }

    public void SetInstantAlpha(float alpha)
    {
        LeanTween.cancel(gameObject);
        myCanvasGroup.alpha = alpha;
    }
}
