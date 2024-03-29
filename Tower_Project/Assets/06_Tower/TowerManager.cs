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

    // Bonus
    public List<GameObject> BonusBlocksBase;
    public List<GameObject> BonusBlocksSpecial;

    // UI
    public TowerUI TowerUI;

    // Camera
    public GameObject CameraPlayer;
    private Vector3 _cameraBaseOffset;
    private Vector3 _cameraTargetPos;

    // Inputs
    private bool _blockStop = false;

    // Game
    private bool _isGameOver = false;

    // Stats
    private int _blockCount = 0;
    private int _perfectStrike = 0;
    private int _bestPerfectStrike = 0;

    // Audio
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        IncrementBlockCount(true);

        _previousBlock = InitialBlock;
        _blockScale = InitialBlock.transform.localScale;

        _blockYHeight = _previousBlock.GetComponentInChildren<BoxCollider>().size.y;

        _cameraBaseOffset = CameraPlayer.transform.localPosition;

        SpawnBlock();
    }

    private void Update()
    {
        if (_isGameOver)
            return;

        if (_blockStop)
        {
            _blockStop = false;

            if (_currentBlock.GetComponent<BlockController>().IsBonusBlock)
            {
                StopBonusBlock();
            }
            else
            {
                StopBlock();
            }
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

    private void LateUpdate()
    {
        Vector3 velocity = Vector3.zero;
        CameraPlayer.transform.position = Vector3.SmoothDamp(CameraPlayer.transform.position, _cameraTargetPos, ref velocity, 0.05f);
    }

    private void StopBlock()
    {
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
            newPos.x -= xDiff * 0.5f;
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
            newPos.z -= zDiff * 0.5f;
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
            {
                if (xDiff != 0.0f)
                {
                    if (xDiff > 0.0f)
                    {
                        newPos.x += (_currentBlock.transform.localScale.x * 0.5f) + (xDiff * 0.5f);
                    }
                    else
                    {
                        newPos.x -= (_currentBlock.transform.localScale.x * 0.5f) - (xDiff * 0.5f);
                    }
                    newScale.x = _previousBlock.transform.localScale.x - newScale.x;
                }
                else
                {
                    if (zDiff > 0.0f)
                    {
                        newPos.z += (_currentBlock.transform.localScale.z * 0.5f) + (zDiff * 0.5f);
                    }
                    else
                    {
                        newPos.z -= (_currentBlock.transform.localScale.z * 0.5f) - (zDiff * 0.5f);
                    }
                    newScale.z = _previousBlock.transform.localScale.z - newScale.z;
                }

                GameObject cutBlock = Instantiate(BlockPrefab, newPos, Quaternion.identity);
                cutBlock.transform.localScale = newScale;

                cutBlock.GetComponent<BlockController>().BlockDirection = _currentBlock.GetComponent<BlockController>().BlockDirection;
                cutBlock.GetComponent<BlockController>().ActivatePhysics(xDiff != 0.0f ? xDiff : zDiff);
            }
        }

        _previousBlock = _currentBlock;

        IncrementBlockCount(false);

        if (perfectHit && CanSpawnBonus())
        {
            SpawnBonusBlock();
        }
        else
        {
            SpawnBlock();
        }
    }

    private void StopBonusBlock()
    {
        float xDiff = _currentBlock.transform.position.x - _previousBlock.transform.position.x;
        float zDiff = _currentBlock.transform.position.z - _previousBlock.transform.position.z;
        Vector3 bonusColliderSize = _currentBlock.GetComponent<BoxCollider>().size;

        if (Mathf.Abs(xDiff) < (_previousBlock.transform.localScale.x * 0.5f - bonusColliderSize.x * 0.5f)
            || Mathf.Abs(zDiff) < _previousBlock.transform.localScale.z * 0.5f - bonusColliderSize.z * 0.5f)
        {
            _audioSource.PlayOneShot(SoundManager.Instance.GetPerfectHitSound());
        }
        else
        {
            _audioSource.PlayOneShot(SoundManager.Instance.GetFailHitsound());
            _currentBlock.GetComponent<BlockController>().ActivatePhysics(1.0f);
        }

        SpawnBlock();
    }

    private void OnPerfectHit()
    {
        ++_perfectStrike;

        if (_perfectStrike % 3 == 0)
        {
            BlockSpeedFactor += 0.1f;

            Vector3 newScale = _currentBlock.transform.localScale;
            newScale.x += 0.4f;
            newScale.z += 0.4f;
            
            _currentBlock.transform.localScale = newScale;
            _blockScale = newScale;
        }

        _audioSource.PlayOneShot(SoundManager.Instance.GetPerfectHitSound());
    }

    private void OnFailHit()
    {
        _bestPerfectStrike = Mathf.Max(_bestPerfectStrike, _perfectStrike);
        _perfectStrike = 0;
        BlockSpeedFactor = 1.0f;

        _audioSource.PlayOneShot(SoundManager.Instance.GetFailHitsound());
    }

    private void OnGameOver()
    {
        _isGameOver = true;

        OnFailHit();

        _currentBlock.GetComponent<BlockController>().ActivatePhysics(1.0f);

        _cameraTargetPos = CameraPlayer.transform.position;
        _cameraTargetPos.x += _blockCount * 0.5f;
        _cameraTargetPos.z -= _blockCount * 0.5f;

        GameManager.Instance.OnGameOver();
    }

    private void SpawnBlock()
    {
        EBlockDirection previousDirection = _previousBlock.GetComponent<BlockController>().BlockDirection;

        // Spawn location
        Vector3 newPosition = _previousBlock.transform.position;
        if (previousDirection == EBlockDirection.Left)
        {
            newPosition.x -= 10.0f;
        }
        else
        {
            newPosition.z += 10.0f;
        }
        newPosition.y += _blockYHeight;

        // Create object
        _currentBlock = Instantiate(BlockPrefab, newPosition, Quaternion.identity);

        // Scale
        _currentBlock.transform.localScale = _blockScale;

        // Set block direction
        _currentBlock.GetComponent<BlockController>().Initialize(previousDirection == EBlockDirection.Left ? EBlockDirection.Right : EBlockDirection.Left, _previousBlock.transform.position);

        // Update camera height
        _cameraTargetPos = _previousBlock.transform.position + _cameraBaseOffset;
    }

    private bool CanSpawnBonus()
    {
        EBlockDirection previousDirection = _previousBlock.GetComponent<BlockController>().BlockDirection;

        if (previousDirection == EBlockDirection.Left)
        {
            if (_blockScale.z < 0.5f)
                return false;
        }
        else
        {
            if (_blockScale.x < 0.5f)
                return false;
        }

        return true;
    }

    private void SpawnBonusBlock()
    {
        EBlockDirection previousDirection = _previousBlock.GetComponent<BlockController>().BlockDirection;

        // Spawn location, same as previous block
        Vector3 newPosition = _previousBlock.transform.position;
        if (previousDirection == EBlockDirection.Left)
        {
            newPosition.x += _previousBlock.transform.localScale.x * 0.5f;
            newPosition.z += 10.0f;
        }
        else
        {
            newPosition.x -= 10.0f;
            newPosition.z -= _previousBlock.transform.localScale.z * 0.5f;
        }

        // Create object
        GameObject bonusBlock = _perfectStrike % 3 == 0 ? BonusBlocksSpecial[Random.Range(0, BonusBlocksSpecial.Count)] : BonusBlocksBase[Random.Range(0, BonusBlocksBase.Count)];
        _currentBlock = Instantiate(bonusBlock, newPosition, Quaternion.Euler(0.0f, previousDirection == EBlockDirection.Left ? 180.0f : -90.0f, 0.0f));

        _currentBlock.GetComponent<BlockController>().Initialize(previousDirection, _previousBlock.transform.position);

        // Adjust position and scale if needed
        Vector3 newScale = _currentBlock.transform.localScale;
        if (previousDirection == EBlockDirection.Left)
        {
            if (_previousBlock.transform.localScale.z < _currentBlock.GetComponent<BoxCollider>().size.z)
            {
                newScale.z = _previousBlock.transform.localScale.z / _currentBlock.GetComponent<BoxCollider>().size.z;
                newScale.x = newScale.z;
                _currentBlock.transform.localScale = newScale;
            }

            newPosition.x += newScale.x * (_currentBlock.GetComponent<BoxCollider>().size.x * 0.5f);
            _currentBlock.transform.position = newPosition;
        }
        else
        {
            if (_previousBlock.transform.localScale.x < _currentBlock.GetComponent<BoxCollider>().size.z)
            {
                newScale.z = _previousBlock.transform.localScale.x / _currentBlock.GetComponent<BoxCollider>().size.z;
                newScale.x = newScale.z;
                _currentBlock.transform.localScale = newScale;
            }

            newPosition.z -= newScale.z * (_currentBlock.GetComponent<BoxCollider>().size.x * 0.5f);
            _currentBlock.transform.position = newPosition;
        }
    }

    private void IncrementBlockCount(bool reset)
    {
        if (reset)
        {
            _blockCount = 0;
        }
        else
        {
            ++_blockCount;
        }

        TowerUI.SetBlockCount(_blockCount);
    }


    public void OnBlockStop()
    {
        _blockStop = _currentBlock != null;
    }

}
