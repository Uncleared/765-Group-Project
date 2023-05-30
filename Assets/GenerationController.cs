using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenerationController : MonoBehaviour
{
    public float bounds = 30f;
    public int populationSize = 50;
    public GameObject creaturePrefab;
    public List<GameObject> population;
    public float populationLifetime = 5.0f;
    public int mutationRate; // Chance of a mutation occuring, 0 - 100%

    public TextMeshProUGUI generationText;
    private int currentGeneration = 1;
    private float lifetimeLeft;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise a random starting population
        InitialisePopulation();

        // For each population Lifetime, breed a new generation, this will repeat indefinitely
        InvokeRepeating("BreedPopulation", populationLifetime, populationLifetime);

        // Set the value so we can show a countdown for each population
        lifetimeLeft = populationLifetime;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the text showing what generation we're on
        generationText.text = "Generation " + currentGeneration;

        // Perform a countdown, showing the lifetime of the current population, reset on breeding
        lifetimeLeft -= Time.deltaTime;
    }

    /**
     * Initialises the population
     */
    private void InitialisePopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            // Choose a random position for the creature to appear
            Vector3 pos = new Vector3(Random.Range(-bounds, bounds), 0f, Random.Range(-bounds, bounds));

            // Instantiate a new creature
            GameObject creature = Instantiate(creaturePrefab, pos, Quaternion.identity);
            Agent agent = creature.GetComponent<Agent>();
            agent.bounds = bounds;

            // Add the creature to the population
            population.Add(creature);
        }
    }

    private void BreedPopulation()
    {
        List<GameObject> newPopulation = new List<GameObject>();

        // Remove unfit individuals, by sorting the list by the longest surviving creatures
        List<GameObject> sortedList = population.OrderByDescending(o => o.GetComponent<Agent>().survivalTime).ToList();

        population.Clear();

        // then breeding only the most red creatures
        int halfOfPopulation = (int)(sortedList.Count / 2.0f);
        for (int i = halfOfPopulation - 1; i < sortedList.Count - 1; i++)
        {
            // Breed two creatures
            population.Add(Breed(sortedList[i], sortedList[i + 1]));
            population.Add(Breed(sortedList[i + 1], sortedList[i]));

        }

        // Then destroy all of the original creatures
        for (int i = 0; i < sortedList.Count; i++)
        {
            Destroy(sortedList[i]);
        }

        lifetimeLeft = populationLifetime;
        currentGeneration++;
    }

    // Breeds a new creature using the DNA of the two parents
    private GameObject Breed(GameObject parent1, GameObject parent2)
    {
        Vector3 pos = new Vector3(Random.Range(-bounds, bounds), 0f, Random.Range(-bounds, bounds));

        // Create a new creature and get a reference to its DNA
        GameObject offspring = Instantiate(creaturePrefab, pos, Quaternion.identity);
        Agent offspringDNA = offspring.GetComponent<Agent>();

        // Get the parents DNA
        Agent dna1 = parent1.GetComponent<Agent>();
        Agent dna2 = parent2.GetComponent<Agent>();

        // MIX the two neural nets together

        // Get a mix of the parents DNA majority of the time, dependant on mutation chance
        if (mutationRate <= Random.Range(0, 100))
        {
            // Pick a range between 0 - 10, if it's less than 5 then pick parent1's DNA, otherwise pick parent 2's
            offspringDNA.r = Random.Range(0, 10) < 5 ? dna1.r : dna2.r;
            offspringDNA.g = Random.Range(0, 10) < 5 ? dna1.g : dna2.g;
            offspringDNA.b = Random.Range(0, 10) < 5 ? dna1.b : dna2.b;
        }
        else
        {
            int random = Random.Range(0, 3);
            if (random == 0)
            {
                offspringDNA.r = Random.Range(0.0f, 1.0f);
                offspringDNA.g = Random.Range(0, 10) < 5 ? dna1.g : dna2.g;
                offspringDNA.b = Random.Range(0, 10) < 5 ? dna1.b : dna2.b;
            }
            else if (random == 1)
            {
                offspringDNA.r = Random.Range(0, 10) < 5 ? dna1.r : dna2.r;
                offspringDNA.g = Random.Range(0.0f, 1.0f);
                offspringDNA.b = Random.Range(0, 10) < 5 ? dna1.b : dna2.b;
            }
            else
            {
                offspringDNA.r = Random.Range(0, 10) < 5 ? dna1.r : dna2.r;
                offspringDNA.g = Random.Range(0, 10) < 5 ? dna1.g : dna2.g;
                offspringDNA.b = Random.Range(0.0f, 1.0f);
            }
        }

        // Assign the breeded neural net
        offspring.GetComponent<Agent>().material.color = new Color(offspringDNA.r, offspringDNA.g, offspringDNA.b);

        return offspring;
    }
}