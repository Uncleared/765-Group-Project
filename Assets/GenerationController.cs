using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class GenerationController : MonoBehaviour
{
    public float bounds = 30f;
    public int populationSize = 50;
    public GameObject creaturePrefab;
    public List<GameObject> population;
    public float populationLifetime = 5.0f;
    public float mutationRate; // Chance of a mutation occuring, 0 - 100%

    public TextMeshProUGUI generationText;
    public TextMeshProUGUI maxSurvivalText;
    private int currentGeneration = 1;
    private float lifetimeLeft;

    public MapSpawner mapSpawner;

    float timer = 0f;

    public int died = 0;

    public void RegisterDead()
    {
        died++;
    }
    // Start is called before the first frame update
    void Start()
    {
        // Initialise a random starting population
        InitialisePopulation();

        // For each population Lifetime, breed a new generation, this will repeat indefinitely

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

        timer -= Time.deltaTime;
        if(timer <= 0f || died == populationSize - 1)
        {
            timer = populationLifetime;
            BreedPopulation();
        }
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
            agent.controller = this;
            agent.bounds = bounds;

            // Add the creature to the population
            population.Add(creature);
        }
    }

    float maxSurvivalTime = 0f;
    private void BreedPopulation()
    {
     

        died = 0;
        mapSpawner.Reset(populationSize);
        List<GameObject> newPopulation = new List<GameObject>();

        // Remove unfit individuals, by sorting the list by the longest surviving creatures
        List<GameObject> sortedList = population.OrderByDescending(o => o.GetComponent<Agent>().survivalTime).ToList();

        // Write to spreadsheet
        // The target file path e.g.
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);


        var filePath = Path.Combine(folder, "generationData.csv");

        using (var writer = new StreamWriter(filePath, true))
        {
            writer.Write("\n" + currentGeneration + "," + sortedList[0].GetComponent<Agent>().survivalTime);
        }
        print(filePath);

        if (sortedList[0].GetComponent<Agent>().survivalTime > maxSurvivalTime)
        {
            maxSurvivalTime = sortedList[0].GetComponent<Agent>().survivalTime;
        }
        maxSurvivalText.text = "Max Survival Time: " + maxSurvivalTime;
        
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
        Agent offspringAgent = offspring.GetComponent<Agent>();
        offspringAgent.controller = this;

        // Get the parents DNA
        Agent agent1 = parent1.GetComponent<Agent>();
        Agent agent2 = parent2.GetComponent<Agent>();

        // MIX the two neural nets together
        NeuralNet childNet = new NeuralNet(agent1.net.InputLayer.Count, agent1.net.HiddenLayers.Count, agent1.net.OutputLayer.Count);
        childNet.InputLayer = new List<Neuron>(agent1.net.InputLayer);
        childNet.HiddenLayers = new List<List<Neuron>>(agent1.net.HiddenLayers);
        childNet.OutputLayer = new List<Neuron>(agent1.net.OutputLayer);

        // Get a mix of the parents DNA majority of the time, dependant on mutation chance
        // Breed the weights
        for(int i = 0; i < agent1.net.InputLayer.Count; i++)
        {
            Neuron neuron = agent1.net.InputLayer[i];
            for(int j = 0; j < neuron.InputSynapses.Count; j++)
            {
                // Decide between the two parents
                if(Random.value > 0.5f)
                {
                    childNet.InputLayer[i].InputSynapses[j].Weight = Random.value > mutationRate ? neuron.InputSynapses[j].Weight : Random.value;
                }
                else
                {
                    childNet.InputLayer[i].InputSynapses[j].Weight = Random.value > mutationRate ? agent2.net.InputLayer[i].InputSynapses[j].Weight : Random.value;
                }
            }

            for (int j = 0; j < neuron.OutputSynapses.Count; j++)
            {
                // Decide between the two parents
                if (Random.value > 0.5f)
                {
                    childNet.InputLayer[i].OutputSynapses[j].Weight = Random.value > mutationRate ?  neuron.OutputSynapses[j].Weight : Random.value;
                }
                else
                {
                    childNet.InputLayer[i].OutputSynapses[j].Weight = Random.value > mutationRate ? agent2.net.InputLayer[i].OutputSynapses[j].Weight : Random.value;
                }
            }
        }
            
        for(int i = 0; i < agent1.net.HiddenLayers.Count; i++)
        {
            List<Neuron> layer = agent1.net.HiddenLayers[i];
            for(int j = 0; j < layer.Count; j++)
            {
                Neuron neuron = layer[j];
                for(int k = 0; k < neuron.InputSynapses.Count; k++)
                {
                    // Decide between the two parents
                    if (Random.value > 0.5f)
                    {
                        childNet.HiddenLayers[i][j].InputSynapses[k].Weight = Random.value > mutationRate ? neuron.InputSynapses[k].Weight : Random.value;
                    }
                    else
                    {
                        childNet.HiddenLayers[i][j].InputSynapses[k].Weight = Random.value > mutationRate ? agent2.net.HiddenLayers[i][j].InputSynapses[k].Weight : Random.value;
                    }
                }
                for (int k = 0; k < neuron.OutputSynapses.Count; k++)
                {
                    // Decide between the two parents
                    if (Random.value > 0.5f)
                    {
                        childNet.HiddenLayers[i][j].OutputSynapses[k].Weight = Random.value > mutationRate ? neuron.OutputSynapses[k].Weight: Random.value;
                    }
                    else
                    {
                        childNet.HiddenLayers[i][j].OutputSynapses[k].Weight = Random.value > mutationRate ? agent2.net.HiddenLayers[i][j].OutputSynapses[k].Weight : Random.value;
                    }
                }
            }
        }

        for (int i = 0; i < agent1.net.OutputLayer.Count; i++)
        {
            Neuron neuron = agent1.net.OutputLayer[i];
            for (int j = 0; j < neuron.InputSynapses.Count; j++)
            {
                // Decide between the two parents
                if (Random.value > 0.5f)
                {
                    childNet.OutputLayer[i].InputSynapses[j].Weight = Random.value > mutationRate ? neuron.InputSynapses[j].Weight : Random.value;
                }
                else
                {
                    childNet.OutputLayer[i].InputSynapses[j].Weight = Random.value > mutationRate ? agent2.net.OutputLayer[i].InputSynapses[j].Weight : Random.value;
                }
            }

            for (int j = 0; j < neuron.OutputSynapses.Count; j++)
            {
                if (Random.value > 0.5f)
                {
                    childNet.OutputLayer[i].OutputSynapses[j].Weight = Random.value > mutationRate ? neuron.OutputSynapses[j].Weight : Random.value;
                }
                else
                {
                    childNet.OutputLayer[i].OutputSynapses[j].Weight = Random.value > mutationRate ? agent2.net.OutputLayer[i].OutputSynapses[j].Weight : Random.value;
                }
            }
        }

        // Assign the breeded neural net
        offspring.GetComponent<Agent>().net = childNet;

        return offspring;
    }
}