using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAudio : AudioHandler
{
    [Header("Cat Audios")] 
    [SerializeField]
    private AudioClip[] meows;
    [SerializeField]
    private AudioClip[] purrs;
    [SerializeField]
    private AudioClip[] hiss;
    [SerializeField]
    private AudioClip[] scratch;

    public void RandomMeow()
    {
        PlayRandomSoundRandomPitch(meows, 1f);
    }
    
    public void RandomPurr()
    {
        PlayRandomSoundRandomPitch(purrs, 1f);
    }
    
    public void RandomHiss()
    {
        PlayRandomSoundRandomPitch(hiss, 1f);
    }
    
    public void RandomScratch()
    {
        PlayRandomSoundRandomPitch(scratch, 1f);
    }
}
