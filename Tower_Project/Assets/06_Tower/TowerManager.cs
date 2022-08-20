using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    // Blocks
    public GameObject BlockPrefab = null;
    public GameObject InitialBlock = null;
    private GameObject _previousBlock = null;
    private GameObject _currentBlock = null;

    // Block attributes
    public float BlockBaseSpeed = 10.0f;
    public float BlockSpeedFactor = 1.0f;
    public float BlockMatchTolerance = 0.3f;
    private float _blockYHeight;
    private Vector3 _blockScale = Vector3.one;

    // Camera
    public GameObject CameraPlayer;

    // Inputs
    private bool _blockStop = false;

    // Game
    private bool _isGameOver = false;

    // Bonus
    private int _perfectStrike = 0;
    private int _bestPerfectStrike = 0;

    private void Start()
    {
        _previousBlock = InitialBlock;
        _blockScale = InitialBlock.transform.localScale;

        _blockYHeight = _previousBlock.GetComponent<BoxCollider>().size.y;

        SpawnBlock();
    }

    private void Update()
    {
        if (_isGameOver)
            return;

        if (_blockStop)
        {
            _blockStop = false;

            float xDiff = _currentBlock.transform.position.x - _previousBlock.transform.position.x;
            float zDiff = _currentBlock.transform.position.z - _previousBlock.transform.position.z;

            if (Mathf.Abs(xDiff) >= _currentBlock.transform.localScale.x
                || Mathf.Abs(zDiff) >= _currentBlock.transform.localScale.z)
            {
                // Game over, too far
                OnGameOver();
                return;
            }

            Vector3 newPos = _currentBlock.transform.position;
            Vector3 newScale = _currentBlock.transform.localScale;

            bool perfectHit = true;

            // X axis
            if (Mathf.Abs(xDiff) <= BlockMatchTolerance)
            {
                // Close enough, give it to the player
                newPos.x = _previousBlock.transform.position.x;
                xDiff = 0.0f;
            }
            else
            {
                // In the range, scale the current block
                newScale.x -= Mathf.Abs(xDiff);

                if (xDiff < 0.0f)
                {
                    newPos.x -= xDiff;
                }

                perfectHit = false;
            }

            // Z axis
            if (Mathf.Abs(zDiff) <= BlockMatchTolerance)
            {
                // Close enough, give it to the player
                newPos.z = _previousBlock.transform.position.z;
                zDiff = 0.0f;
            }
            else
            {
                // In the range, scale the current block
                newScale.z -= Mathf.Abs(zDiff);

                if (zDiff < 0.0f)
                {
                    newPos.z -= zDiff;
                }

                perfectHit = false;
            }

            _currentBlock.transform.position = newPos;
            _currentBlock.transform.localScale = newScale;

            // Update next block scale based on new scale of current block
            _blockScale = newScale;

            if (perfectHit)
            {
                OnPerfectHit();
            }
            else
            {
                OnFailHit();

                // Spawn cut block
                newPos.x += xDiff;
                newPos.z += zDiff;

                GameObject cutBlock = Instantiate(BlockPrefab, newPos, Quaternion.identity);

                if (xDiff != 0.0f)
                    newScale.x = _previousBlock.transform.localScale.x - newScale.x;
                else
                    newScale.z = _previousBlock.transform.localScale.z - newScale.z;
                cutBlock.transform.localScale = newScale;

                cutBlock.GetComponent<Rigidbody>().isKinematic = false;
            }

            _previousBlock = _currentBlock;

            SpawnBlock();
        }
        else
        {
            // Move block
            if (_currentBlock)
            {
                _currentBlock.GetComponent<BlockController>().UpdateLocation(BlockBaseSpeed * BlockSpeedFactor * Time.deltaTime);
            }
        }
    }

    private void OnPerfectHit()
    {
        //TODO handle perfect strike & co
        ++_perfectStrike;
        Debug.Log("Perfect " + _perfectStrike);

        if (_perfectStrike % 3 == 0)
        {
            BlockSpeedFactor += 0.1f;

            Vector3 newScale = _currentBlock.transform.localScale;
            newScale.x += 0.2f;
            newScale.z += 0.2f;
            
            _currentBlock.transform.localScale = newScale;
            _blockScale = newScale;
        }
    }

    private void OnFailHit()
    {
        _bestPerfectStrike = Mathf.Max(_bestPerfectStrike, _perfectStrike);
        _perfectStrike = 0;
        BlockSpeedFactor = 1.0f;
    }

    private void OnGameOver()
    {
        _isGameOver = true;

        OnFailHit();

        //TODO dézoom caméra
        Debug.Log("Game over");
        _currentBlock.GetComponent<Rigidbody>().isKinematic = false;

        GameManager.Instance.OnGameOver();
    }

    private void SpawnBlock()
    {
        EBlockDirection previousDirection = _previousBlock.GetComponent<BlockController>().BlockDireciton;

        // Spawn location
        Vector3 newPosition = _previousBlock.transform.position;
        if (previousDirection == EBlockDirection.Left)
            newPosition.x = -10.0f;
        else
            newPosition.z = 10.0f;
        newPosition.y += _blockYHeight;

        // Create object
        _currentBlock = Instantiate(BlockPrefab, newPosition, Quaternion.identity);

        // Scale
        _currentBlock.transform.localScale = _blockScale;

        // Set block direction
        _currentBlock.GetComponent<BlockController>().BlockDireciton = previousDirection == EBlockDirection.Left ? EBlockDirection.Right : EBlockDirection.Left;

        // Update camera height
        newPosition = CameraPlayer.transform.position;
        newPosition.y += _blockYHeight;
        CameraPlayer.transform.position = newPosition;
    }

    public void OnBlockStop()
    {
        _blockStop = _currentBlock != null;
    }
}
