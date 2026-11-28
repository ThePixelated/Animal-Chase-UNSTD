
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static float JourneyPlayer { get; private set; }

    [SerializeField] private Animator playerAnim;
    [SerializeField] private float calcJourneyPoint = 1f;

    private float elapsedJourney;

    private void OnEnable()
    {
        GameManager.onGameStart += PlayAnim;
        GameManager.onGameEnd += EndAnim;
    }

    private void OnDisable()
    {
        GameManager.onGameStart -= PlayAnim;
        GameManager.onGameEnd -= EndAnim;
    }

    private void PlayAnim()
    {
        elapsedJourney = 0f;
        JourneyPlayer = 0f;

        playerAnim.enabled = true;
    }

    private void EndAnim()
    {
        playerAnim.enabled = false;
    }

    private void Update()
    {
        if (GameManager.CurrentGameState != GameState.PLAYING)
            return;

        elapsedJourney += calcJourneyPoint * Time.deltaTime;
        JourneyPlayer = elapsedJourney;
    }
}
