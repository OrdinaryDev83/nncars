using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    /// <summary>
    /// Singleton Pattern
    /// </summary>
    public static CarManager i = null;

    private void Awake()
    {
        i = this;
    }

    /// <summary>
    /// Prefab Object of the Car Entity
    /// </summary>
    [SerializeField]
    private GameObject carPrefab;

    /// <summary>
    /// All cars will spawn there
    /// </summary>
    
    // Serialize private fields
    [SerializeField]
    private Transform spawnpoint;

    /// <summary>
    /// Currently simulated cars, used for tracking and pooling
    /// </summary>
    private List<InputBrain> spawnedCars;

    /// <summary>
    /// Last best Neural Network
    /// </summary>
    private NeuralNetwork lastBestNN;

    /// <summary>
    /// Controller of the main camera, used to focus the best car
    /// </summary>
    private CameraController cc;

    /// <summary>
    /// Numbers of sensors the cars have
    /// </summary>
    [Range(1, 10)]
    public int sensorCount;


    /// <summary>
    /// Current generation number
    /// </summary>
    [SerializeField]
    private int generation = 0;

    /// <summary>
    /// Probability of mutation for weights and biases
    /// </summary>
    [SerializeField, Range(0, 100)]
    private int mutationProbability;

    /// <summary>
    /// Amount of mutation for weights and biases
    /// </summary>
    [SerializeField]
    private float mutationAmount;

    [SerializeField]
    private float startWeightRandomnessRange = 0.5f;
    [SerializeField]
    private float startBiasRandomnessRange = 0.5f;
    /// <summary>
    /// If the car doesn't capture checkpoints fast enough, it will be desactivated
    /// </summary>
    public float maxTimeBetweenCheckPoints = 0.7f;

    /// <summary>
    /// Max distance for sensors, it will clamp to this number
    /// </summary>
    public float maxSight = 20f;

    /// <summary>
    /// Get an instance of a Neural Network layout
    /// </summary>
    /// <returns></returns>
    private int[] GetLayout()
    {
        return new int[] { sensorCount, Mathf.FloorToInt((float)sensorCount * 0.7f), 2 };
    }

    /// <summary>
    /// Get an instance of a Neural Network
    /// </summary>
    /// <returns></returns>
    private NeuralNetwork InstanciateNN()
    {
        return new NeuralNetwork(GetLayout(), startWeightRandomnessRange, startBiasRandomnessRange);
    }

    // At the beginning of the scene
    private void Start()
    {
        // This network structure is arbitrary
        lastBestNN = InstanciateNN();
        spawnedCars = new List<InputBrain>();

        // For performance
        cc = CameraController.i;
    }

    /// <summary>
    /// Tells if the pool was spawned at least once
    /// </summary>
    bool pooled = false;

    /// <summary>
    /// Spawn a batch of cars
    /// </summary>
    /// <param name="count">Number of cars to spawn at each batch (generation)</param>
    private void SpawnBatch(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = null;
            if (!pooled)
                go = Instantiate(carPrefab, spawnpoint.position, Quaternion.identity, spawnpoint);
            else
            {
                go = spawnedCars[i].gameObject;
                go.transform.position = spawnpoint.position;
                go.transform.rotation = Quaternion.identity;
                go.GetComponent<InputBrain>().Init();
            }
            go.name = string.Concat("Generation_", generation.ToString(), "_ID_", i.ToString());

            NeuralNetwork nn = InstanciateNN();
            // Copy the best NN
            lastBestNN.DeepCopy(nn);
            // Mutate it
            nn.Mutate(mutationProbability, mutationAmount);

            var ib = go.GetComponent<InputBrain>();

            // Update the brain with the new NN
            ib.NN = nn;
            spawnedCars.Add(ib);
            ib.activated = true;
        }
        generation++;

        if (!pooled)
            pooled = true;
    }


    /// <summary>
    /// Tells if the batch is dead
    /// </summary>
    /// <returns></returns>
    bool BatchDesactivated()
    {
        foreach (var item in spawnedCars)
        {
            if (item.activated)
                return false;
        }
        return true;
    }

    /// <summary>
    /// End the batch, keep the best NN, restart with mutations
    /// </summary>
    void EndBatch()
    {
        NeuralNetwork best = lastBestNN;
        int maxFitness = lastBestNN.Fitness;
        foreach (var item in spawnedCars)
        {
            if (item.NN.Fitness > maxFitness)
            {
                best = item.NN;
                maxFitness = item.NN.Fitness;
            }
        }
        if (best == null)
            return;
        if (lastBestNN.Fitness != maxFitness)
            Debug.LogWarning("New Record : " + best.Fitness.ToString());

        lastBestNN = best;
    }

    /// <summary>
    /// Cars per generation
    /// </summary>
    public int populationAmount = 10;

    void Update()
    {
        if (spawnedCars != null)
        {
            if (BatchDesactivated())
            {
                EndBatch();
                SpawnBatch(populationAmount);
            }
            else
            {
                int maxFitness = 0;
                InputBrain bestBrain = spawnedCars[0];
                foreach (var item in spawnedCars)
                {
                    item.Focus = false;
                    if (item.NN.Fitness > maxFitness)
                    {
                        maxFitness = item.NN.Fitness;
                        bestBrain = item;
                    }
                }

                // Update the UI and Camera
                NeuralNetworkUI.i.SetNeuralNetwork(bestBrain.NN, generation);
                cc.Target = bestBrain.transform.position;

                // Tells the car it is focused by the game
                bestBrain.Focus = true;
            }
        }
    }

    /// <summary>
    /// Wrapper for saving a NN
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="content"></param>
    public void SaveToFile(string filename, NeuralNetwork content)
    {
        content.Save(GetPath(filename));
    }

    /// <summary>
    /// Load a NN from a file and put it in the next generation
    /// </summary>
    /// <param name="path">Absolute path</param>
    public void LoadFromFile(string path)
    {
        EndBatch();
        Debug.Log("Loaded " + path);
        lastBestNN.Load(path);
    }

    /// <summary>
    /// Wrapper for getting the absolute path
    /// </summary>
    /// <param name="filename">Relative filename</param>
    /// <returns></returns>
    public string GetPath(string filename)
    {
        return string.Concat(Application.dataPath, "/Data/", filename);
    }

    /// <summary>
    /// Used in the formula
    /// </summary>
    static float trackSize = 2f;

    /// <summary>
    /// Guassian Fonction that exponentially rewards cars that are the closest to the center
    /// </summary>
    /// <param name="x">Variable</param>
    /// <param name="a">Width</param>
    /// <param name="b">Height</param>
    /// <returns></returns>
    static float DistanceFormula(float x, float a, float b)
    {
        return b * Mathf.Exp(-a * (x * x));
    }

    /// <summary>
    /// Compute the score for the captured checkpoint (arbitraty)
    /// </summary>
    /// <param name="points">Points given by the checkpoint by default</param>
    /// <param name="distance">Distance from the center of the track</param>
    /// <param name="time">Time since last checkpoint</param>
    /// <param name="hits">Hits since last checkpoint</param>
    /// <returns></returns>
    public static int ComputeScore(int points, float distance, float time, int hits)
    {
        int distanceScore = Mathf.Clamp(Mathf.CeilToInt((float)points * DistanceFormula(distance, trackSize, 10f)), 0, 1000) * 3;
        int timeScore = Mathf.Clamp(Mathf.RoundToInt(1f / time), 0, 100) * 3;
        int hitPenalty = hits * 100;
        // Debug.Log(distance + " " + distanceScore.ToString() + "/" + time + " " + timeScore.ToString() + "/" +hits + " " + hitPenalty.ToString());
        return distanceScore + timeScore - hitPenalty;
    }
}
