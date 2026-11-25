using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private GameObject homeHUD;
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject gameOverHUD;
    [SerializeField] private GameObject mainMenuObj;

    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI journeyGameplayTxt;

    [Header("Gameover UI")]
    [SerializeField] private TextMeshProUGUI journeyEndTxt;


    private void OnEnable()
    {
        GameManager.onGameStart += DisableHomeHUD;
        GameManager.onGameEnd += EnableGameOverHUD;
    }

    private void OnDisable()
    {
        GameManager.onGameStart -= DisableHomeHUD;
        GameManager.onGameEnd -= EnableGameOverHUD;
    }

    private void DisableHomeHUD()
    {
        //homeHUD.SetActive(false);
        mainMenuObj.SetActive(false);
        gameplayHUD.SetActive(true);
    }

    private void EnableGameOverHUD()
    {
        gameOverHUD.SetActive(true);
        journeyEndTxt.text = PlayerManager.JourneyPlayer.ToString();
    }

    private void Update()
    {
        journeyGameplayTxt.text = PlayerManager.JourneyPlayer.ToString();
    }
}
