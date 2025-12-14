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
    [SerializeField] private GameObject carClone;
    private Car lastCar;
    [SerializeField] private bool flipped;

    private void Start()
    {
        SetSpawnTimer();
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
        if(cars.Length > 1)
            randomCarPrefab = cars[Random.Range(0, cars.Length)];
        carClone = Instantiate(randomCarPrefab, transform.position, Quaternion.identity, transform);
        lastCar = carClone.GetComponent<Car>();
        lastCar.SetMoving(flipped);
        SetSpawnTimer();
    }

    void SetSpawnTimer()
    {
        spawnTimer = Random.Range(spawnTimeRange.x, spawnTimeRange.y);
    }
}
