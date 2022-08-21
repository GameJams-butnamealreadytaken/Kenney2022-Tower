using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public EBlockDirection BlockDirection;
    private float _directionFactor = 1.0f;

    public bool IsBonusBlock = false;

    private Vector3 _previousBlockPosition;

    private bool _simulatePhysics = false;

    public void Initialize(EBlockDirection direction, Vector3 previousBlockPosition)
    {
        BlockDirection = direction;
        _previousBlockPosition = previousBlockPosition;
    }

    public void UpdateLocation(float speed)
    {
        if (_simulatePhysics)
            return;

        if (BlockDirection == EBlockDirection.Left)
        {
            transform.position = transform.position + new Vector3(0.0f, 0.0f, -speed * _directionFactor);

            if (Mathf.Abs(_previousBlockPosition.z - transform.position.z) > 10.0f)
            {
                _directionFactor *= -1.0f;
            }
        }
        else
        {
            transform.position = transform.position + new Vector3(speed * _directionFactor, 0.0f, 0.0f);
            
            if (Mathf.Abs(_previousBlockPosition.x - transform.position.x) > 10.0f)
            {
                _directionFactor *= -1.0f;
            }
        }
    }

    public void ActivatePhysics(float cutSide)
    {
        _simulatePhysics = true;

        GetComponent<Rigidbody>().isKinematic = false;

        Vector3 impulseDir = Vector3.right;
        if (BlockDirection == EBlockDirection.Left)
        {
            impulseDir = Vector3.forward;
        }

        cutSide = cutSide < 0.0f ? -1.0f : 1.0f;

        GetComponent<Rigidbody>().AddForceAtPosition(impulseDir * (cutSide * 5.0f), transform.position + new Vector3(0.0f, 0.75f, 0.0f), ForceMode.Impulse); ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BlockDestructor"))
        {
            Destroy(gameObject);
        }
    }
}
