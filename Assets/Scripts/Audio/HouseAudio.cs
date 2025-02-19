using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseAudio : AudioHandler
{
    [Header("House Audios")] 
    [SerializeField]
    private AudioClip[] doors;

    public void RandomDoorOpen()
    {
        PlayRandomSoundRandomPitch(doors, 1f);
    }
}