using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Fulfills some action on Press Any Key. 
/// </summary>
public class PressAnyKey : MonoBehaviour
{
    public int [] playerIds ;
    private Player[] player; // The Rewired Player

    public int sceneToLoad;
    public bool loadsScene;
    [SerializeField] private float loadWait = 0f;
    public FadeUiRevamped[] fadeObjects;
    
    public UnityEvent OnPress;
    
    void Start()
    {
        //Get player ids
        player = new Player[playerIds.Length];
        for (int i = 0; i < playerIds.Length; i++)
        {
            player[i] = ReInput.players.GetPlayer(playerIds[i]);
        }
    }

    // Detect input 
    void Update()
    {
        for (int i = 0; i < player.Length; i++)
        {
            if (player[i].GetAnyButton())
            {
                if (loadsScene)
                {
                    StartCoroutine(WaitForLoad());
                }

                //fade out all fade objs 
                foreach (var fade in fadeObjects)
                {
                    fade.FadeOut();
                }
                OnPress.Invoke();
            }
        }
    }

    IEnumerator WaitForLoad()
    {
        yield return new WaitForSeconds(loadWait);
        SceneManager.LoadScene(sceneToLoad);
    }
}
