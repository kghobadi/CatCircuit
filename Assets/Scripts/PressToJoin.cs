using Rewired;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CatController;

/// <summary>
/// Handles specific player set up in Title screen. 
/// </summary>
public class PressToJoin : MonoBehaviour
{
    public int playerId;
    private Player player; // The Rewired Player
    [SerializeField] private string joinKey = "";
    [SerializeField] private string removeKey = "";

    [SerializeField]
    private SpriteRenderer cat;
    [SerializeField]
    private SpriteRenderer collar;
    [SerializeField]
    private CatAudio catAudio;
    [SerializeField] private TMP_Text actionText;

    public bool playerJoined;
    public bool playerRemoved;
    [SerializeField]
    private RectTransform pJoinedUI;
    [SerializeField]
    private RectTransform pRemovedUI;
    [SerializeField] private TMP_Text pJoinedText;

    string pJoined => "Player" + playerId.ToString() + "_joined";
    string pRemoved => "Player" + playerId.ToString() + "_removed";

#if UNITY_EDITOR
    private void OnValidate()
    {
        string playerName = "P" + (playerId + 1).ToString();
        gameObject.name = "House (" + playerName + ")";
        pJoinedUI.gameObject.name = pJoinedUI.gameObject.name.Replace("P1", playerName);
        pRemovedUI.gameObject.name = pRemovedUI.gameObject.name.Replace("P1", playerName);
        actionText.gameObject.name = actionText.gameObject.name.Replace("P1", playerName);
        pJoinedUI.gameObject.name = pJoinedUI.gameObject.name.Replace("P1", playerName);
        pJoinedText.gameObject.name = pJoinedText.gameObject.name.Replace("P1", playerName);
        pJoinedText.color = collar.color;
        actionText.color = collar.color;
    }
#endif

    void Start()
    {
        //Get player ids
        player = ReInput.players.GetPlayer(playerId);
        actionText.enabled = false;

        //start off uI 
        pJoinedUI.gameObject.SetActive(false);
        pRemovedUI.gameObject.SetActive(true);
    }

    // Detect input 
    void Update()
    {
        if (player.GetButtonDown(joinKey))
        {
            PlayerJoined();
        }
        else if (player.GetButtonDown(removeKey))
        {
            PlayerRemoved();
        }
    }

    public void PlayerJoined()
    {
        Meow();
        playerJoined = true;
        playerRemoved = false;
        cat.gameObject.SetActive(true);
        pJoinedUI.gameObject.SetActive(true);
        pRemovedUI.gameObject.SetActive(false);
        //actionText.gameObject.SetActive(true);

        PlayerPrefs.SetString(pJoined, "true");
        PlayerPrefs.SetString(pRemoved, "false");
    }

    public void PlayerRemoved()
    {
        playerJoined = false;
        playerRemoved = true;
        cat.gameObject.SetActive(false);
        catAudio.RandomScratch();
        pJoinedUI.gameObject.SetActive(false);
        pRemovedUI.gameObject.SetActive(true);
        //actionText.gameObject.SetActive(false);

        PlayerPrefs.SetString(pRemoved, "true");
        PlayerPrefs.SetString(pJoined, "false");
    }

    void Meow()
    {
        if (catAudio.myAudioSource.isPlaying)
            return;

        catAudio.RandomMeow();
        SetActionText("MEOW!");
    }

    void SetActionText(string message)
    {
        actionText.text = message;
        actionText.enabled = true;

        if (waitToDisableActionText != null)
        {
            StopCoroutine(waitToDisableActionText);
        }
        waitToDisableActionText = WaitToDisableActionText();
        StartCoroutine(waitToDisableActionText);
    }

    private IEnumerator waitToDisableActionText;
    IEnumerator WaitToDisableActionText()
    {
        yield return new WaitForEndOfFrame();

        while (catAudio.myAudioSource.isPlaying)
        {
            yield return null;
        }

        actionText.enabled = false;
    }
}
