using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Teleports cat to a position. 
/// </summary>
public class TeleportTrigger : AudioHandler
{
    private FadeUiRevamped teleFader;
    [SerializeField] private bool proximityFade;
    [Header("Teleport Settings")]
    [SerializeField] private AudioClip[] teleportSounds;
    [SerializeField] private Transform teleportSpot;
    [SerializeField] private float teleportWait = 0f;

    public override void Awake()
    {
        base.Awake();
        teleFader = GetComponent<FadeUiRevamped>();
    }

    private void Update()
    {
        //fade loop 
        if (proximityFade)
        {
            CatController nearest = GameManager.Instance.GetNearestCatToPoint(transform.position);
            float dist = Vector2.Distance(transform.position, nearest.transform.position);
            if (dist < 0.5f)
            {
                if (!teleFader.IsShowing)
                {
                    teleFader.FadeIn();
                }
            }
            else
            {
                if (teleFader.IsShowing)
                {
                    teleFader.FadeOut();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CatController cat = other.gameObject.GetComponentInParent<CatController>();

            //Teleport cat - NO AI for now. 
            if (cat && !cat.teleporting && !cat.IsAiEnabled)
            {
                cat.TeleportCatWithWait(teleportWait, teleportSpot);
                PlayRandomSoundRandomPitch(teleportSounds, 1f);
            }
        }
    }
}
