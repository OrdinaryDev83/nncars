using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBrain : MonoBehaviour
{
    /// <summary>
    /// Score text displayed while focused on the car
    /// </summary>
    public TMPro.TextMeshProUGUI scoreText;

    /// <summary>
    /// Car controller
    /// </summary>
    private CarController cc;

    /// <summary>
    /// Checkpoints captured
    /// </summary>
    [SerializeField]
    private List<CheckPoint> captured;

    /// <summary>
    /// Actual brain
    /// </summary>
    public NeuralNetwork NN
    {
        get
        {
            return nn;
        }
        set
        {
            nn = value;
        }
    }
    private NeuralNetwork nn;

    /// <summary>
    /// Tells if it still valid
    /// </summary>
    public bool activated = true;

    private void Start()
    {
        cc = GetComponent<CarController>();
        captured = new List<CheckPoint>();
        Init();
    }

    public void Init()
    {
        captured.Clear();
        timeSinceLastCP = 0f;
        focusTime = 0f;
        focus = false;
        activated = true;
        hitsSinceLastCP = 0;
    }

    int hitsSinceLastCP;
    // Remove points if collision with wall
    private void OnCollisionEnter2D(Collision2D collision)
    {
        hitsSinceLastCP++;
    }

    private bool focus = false;
    /// <summary>
    /// Tells if the car is focused by the game
    /// </summary>
    public bool Focus
    {
        get
        {
            return focus;
        }
        set
        {
            focus = value;

            // Update the text
            scoreText.text = NN.Fitness.ToString();
        }
    }

    /// <summary>
    /// Tells the car it has captured a checkpoint
    /// </summary>
    /// <param name="p">Checkpoint instance</param>
    /// <param name="distance">Distance from the center of the checkpoint, therefore the center of the track</param>
    public void CaptureCheckPoint(CheckPoint p, float distance)
    {
        if (!activated || captured.Contains(p))
            return;

        NN.Fitness += CarManager.ComputeScore(p.points, distance, timeSinceLastCP, hitsSinceLastCP);

        // Add the checkpoint to captured checkpoints
        captured.Add(p);

        // Reset some stats for checkpoint checking
        timeSinceLastCP = 0f;
        hitsSinceLastCP = 0;
    }

    /// <summary>
    /// Tells the car it has finished the track, saves its brain to a file
    /// </summary>
    public void Finished()
    {
        CarManager.i.SaveToFile(string.Concat("NN_SensorCount_", CarManager.i.sensorCount, "_Fitness_", NN.Fitness.ToString(), "_", gameObject.name, ".nn"), NN);
    }

    /// <summary>
    /// Time since last checkpoint
    /// </summary>
    float timeSinceLastCP;
    /// <summary>
    /// Time focused
    /// </summary>
    float focusTime = 0f;
    void Update()
    {
        if (focus)
        {
            if (focusTime < 1f)
            {
                focusTime += Time.deltaTime * 5f;
            }
            else if (focusTime > 1f)
                focusTime = 1f;
        }
        else
        {
            if (focusTime > 0f)
            {
                focusTime -= Time.deltaTime * 5f;
            }
            else if (focusTime < 0f)
                focusTime = 0f;
        }
        // Fade in or out if focused
        scoreText.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, focusTime);

        if (!activated || NN == null)
            return;
        
        // Move the car according to the result of the NN
        var result = NN.FeedForward(cc.GetSensors());
        cc.Move(result[0], result[1]);

        // Swap the text if upside down
        int sign = (int)Mathf.Sign(Vector2.Dot(transform.right, Vector2.right));
        scoreText.transform.localScale = new Vector3(sign, sign, 1f);

        // Increase time
        timeSinceLastCP += Time.deltaTime;
        if (timeSinceLastCP > CarManager.i.maxTimeBetweenCheckPoints)
        {
            // Kill if too slow
            activated = false;
        }
    }
}
