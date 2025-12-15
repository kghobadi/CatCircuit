using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Responsible for spawning cars. 
/// </summary>
public class CarSpawner : MonoBehaviour
{
    [SerializeField] private bool spawning;
    [SerializeField] private float spawnTimer;
    [SerializeField] private Vector2 spawnTimeRange = new Vector2(10f, 15f);

    [SerializeField] private GameObject[] cars;
    [Tooltip("These must all be within 0 - 100. They should be increasing, eg 35, 55, 75")]
    [SerializeField] private float[] randomnessIntervals;
    [SerializeField] private GameObject carClone;
    private Car lastCar;
    [SerializeField] private bool flipped;

    public bool hasSpawnedMailman;

    private void Start()
    {
        SetSpawnTimer();

        GameManager.Instance.OnQuarterEvent.AddListener(OnQuarterEvent);
    }

    void Update()
    {
        if (spawning)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer < 0)
            {
                SpawnRandomCar();
            }
        }  
    }

    void SpawnRandomCar()
    {
        GameObject randomCarPrefab = cars[0];
        if (cars.Length > 1)
        {
            float randomCar = UnityEngine.Random.Range(0f, 100f);
            for (int i = 0; i < randomnessIntervals.Length; i++)
            {
                //Check if the chance fell below the interval 
                if (randomCar < randomnessIntervals[i])
                {
                    randomCarPrefab = cars[i]; 
                    break;
                }
            }
        }
        carClone = Instantiate(randomCarPrefab, transform.position, Quaternion.identity, transform);
        lastCar = carClone.GetComponent<Car>();
        if (lastCar.carType == Car.CarType.MailTruck)
            hasSpawnedMailman = true; 
        lastCar.SetMoving(flipped);
        SetSpawnTimer();
    }

    void SpawnMailman()
    {
        if(hasSpawnedMailman)
            return;
        
        carClone = Instantiate(cars[3], transform.position, Quaternion.identity, transform);
        lastCar = carClone.GetComponent<Car>();
        if (lastCar.carType == Car.CarType.MailTruck)
            hasSpawnedMailman = true; 
        lastCar.SetMoving(flipped);
        SetSpawnTimer();
    }

    void SetSpawnTimer()
    {
        spawnTimer = Random.Range(spawnTimeRange.x, spawnTimeRange.y);
    }

    /// <summary>
    /// Checks at quarter time if there's been mailmen. Spawns on L or R if so. 
    /// </summary>
    void OnQuarterEvent(int quarter)
    {
        if (flipped)
        {
            if (quarter == 2 || quarter == 4)
            {
                SpawnMailman();
            }
        }
        else
        {
            if (quarter == 1 || quarter == 3)
            {
                SpawnMailman();
            }
        }

        hasSpawnedMailman = false;
    }
}
