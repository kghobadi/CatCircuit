using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Teleports cat to a position. 
/// </summary>
public class TeleportTrigger : AudioHandler
{
    [Header("Teleport Settings")]
    [SerializeField] private AudioClip[] teleportSounds;
    [SerializeField] private Transform teleportSpot;
    [SerializeField] private float teleportWait = 0f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CatController cat = other.gameObject.GetComponentInParent<CatController>();

            //Teleport cat 
            if (cat && !cat.teleporting)
            {
                cat.TeleportCatWithWait(teleportWait, teleportSpot);
                PlayRandomSoundRandomPitch(teleportSounds, 1f);
            }
        }
    }
}
