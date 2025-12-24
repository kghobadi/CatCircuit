using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple food item class. 
/// </summary>
public class FoodItem : MonoBehaviour
{
    private CatController consumer;
    private SpriteRenderer _spriteRenderer;
    [Header("Food Settings")] [SerializeField]
    private FoodScriptable foodData;

    [SerializeField] private int truePoints;
   
    public bool consuming;
    public float consumeSpeed = 5f;
    private Transform endPos;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void AssignFoodData(FoodScriptable data)
    {
        foodData = data;
        _spriteRenderer.sprite = data.foodSprite;
    }

    public void SetScore(int multiplier)
    {
        truePoints = foodData.pointsValue * multiplier;
    }

    /// <summary>
    /// For a death item. 
    /// </summary>
    /// <param name="pts"></param>
    public void SetScoreDeath(int pts)
    {
        truePoints = pts;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !consuming)
        {
            CatController cat = other.gameObject.GetComponentInParent<CatController>();
            CatConsumesMe(cat);
        }
    }

    /// <summary>
    /// Called by cat hitting my trigger. 
    /// </summary>
    /// <param name="consume"></param>
    void CatConsumesMe(CatController consume)
    {
        if (consume.HealthUI.IsDead)
            return;
        consumer = consume;
        consumer.GainFood(truePoints);
        consumer.CatAudio.PlaySoundRandomPitch(foodData.foodSound, 1f);
        
        //TODO play effect before destroy? 
        //TODO move to Player UI spot while inactive? 
        endPos = consumer.ConsumePos;
        consuming = true;
    }

    /// <summary>
    /// Hoovers up the food into cat consume pos. 
    /// </summary>
    private void Update()
    {
        if (consuming)
        {
            //Update end pos according to flip state 
            transform.position = Vector3.Lerp(transform.position, endPos.position, consumeSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, consumeSpeed * Time.deltaTime);
            float dist = Vector2.Distance(transform.position, endPos.position);
            if (dist < 0.05f)
            {
                Destroy(gameObject);
            }
        }
    }
}
