using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class NeuralNetwork
{
    #region Fields
    /// <summary>
    /// Network layout of the different layers from left to right in neuron count
    /// </summary>
    public int[] Layers
    {
        get
        {
            return layers;
        }
    }
    protected int[] layers;

    /// <summary>
    /// Neurons values 2D matrix (layer, id), the x in r = w * x + b
    /// </summary>
    public float[][] Neurons
    {
        get
        {
            return neurons;
        }
    }
    protected float[][] neurons;

    /// <summary>
    /// Neurons values 2D matrix (layer, id), the b in r = w * x + b
    /// </summary>
    public float[][] Biases
    {
        get
        {
            return biases;
        }
    }
    protected float[][] biases;

    /// <summary>
    /// Weights values 3D matrix (layer, left neuron, right neuron), the w in r = w * x + b
    /// </summary>
    public float[][][] Weights
    {
        get
        {
            return weights;
        }
    }
    protected float[][][] weights;

    /// <summary>
    /// How well is the Neural Network doing at its job
    /// </summary>
    public int Fitness
    {
        get;
        set;
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialize a Neural Network
    /// </summary>
    /// <param name="layers">Network layout of the different layers from left to right in neuron count</param>
    /// <param name="weightRange">Random range for weights (-range, range)</param>
    /// <param name="biasesRange">Random range for biases (-range, range)</param>
    public NeuralNetwork(int[] layers, float weightRange, float biasesRange)
    {
        Init(layers, weightRange, biasesRange);
    }
    
    protected virtual void Init(int[] layers, float weightRange, float biasesRange)
    {
        Fitness = 0;

        // Deep copy the layout
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        // Set random values, weights and biases
        InitNeurons();
        InitBiases(biasesRange);
        InitWeights(weightRange);
    }

    protected virtual void InitNeurons()
    {
        List<float[]> temp = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            temp.Add(new float[layers[i]]);
        }
        neurons = temp.ToArray();
    }

    protected virtual void InitBiases(float range)
    {
        List<float[]> temp = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] temp2 = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                temp2[j] = Random.Range(-range, range);
            }
            temp.Add(temp2);
        }
        biases = temp.ToArray();
    }

    protected virtual void InitWeights(float range)
    {
        List<float[][]> temp = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> temp2 = new List<float[]>();
            int nbNeurPrevLay = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] temp3 = new float[nbNeurPrevLay];
                for (int k = 0; k < nbNeurPrevLay; k++)
                {
                    temp3[k] = Random.Range(-range, range);
                }
                temp2.Add(temp3);
            }
            temp.Add(temp2.ToArray());
        }
        weights = temp.ToArray();
    }
    #endregion

    /// <summary>
    /// Activation function
    /// </summary>
    /// <param name="value">X value</param>
    /// <returns></returns>
    protected virtual float Activate(float value)
    {
        // Hyperbolic Tangent defined x : -inf, inf; y : -1 to 1
        return (float)System.Math.Tanh(value);
    }

    /// <summary>
    /// Calculate the outputs from the inputs using the whole network, left to right
    /// </summary>
    /// <param name="inputs">Input values array</param>
    /// <returns></returns>
    public virtual float[] FeedForward(float[] inputs)
    {
        if (inputs.Length != neurons[0].Length)
            throw new System.Exception("Input Array Size different from Input Layer Size");

        // Copy the inputs in the first layer
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        // Apply the formula
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = Activate(value + biases[i][j]);
            }
        }

        // Return the last layer (result)
        return neurons[neurons.Length - 1];
    }

    /// <summary>
    /// Genetic mutation on the network
    /// </summary>
    /// <param name="probability">Probability the mutation happens</param>
    /// <param name="range">Range of the mutation (-range, range)</param>
    public virtual void Mutate(int probability, float range)
    {
        for (int i = 0; i < biases.Length; i++)
            for (int j = 0; j < biases[i].Length; j++)
                biases[i][j] = (Random.Range(0, 101) <= probability) ? biases[i][j] + Random.Range(-range, range) : biases[i][j];

        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    weights[i][j][k] = (Random.Range(0, 101) <= probability) ? weights[i][j][k] + Random.Range(-range, range) : weights[i][j][k];
    }

    /// <summary>
    /// Perform a deep copy on a Neural Network
    /// </summary>
    /// <param name="nn">Destination</param>
    /// <returns></returns>
    public NeuralNetwork DeepCopy(NeuralNetwork nn)
    {
        for (int i = 0; i < biases.Length; i++)
            for (int j = 0; j < biases[i].Length; j++)
                nn.biases[i][j] = biases[i][j];
        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    nn.weights[i][j][k] = weights[i][j][k];
        return nn;
    }


    /// <summary>
    /// Load in this instance a Neural Network according to a text file.
    /// It must be the same network layout
    /// </summary>
    /// <param name="path">Absolute path of the file</param>
    public void Load(string path)
    {
        TextReader tr = new StreamReader(path);

        this.Fitness = int.Parse(tr.ReadLine());

        int NumberOfLines = (int)new FileInfo(path).Length - 1;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Encode this Neural Network into a file
    /// </summary>
    /// <param name="path">Absolute path of the file</param>
    public void Save(string path)
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine(Fitness.ToString());

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }
}