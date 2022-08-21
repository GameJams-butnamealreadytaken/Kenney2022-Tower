using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public List<AudioClip> BlockFailHitSounds;
    public List<AudioClip> BlockPerfectHitSounds;

    private static SoundManager _instance;
    public static SoundManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
    }

    public AudioClip GetFailHitsound()
    {
        return BlockFailHitSounds[Random.Range(0, BlockFailHitSounds.Count - 1)];
    }

    public AudioClip GetPerfectHitSound()
    {
        return BlockPerfectHitSounds[Random.Range(0, BlockPerfectHitSounds.Count - 1)];
    }
}
