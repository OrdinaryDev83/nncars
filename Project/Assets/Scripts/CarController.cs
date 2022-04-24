using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    Rigidbody2D rigid;
    /// <summary>
    /// Prevents the car from sliding when steering (sideways friction)
    /// </summary>*
    [SerializeField]
    private float steerDrag = 0.75f;
    [SerializeField]
    private float steerForce = 0.7f;

    [SerializeField]
    private float forwardForce = 5f;

    public Transform sensorOrigin;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        /// Sideways friction 

        // Local velocity
        Vector2 localVel = transform.InverseTransformDirection(rigid.velocity);

        // Apply Friction exponentially
        localVel.y *= steerDrag;
        Vector2 vel = transform.TransformDirection(localVel);

        // Apply it
        rigid.velocity = vel;
    }

    /// <summary>
    /// Drive the car
    /// </summary>
    /// <param name="forward">Amount forward (0, inf), it cannot go backwards</param>
    /// <param name="steer">Steer the car (-inf, inf), positive is left, negative is right</param>
    public void Move(float forward, float steer)
    {
        if (forward > 0f)
            rigid.AddForce(transform.right * forward * forwardForce, ForceMode2D.Force);
        rigid.AddTorque(-steer * steerForce, ForceMode2D.Force);
    }

    /// <summary>
    /// Return the sensor array
    /// </summary>
    /// <returns>Sensor Array</returns>
    public float[] GetSensors()
    {
        float[] result = new float[CarManager.i.sensorCount];
        for (int i = 1; i <= CarManager.i.sensorCount; i++)
        {
            // Arrange rays in flower by 180°
            float angle = ((float)(i - 1) / (float)(CarManager.i.sensorCount - 1)) * 180f - 90f + transform.eulerAngles.z;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            RaycastHit2D[] hits = Physics2D.RaycastAll(sensorOrigin.position, dir, CarManager.i.maxSight);

            // Touched count
            int c = 0;
            foreach (var item in hits)
            {
                InputBrain bb = null;
                CheckPoint cp = null;

                // Only touch walls
                if (item.collider.TryGetComponent(out bb) || item.collider.TryGetComponent(out cp))
                    continue;
                result[i - 1] = Vector2.Distance(sensorOrigin.position, item.point);
                Debug.DrawLine(sensorOrigin.position, item.point, Color.Lerp(Color.red, Color.green, result[i - 1] / CarManager.i.maxSight));
                c++;
                break;
            }
            if (c == 0)
            {
                result[i - 1] = CarManager.i.maxSight;
            }
        }
        return result;
    }
}
