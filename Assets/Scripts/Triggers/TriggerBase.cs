using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerBase : MonoBehaviour
{
    protected bool canTrigger;
    
    [Header("Trigger Cause Settings")]
    [Tooltip("Lets us know if this object has been triggered.")]
    public bool hasTriggered;
    [Tooltip("Lets us know accepted object tags which can trigger this.")]
    public string[] acceptedTags;
    [Tooltip("Check this and set the specific game object if there is only one very specific object which can trigger this.")]
    public bool specificObject;
    [Tooltip("Specific object this trigger is waiting for.")]
    public GameObject specificObj;

    [Header("Wait & Reset Settings")]
    [Tooltip("Check if trigger waits to activate its effect once it detects a relevant cause.")]
    public bool waits;
    [Tooltip("Amount of time to wait once OnTriggerEnter fires.")]
    public float waitTime = 5f;

    [Tooltip("Check if trigger can activate repeatedly")]
    public bool repeats;
    [Tooltip("Amount of time for trigger to reset.")]
    public float resetTime = 5f;
    
    //Coroutines for wait/reset
    protected IEnumerator waitToTrigger;
    protected IEnumerator resetTrigger;

    /// <summary>
    /// Called whenever an object with a collider enters this collider (marked as trigger). 
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        //already triggered?
        if (hasTriggered)
        {
            return;
        }

        //check for a specific obj to trigger this. 
        if (specificObject)
        {
            if (other.gameObject == specificObj)
            {
                SetTrigger();
            }
        }
        else
        {
            //loop through accepted tags to check if this object effects the trigger. 
            foreach (var tag in acceptedTags)
            {
                if (other.gameObject.CompareTag(tag))
                {
                    SetTrigger();
                }
            }
        }
    }

    /// <summary>
    /// Sets the trigger logic in motion regardless of the cause.
    /// Has built in wait/immediate check.
    /// Sets has triggered to true regardless.
    /// </summary>
    public virtual void SetTrigger()
    {
        if (waits)
        {
            SetCoroutine(waitToTrigger, WaitToTrigger());
        }
        else
        {
            ActivateTriggerEffect();
        }

        hasTriggered = true;
    }

    /// <summary>
    /// Actually causes the effect of the trigger.
    /// Should be called with a public override void
    /// Include base.ActivateTriggerEffect() at the end of your logic to get the repeat. 
    /// </summary>
    public virtual void ActivateTriggerEffect()
    {
        //call repeat if necessary 
        if (repeats)
        {
            SetCoroutine(resetTrigger, Reset());
        }
    }

    #region Coroutines (Wait & Reset)

    /// <summary>
    /// Abstracted method for setting a stored variable coroutine (and making sure any instances of it running stop first).
    /// </summary>
    /// <param name="coroutineVar"></param>
    /// <param name="coroutineMethod"></param>
    protected virtual void SetCoroutine(IEnumerator coroutineVar, IEnumerator coroutineMethod)
    {
        //stop it if it exists
        if (coroutineVar != null)
        {
            StopCoroutine(coroutineVar);
        }

        //start new version of coroutine. 
        coroutineVar = coroutineMethod;
        StartCoroutine(coroutineVar);
    }
    
    /// <summary>
    /// Waits for wait time from moment of cause to activate the trigger effect. 
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator WaitToTrigger()
    {
        yield return new WaitForSeconds(waitTime);

        ActivateTriggerEffect();
    }

    /// <summary>
    /// Waits for reset time to reset has triggered, allowing the trigger to enact its cause/effect logic again.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator Reset()
    {
        yield return new WaitForSeconds(resetTime);

        hasTriggered = false;
    }

    /// <summary>
    /// Allows you to manually reset a trigger at runtime.
    /// </summary>
    public virtual void ManualReset()
    {
        hasTriggered = false;
    }

    #endregion
}