using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    public int numberOfFood;
    public GameObject foodPrefab;

    public float xRange = 30f;
    public float zRange = 30f;
    public void SpawnFood()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-xRange, xRange), 0, Random.Range(-zRange, zRange));
        GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
    }

    public void SpawnAllFood()
    {
        for(int i = 0; i < numberOfFood; i++)
        {
            SpawnFood();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SpawnAllFood();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
