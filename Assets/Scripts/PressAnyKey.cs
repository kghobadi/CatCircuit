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

    [SerializeField]
    private bool useParticularKey;
    [SerializeField] private string keyToPress = "";
    public int sceneToLoad;
    public bool loadsScene;
    [SerializeField] private float loadWait = 0f;
    public FadeUiRevamped[] fadeObjects;
    
    public UnityEvent OnPress;
    public bool hasLoaded; 
    
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
            bool pressAnyKey = player[i].GetAnyButton();
            if (useParticularKey)
            {
                pressAnyKey = player[i].GetButtonDown(keyToPress);
            }
            if(pressAnyKey && !hasLoaded)
            {
                if (loadsScene)
                {
                    StartCoroutine(WaitForLoad());
                }

                //fade out all fade objs 
                foreach (var fade in fadeObjects)
                {
                    fade.gameObject.SetActive(true);
                    fade.FadeOut();
                }
                OnPress.Invoke();

                hasLoaded = true;
            }
        }
    }

    IEnumerator WaitForLoad()
    {
        yield return new WaitForSeconds(loadWait);
        SceneManager.LoadScene(sceneToLoad);
    }
}
