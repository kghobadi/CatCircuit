using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// This script handles fades of any and all UI + sprites. 
/// can fadeIn at start or fadeOut when leaving
/// </summary>
public class FadeUiRevamped : MonoBehaviour
{
    //This is automatically determined by the script at start.
    public enum UIType
    {
        IMAGE, RAWIMAGE, TEXT, TMPTEXT, SPRITE, CANVASGROUP
    }
    
    [SerializeField]
    private UIType uiType;

    public enum FadeStates
    {
        IDLE_HIDDEN,
        FADING_IN,
        IDLE_SHOWN,
        FADING_OUT
    }

    [Tooltip("Lets us know the current state of the Fade component.")]
    [SerializeField]
    private FadeStates fadeState;

    //store image/text + color
    private CanvasGroup _canvasGroup;
    Image thisImage;
    RawImage rawImage;
    Text thisText;
    TMP_Text tmpText;
    SpriteRenderer spritRenderer;
    Color alphaValue;

    [Tooltip("Should the object fade in and be active at start?")]
    [SerializeField] private bool shownAtStart;

    [SerializeField] private float startAlpha = 0f;
    [Tooltip("Will the object stay active after fading out?")]
    [SerializeField] private bool keepActive = true;

    [Header("Fade In Settings")]
    [Tooltip("Alpha value that the item will fade in to. Defaults to 1.")]
    [SerializeField] private float fadeInAmount = 1f;
    [Tooltip("Controls the length of the fade in. Defaults to 1.")]
    [SerializeField] private float fadeInDuration = 1f;
    [Tooltip("Amount of time item will wait before fading in. Defaults to 0.")]
    [SerializeField] private float fadeInWait;

    private IEnumerator waitToFadeIn;
    private IEnumerator fadeIn;

    [Header("Fade Out Settings")]
    [Tooltip("Check this to apply fade out as soon as the object has faded in. Can be combined with a fade out wait.")]
    [SerializeField] private bool fadeOutImmediately;
    [Tooltip("Alpha value that the item will fade out to. Defaults to 0.")]
    [SerializeField] private float fadeOutAmount = 0f;
    [Tooltip("Controls the length of the fade out. Defaults to 1.")]
    [SerializeField] private float fadeOutDuration = 1f;
    [Tooltip("Amount of time item will wait before fading out. Defaults to 0.")]
    [SerializeField] private float fadeOutWait;

    private IEnumerator waitToFadeOut;
    private IEnumerator fadeOut;

    public UnityEvent FadedOut;

    #region Properties

    /// <summary>
    /// Lets us know the fade component is in a state where the Image may be showing. 
    /// </summary>
    public bool IsShowing => fadeState == FadeStates.FADING_IN || fadeState == FadeStates.IDLE_SHOWN;

    #endregion

    void Awake()
    {
        GetUiType();
    }

    void GetUiType()
    {
        //checks privately whether this object has image or text component
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            thisImage = GetComponent<Image>();
            if (thisImage == null)
            {
                rawImage = GetComponent<RawImage>();

                if (rawImage == null)
                {
                    thisText = GetComponent<Text>();

                    //it's a TMP TEXT
                    if (thisText == null)
                    {
                        tmpText = GetComponent<TMP_Text>();
                        //last check --> sprite
                        if (tmpText == null)
                        {
                            spritRenderer = GetComponent<SpriteRenderer>();
                            uiType = UIType.SPRITE;
                        }
                        //it's a TMP text
                        else
                        {
                            uiType = UIType.TMPTEXT;
                        }
                    }
                    //its a Text
                    else
                    {
                        uiType = UIType.TEXT;
                    }
                }
                else
                {
                    uiType = UIType.RAWIMAGE;
                }
            }
            //its an image
            else
            {
                uiType = UIType.IMAGE;
            }
        }
        else
        {
            uiType = UIType.CANVASGROUP;
        }
        
        SetAlpha();
    }

    void Start()
    {
        GetUiType();
        
        alphaValue.a = startAlpha;
        UpdateAlpha();
        
        //set starting fade state to hidden by default. 
        fadeState = FadeStates.IDLE_HIDDEN;

        //automatically fadeIn at start if object has this script
        if (shownAtStart)
        {
            FadeIn();
        }
    }

    /// <summary>
    /// Starts the object fading in if it isn't already.
    /// Objects can be set to automatically fade out in the fade UI component and will do that when they finish fading in.
    /// </summary>
    public void FadeIn()
    {
        StopAllCoroutines();

        if (fadeInWait > 0)
        {
            if (gameObject.activeSelf)
                StartCoroutine(WaitToFadeIn());
        }
        else
        {
            if (gameObject.activeSelf)
                StartCoroutine(FadeInOverTime());
        }
    }
    
    /// <summary>
    /// Fades out the object. 
    /// </summary>
    public void FadeOut()
    {
        StopAllCoroutines();

        if (fadeOutWait > 0)
        {
            if (gameObject.activeSelf)
                StartCoroutine(WaitToFadeOut());
        }
        else
        {
            if(gameObject.activeSelf)
                StartCoroutine(FadeOutOverTime());
        }
    }

    IEnumerator FadeInOverTime()
    {
        //set fade state.
        fadeState = FadeStates.FADING_IN;

        //get start values 
        float currentTime = 0;
        float startAlpha = alphaValue.a;
        if (uiType == UIType.CANVASGROUP)
        {
            startAlpha = _canvasGroup.alpha;
        }

        //actual value transformation over time
        while (currentTime <= fadeInDuration)
        {
            var lerpValue = currentTime / fadeInDuration;
            alphaValue.a = Mathf.Lerp(startAlpha, fadeInAmount, lerpValue);
            if (uiType == UIType.CANVASGROUP)
            {
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, fadeInAmount, lerpValue);
            }
            currentTime += Time.deltaTime;
            UpdateAlpha();
            yield return null;
        }

        SetOpaque();

        //set fade state.
        fadeState = FadeStates.IDLE_SHOWN;

        //begin immediate fade out?
        if (fadeOutImmediately)
        {
            FadeOut();
        }
    }

    /// <summary>
    /// Final set value of Fade in method. 
    /// </summary>
    public void SetOpaque()
    {
        alphaValue.a = fadeInAmount;
        UpdateAlpha();
        
        if (uiType == UIType.CANVASGROUP)
        {
            _canvasGroup.alpha = fadeInAmount;
        }
    }

    IEnumerator FadeOutOverTime()
    {
        //set fade state.
        fadeState = FadeStates.FADING_OUT;

        //get start values 
        float currentTime = 0;
        float startAlpha = alphaValue.a;
        if (uiType == UIType.CANVASGROUP)
        {
            startAlpha = _canvasGroup.alpha;
        }

        //actual value transformation over time
        while (currentTime <= fadeOutDuration)
        {
            var lerpValue = currentTime / fadeOutDuration;
            alphaValue.a = Mathf.Lerp(startAlpha, fadeOutAmount, lerpValue);
            if (uiType == UIType.CANVASGROUP)
            {
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, fadeOutAmount, lerpValue);
            }
            currentTime += Time.deltaTime;
            UpdateAlpha();
            yield return null;
        }

        SetTransparent();

        //set fade state.
        fadeState = FadeStates.IDLE_HIDDEN;
        FadedOut.Invoke();

        //disable object?
        if (!keepActive)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Final set value of Fade out method. 
    /// </summary>
    public void SetTransparent()
    {
        alphaValue.a = fadeOutAmount;
        UpdateAlpha();
        if (uiType == UIType.CANVASGROUP)
        {
            _canvasGroup.alpha = fadeOutAmount;
        }
    }

    //switch statement sets alpha depending on component type 
    void SetAlpha()
    {
        switch (uiType)
        {
            case UIType.IMAGE:
                alphaValue = thisImage.color;
                break;
            case UIType.RAWIMAGE:
                alphaValue = rawImage.color;
                break;
            case UIType.TEXT:
                alphaValue = thisText.color;
                break;
            case UIType.TMPTEXT:
                alphaValue = tmpText.color;
                break;
            case UIType.SPRITE:
                alphaValue = spritRenderer.color;
                break;
        }
    }

    //switch statement updates alpha depending on component type 
    void UpdateAlpha()
    {
        switch (uiType)
        {
            case UIType.IMAGE:
                thisImage.color = alphaValue;
                break;
            case UIType.RAWIMAGE:
                rawImage.color = alphaValue;
                break;
            case UIType.TEXT:
                thisText.color = alphaValue;
                break;
            case UIType.TMPTEXT:
                tmpText.color = alphaValue;
                break;
            case UIType.SPRITE:
                spritRenderer.color = alphaValue;
                break;
        }
    }

    IEnumerator WaitToFadeIn()
    {
        yield return new WaitForSeconds(fadeInWait);

        StartCoroutine(FadeInOverTime());
    }

    public void SetWaitToFadeOut(float wait)
    {
        fadeOutWait = wait;
        FadeOut();
    }
    IEnumerator WaitToFadeOut()
    {
        yield return new WaitForSeconds(fadeOutWait);

        StartCoroutine(FadeOutOverTime());
    }
}