using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNeuralNet : MonoBehaviour
{
    NeuralNet net;
    // Start is called before the first frame update
    void Start()
    {
        // Take in the direction of closesness to agent and food, as well as hunger level
        net = new NeuralNet(3, 5, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            net.RandomizeWeights();
            print(net.Compute(0.1f, 0.5f, 0.1f)[0]);
        }
    }
    }
