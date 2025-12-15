using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles behavior of moving cars. 
/// </summary>
public class Car : AudioHandler
{
    private SpriteRenderer carSprite;
    private Rigidbody2D body;
    [Header("Car Settings")]
    public CarType carType;
    public enum CarType
    {
        Normal,
        MailTruck,
    }

    [SerializeField] private float moveSpeed;
    public bool moving = true;
    [SerializeField] private Vector2 speedRange = new Vector2(5,10);
    private Vector2 dir;
    [SerializeField] private AudioClip[] honkSounds;
    [SerializeField] private AudioClip[] crashSounds;

    public bool flipped; // set true when going up

    [Header("Mail Settings")] 
    public Inhabitant mailman;
    public override void Awake()
    {
        base.Awake();
        body = GetComponent<Rigidbody2D>();
        carSprite = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Called by spawner 
    /// </summary>
    /// <param name="flip"></param>
    public void SetMoving(bool flip)
    {
        flipped = flip;
        carSprite.flipY = flip;
        
        moveSpeed = Random.Range(speedRange.x, speedRange.y);
        moving = true;
        dir = Vector2.down;
        if (flipped)
        {
            dir = Vector2.up;
        }
        
        gameObject.SetActive(true);
        PlayRandomSoundRandomPitch(honkSounds, 0.5f);
    }

    void FixedUpdate()
    {
        if (moving)
        {
            body.AddForce(moveSpeed * dir,  ForceMode2D.Force);
        }
    }

    /// <summary>
    /// Called by AttackTrigger
    /// </summary>
    /// <param name="obj"></param>
    public void Crash(GameObject obj)
    {
        HealthUI cat = obj.GetComponentInParent<HealthUI>();
        if (cat)
        {
            //This will kill the cat with proper UI 
            cat.UpdateHealth(0);
        }
        //Hit a dog? 
        else
        {
            Inhabitant inhabitant = obj.GetComponent<Inhabitant>();
            if (inhabitant)
            {
                inhabitant.DisableInhabitant();
            }
        }
        PlayRandomSound(crashSounds, 0.5f);
    }

    //Triggering delivery should make the packages get thrown out 
    public void TriggerDeliveries(House[] houses)
    {
        StartCoroutine(Deliveries(houses));
    }

    IEnumerator Deliveries(House[] houses)
    {
        //stop truck
        body.velocity = Vector2.zero;
        moving = false;
        
        //show mailman
        mailman.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        //Throw out the packages. 
        for (int i = 0; i < houses.Length; i++)
        {
            float randomPackageChance = Random.Range(0f, 100f);
            if (randomPackageChance < houses[i].Inhab.MailChance)
            {
                if(i == 0)
                    yield return new WaitForSeconds(0.25f);
                
                mailman.ThrowDelivery( houses[i]);

                yield return new WaitForSeconds(0.25f);
            }
        }
        
        yield return new WaitForSeconds(0.25f);
        //bye bye 
        mailman.gameObject.SetActive(false);
        moving = true;
    }

    private int triggerCount;
    public int countToDespawn = 2;
    public void CountTriggers()
    {
        triggerCount++;
        if (triggerCount >= countToDespawn)
        {
            Destroy(gameObject);
        }
    }
}
