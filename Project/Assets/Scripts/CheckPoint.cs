using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    /// <summary>
    /// Fitness multiplier to the points added when crossing this checkpoint once
    /// </summary>
    public int points;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        InputBrain cc = null;
        if (collision.TryGetComponent(out cc))
        {
            Vector2 dir = (transform.position - collision.transform.position);

            // It needs the direction to reward the most centered car
            cc.CaptureCheckPoint(this, dir.magnitude);
        }
    }
}
