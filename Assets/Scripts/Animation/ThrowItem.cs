using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple wait to throw method for Inhabitants. 
/// </summary>
public class ThrowItem : AudioHandler
{
    [Header("Throw Settings")]
    [SerializeField] private Animator inhabitantAnim;
    [SerializeField] private Vector2 timeRange = new Vector2(1f, 3f);
    [SerializeField] private Transform throwSpot;
    [SerializeField] private GameObject foodPrefab; //todo how do we determine this ? 
    [SerializeField] private AudioClip[] throwSounds;

    public int OverrideScore = -1;
    
    void OnEnable()
    {
        StartCoroutine(WaitToThrow());
    }
    
    IEnumerator WaitToThrow()
    {
        float randomWait = Random.Range(timeRange.x, timeRange.y);
        yield return new WaitForSeconds(randomWait);

        inhabitantAnim.SetTrigger("throw");
        GameObject foodClone = Instantiate(foodPrefab);
        foodClone.transform.position = throwSpot.position;
        FoodItem foodItem = foodClone.GetComponent<FoodItem>();
        if(OverrideScore > 0)
            foodItem.SetScore = OverrideScore;
        PlayRandomSound(throwSounds, 1f);

        yield return new WaitForEndOfFrame();
        
        yield return new WaitUntil(() => inhabitantAnim.GetCurrentAnimatorStateInfo(0).IsName("happy-lady-1-Idle"));
        
        yield return new WaitForSeconds(0.25f);
        //disable after we are back to idle 
        gameObject.SetActive(false);

        OverrideScore = -1;
    }
}
