using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// DISCLAIMER : This file is not optimized at all and is for test purposes only

public class NeuralNetworkUI : MonoBehaviour
{
    public static NeuralNetworkUI i = null;
    private void Awake()
    {
        i = this;
    }

    Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public TextMeshProUGUI title;

    public GameObject layerPrefab;
    public GameObject neuronPrefab;
    public GameObject linePrefab;

    public Transform layersRoot;
    public Transform neuronsRoot;
    public Transform linesRoot;

    Image[][] neurons;

    Image[][][] lines;

    NeuralNetwork r;
    public void SetNeuralNetwork(NeuralNetwork nn, int generation)
    {
        title.text = "Generation " + generation.ToString();
        if (r != null)
        {
            r = nn;
            return;
        }

        Vector2 size = layersRoot.GetComponent<RectTransform>().sizeDelta;;
        float padding = 15f;
        float neuronSize = 50f + padding;

        List<Image[]> a_ = new List<Image[]>();
        for (int i = 0; i < nn.Layers.Length; i++)
        {
            List<Image> b_ = new List<Image>();
            var la = (GameObject)Instantiate(layerPrefab, layersRoot);

            Vector2 layerPos = size / 2f + (((float)(i + 0.5f) - ((float)nn.Layers.Length / 2f)) / (float)nn.Layers.Length) * Vector2.right * size.x;

            la.GetComponent<RectTransform>().anchoredPosition = layerPos;
            la.GetComponent<RectTransform>().sizeDelta = new Vector3(neuronSize, size.y - padding, 1f);

            for (int j = 0; j < nn.Layers[i]; j++)
            {
                var ne = (GameObject)Instantiate(neuronPrefab, neuronsRoot, false);

                float y = (((float)(j + 0.5f) - ((float)nn.Layers[i] / 2f)) / (float)nn.Layers[i]) * size.y;

                ne.GetComponent<RectTransform>().anchoredPosition = layerPos + Vector2.up * y;
                ne.name = "Neuron " + i.ToString() + " " + j.ToString();
                b_.Add(ne.GetComponent<Image>());
            }
            a_.Add(b_.ToArray());
        }
        neurons = a_.ToArray();

        List<Image[][]> weightsList = new List<Image[][]>();
        for (int i = 1; i < nn.Layers.Length; i++)
        {
            List<Image[]> layerWeightsList = new List<Image[]>();
            int neuronsInPreviousLayer = nn.Layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)
            {
                Image[] neuronWeights = new Image[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    RectTransform before = neurons[i - 1][k].GetComponent<RectTransform>();
                    RectTransform after = neurons[i][j].GetComponent<RectTransform>();

                    var line = Instantiate(linePrefab, linesRoot);
                    var rl = line.GetComponent<RectTransform>();

                    rl.localPosition = (after.localPosition + before.localPosition) / 2;
                    Vector3 dif = after.localPosition - before.localPosition;
                    rl.sizeDelta = new Vector3(dif.magnitude, 5);
                    rl.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));

                    neuronWeights[k] = line.GetComponent<Image>();
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        lines = weightsList.ToArray();
        r = nn;
    }

    void Update()
    {
        if (r != null)
        {
            for (int i = 0; i < r.Neurons.Length; i++)
            {
                for (int j = 0; j < r.Neurons[i].Length; j++)
                {
                    neurons[i][j].color = Color.Lerp(Color.black, Color.white, r.Neurons[i][j]);
                    var t0 = neurons[i][j].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    var t1 = neurons[i][j].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    var t2 = neurons[i][j].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                    t0.text = System.Math.Round(r.Neurons[i][j], 2).ToString();
                    t0.color = Color.Lerp(Color.black, Color.white, 1f - r.Neurons[i][j]);


                    if (i == 0)
                    {
                        t1.gameObject.SetActive(true);
                        t1.text = "Sensor " + (j + 1).ToString();
                    }
                    else if (i == r.Neurons.Length - 1)
                    {
                        t2.gameObject.SetActive(true);
                        if (j == 0)
                        {
                            t2.text = "Acceleration";
                        }
                        else if (j == 1)
                        {
                            t2.text = "Steering";
                        }
                    }
                }
            }
            for (int i = 0; i < r.Weights.Length; i++)
            {
                for (int j = 0; j < r.Weights[i].Length; j++)
                {
                    for (int k = 0; k < r.Weights[i][j].Length; k++)
                    {
                        lines[i][j][k].color = r.Weights[i][j][k] > 0f ? Color.green : Color.red;
                    }
                }
            }
        }
    }
}
