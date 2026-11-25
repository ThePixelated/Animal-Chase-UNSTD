using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static float JourneyPlayer { get; set; } = 0f;

    [SerializeField] private Animator playerAnim;
    [SerializeField] private float calcJourneyPoint = 1f;
    private float _elapsedJourney;

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

    private void PlayAnim() => playerAnim.enabled = true;
    private void EndAnim() => playerAnim.enabled = false;

    private void Update()
    {
        if (GameManager.CurrentGameState == GameState.PLAYING)
        {
            _elapsedJourney += calcJourneyPoint * Time.deltaTime;
            JourneyPlayer = Mathf.RoundToInt(_elapsedJourney / 1f);
        }
    }
}
