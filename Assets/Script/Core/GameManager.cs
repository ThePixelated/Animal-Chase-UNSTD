using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameState CurrentGameState { get; private set; } = GameState.MAINMENU;
    public static LevelMap CurrentLevelMap { get; private set; } = LevelMap.PLAIN;
    public static Action onGameStart;
    public static void GameStart() => onGameStart?.Invoke();
    public static Action onGameEnd;

    public static void EndGame()
    {
        onGameEnd?.Invoke();
        SetGameState(GameState.ENDGAME);
    }

    public static void StartGame()
    {
        GameStart();
        SetGameState(GameState.PLAYING);
    }

    public void PauseState() { CurrentGameState = GameState.PAUSE; }
    public void PlayState() { GameStart(); CurrentGameState = GameState.PLAYING; }
    public void GoToScene(string targetScene)
    {
        SceneManager.LoadScene(targetScene);
    }
    public void QuitApp()
    {
        Application.Quit();
    }

    public static void SetGameState(GameState targetState)
    {
        CurrentGameState = targetState;
    }
}

public enum GameState
{
    MAINMENU,
    PLAYING,
    ENDGAME,
    PAUSE
}

public enum LevelMap
{
    PLAIN,
    MUSHROOM
}