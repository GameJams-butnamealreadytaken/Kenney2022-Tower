using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public EBlockDirection BlockDireciton;
    private float _directionFactor = 1.0f;

    public void UpdateLocation(float speed)
    {
        if (BlockDireciton == EBlockDirection.Left)
        {
            transform.position = transform.position + new Vector3(0.0f, 0.0f, -speed * _directionFactor);

            if (Mathf.Abs(transform.position.z) > 10.0f)
            {
                _directionFactor *= -1.0f;
            }
        }
        else
        {
            transform.position = transform.position + new Vector3(speed * _directionFactor, 0.0f, 0.0f);
            
            if (Mathf.Abs(transform.position.x) > 10.0f)
            {
                _directionFactor *= -1.0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BlockDestructor"))
        {
            Destroy(gameObject);
        }
    }
}
