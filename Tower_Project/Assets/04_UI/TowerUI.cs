using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TowerUI : MonoBehaviour
{
    public TMP_Text BlockCount;

    public void SetBlockCount(int blockCount)
    {
        // 0 leads to an empty string to hide the text at game start
        BlockCount.text = blockCount == 0 ? "" : blockCount.ToString();
    }
}
