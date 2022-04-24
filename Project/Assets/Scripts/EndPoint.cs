using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    // Triggers the Finished Method of the Car's Brain on Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        InputBrain cc = null;
        if (collision.TryGetComponent(out cc))
        {
            cc.Finished();
        }
    }
}
