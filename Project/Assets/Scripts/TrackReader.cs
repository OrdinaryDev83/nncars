using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DISCLAIMER : This file is not optimized at all and is for test purposes only

public class TrackReader : MonoBehaviour
{
    public Transform points;
    public Transform root;

    public GameObject wall;

    public float thickness;

    void Start()
    {
        GenerateTrack(points.GetComponentsInChildren<Transform>());
    }

    void GenerateTrack(Transform[] list)
    {
        for (int i = 1; i < list.Length; i++) // skip 0
        {
            Transform a = list[i];
            Transform b = list[i + 1 >= list.Length ? 1 : i + 1];
            var dim = Dims(a, b);

            Vector2 x = a.position;
            Vector2 y = b.position;

            Vector2 mid = dim[0];
            Vector2 size = dim[1];
            float rot = dim[2].x;

            var g = Instantiate(wall, mid, Quaternion.Euler(Vector3.forward * rot), root);
            g.transform.GetChild(0).localScale = new Vector3(1f / size.x, 1f / size.y, 0f) * thickness;
            g.transform.localScale = size;
        }
    }

    Vector2[] Dims(Transform a, Transform b)
    {
        Vector2 mid = Vector2.Lerp(a.position, b.position, 0.5f);

        Vector2 dir = (b.position - a.position);

        Vector2 size = new Vector2(thickness, dir.magnitude);
        Vector2 rotation = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f) * Vector2.right; // r 0

        return new Vector2[] { mid, size, rotation };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var walls0 = points.GetComponentsInChildren<Transform>();
        for (int i = 1; i < walls0.Length; i++)
        {
            Transform a = walls0[i];
            Transform b = walls0[i + 1 >= walls0.Length ? 1 : i + 1];

            Vector2 x = a.position;
            Vector2 y = b.position;

            Gizmos.DrawLine(x, y);
        }
    }
}
