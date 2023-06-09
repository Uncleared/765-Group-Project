using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    public float proportion = 0.3f;
    public int numberOfFood;
    public GameObject foodPrefab;

    public float xRange = 30f;
    public float zRange = 30f;

    public float interval = 2f;
    public float timer = 0f;

    public void Reset(int populationSize)
    {
        timer = interval;
        GameObject[] food = GameObject.FindGameObjectsWithTag("Food");
        foreach(GameObject f in food)
        {
            Destroy(f);
        }

        for (int i = 0; i < Mathf.Ceil(proportion * populationSize); i++)
        {
            SpawnFood();
        }


    }
    public void SpawnFood()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-xRange, xRange), 0, Random.Range(-zRange, zRange));
        GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
    }

    public void SpawnAllFood()
    {
       
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0f)
        {
            GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
            GameObject[] food = GameObject.FindGameObjectsWithTag("Food");

            int required = Mathf.CeilToInt(agents.Length * proportion);
            if(food.Length < required)
            {
                for(int i = 0; i < required - food.Length; i++)
                {
                    SpawnFood();
                }
            }
            timer = interval;
        }
    }
}
