using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * TODO LIST
 * 
 * Smooth camera moves
 * Petite impulse sur le morceau de block cut pour pas qu'il tombe comme tout droit une pierre
 * Gestion game over avec dézoom sur la tour + bouton pour restart
 * FX
 * UI Menu
 * UI IG score + temps
 * SFX
 * Music
 * 
 * Spawn de bonus (fenêtres/balcon/etc) à placer parfaitement pour les garder sur le block et avoir des points à la fin + scale up
 * 
 * Mode 1v1 (voire 4 en ffa joueur avec splitscreen?)
 * 
*/

public enum EBlockDirection
{
    Left,
    Right
}

public class GameManager : MonoBehaviour
{
    public TowerManager Tower;

    public GameObject GameOverUI;

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
    }

    public void OnGameOver()
    {
        GameOverUI.SetActive(true);
    }

    public void OnStopBlock()
    {
        Tower.OnBlockStop();
    }
}
