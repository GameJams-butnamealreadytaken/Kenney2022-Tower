using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void OnRestart()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}
