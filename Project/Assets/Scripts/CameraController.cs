using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Singleton Pattern
    public static CameraController i = null;

    private void Awake()
    {
        i = this;
    }

    // The Target is set by the CarManager
    public Vector2 Target
    {
        set
        {
            target = value;
        }
        get
        {
            return target;
        }
    }

    private Vector2 target;

    public float cameraSmoothness = 6f;

    private void LateUpdate()
    {
        // Vector3.Slerp is a circular Lerp
        transform.position = Vector3.Slerp(transform.position, new Vector3(target.x, target.y, -10f), Time.deltaTime * cameraSmoothness);
    }
}
