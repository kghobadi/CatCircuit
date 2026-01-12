using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Fulfills some action on Press Any Key.
/// Controls the main menu behavior and attract loop. 
/// </summary>
public class PressAnyKey : MonoBehaviour
{
    public int [] playerIds ;
    private Player[] player; // The Rewired Player

    [SerializeField]
    private bool useParticularKey;
    [SerializeField] private string keyToPress = "";
    public int sceneToLoad;
    public bool loadsScene;
    [SerializeField] private float loadWait = 0f;
    public FadeUiRevamped[] fadeObjects;
    
    public UnityEvent OnPress;
    public bool hasLoaded;

    public AudioSource menuMusic;
    [SerializeField] private PressToJoin[] pressToJoins;
    private const string Demo = "Demo";
    private bool inAttract;
    private bool playedVideo;
    [SerializeField] private VideoPlayer attractVideo;
    [SerializeField] private CanvasFader attractFader;
    void Start()
    {
        //Get player ids
        player = new Player[playerIds.Length];
        for (int i = 0; i < playerIds.Length; i++)
        {
            player[i] = ReInput.players.GetPlayer(playerIds[i]);
        }
        menuMusic.Play();
    }
    
    void Update()
    {
        //When menu music stops playing...
        if (!menuMusic.isPlaying)
        {
            //Loop music while there is a joined player 
            if (GetHasPlayerJoined())
            {
                menuMusic.Play();
            }
            //Run attract loop otherwise 
            else if(!inAttract)
            {
                AttractLoop();
            }
        }
        else
        {
            inAttract = false;
        }
        //We are in attract video 
        if (inAttract)
        {
            //Immediately stop if input or...
            if (GetHasPlayerJoined() || 
                (!attractVideo.isPlaying && playedVideo && attractVideo.time > 0)) // video end conditions
            {
                StopVideo();
            }
        }
        
        DetectStartInput();
    }

    bool GetHasPlayerJoined()
    {
        bool hasPlayerJoined = false;
        for (int i = 0; i < pressToJoins.Length; i++)
        {
            if (pressToJoins[i].playerJoined)
            {
                hasPlayerJoined = true;
                break;
            }
        }

        return hasPlayerJoined;
    }

    /// <summary>
    /// Detect input  
    /// </summary>
    void DetectStartInput()
    {
        //Detect start input 
        for (int i = 0; i < player.Length; i++)
        {
            bool pressAnyKey = player[i].GetAnyButton();
            if (useParticularKey)
            {
                pressAnyKey = player[i].GetButtonDown(keyToPress);
            }
            if(pressAnyKey && !hasLoaded)
            {
                //Ensure its not demo mode 
               PlayerPrefs.SetString(Demo, "false");
               BeginGame();
            }
        }
    }

    /// <summary>
    /// Actually loads into the Main scene. 
    /// </summary>
    void BeginGame()
    {
        if (loadsScene)
        {
            StartCoroutine(WaitForLoad());
        }

        //fade out all fade objs 
        foreach (var fade in fadeObjects)
        {
            if(gameObject.activeSelf)
                fade.FadeOut();
        }
        OnPress.Invoke();

        hasLoaded = true;
    }

    IEnumerator WaitForLoad()
    {
        yield return new WaitForSeconds(loadWait);
        SceneManager.LoadScene(sceneToLoad);
    }

    void AttractLoop()
    {
        //Get demo value - determines if we show autoplay round or Mr Ew video 
        bool showDemo = PlayerPrefs.GetString(Demo) == "true";
        showDemo = !showDemo;
        if (showDemo)
        {
            PlayerPrefs.SetString(Demo, "true");
        }
        else
        {
            PlayerPrefs.SetString(Demo, "false");
        }
        
        if (showDemo)
        {
            BeginGame();
            playedVideo = false;
        }
        else
        {
            ShowVideo();
        }

        inAttract = true;
    }

    /// <summary>
    /// Shows mr ew montage video. 
    /// </summary>
    void ShowVideo()
    {
        attractFader.FadeIn();
        attractVideo.time = 0;
        playedVideo = true;
        attractVideo.Play();
    }

    /// <summary>
    /// Stops mr ew video 
    /// </summary>
    void StopVideo()
    {
        attractFader.FadeOut();
        menuMusic.Play();
        playedVideo = false;
    }
}
