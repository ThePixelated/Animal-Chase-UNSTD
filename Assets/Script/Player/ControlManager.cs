
using UnityEngine;
using Lean.Touch;

public class ControlManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float minSwipeDistance = 50f;

    private void OnEnable()
    {
        LeanTouch.OnFingerSwipe += HandleSwipe;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerSwipe -= HandleSwipe;
    }

    private void HandleSwipe(LeanFinger finger)
    {
        if (GameManager.CurrentGameState != GameState.PLAYING)
            return;

        Vector2 swipe = finger.SwipeScreenDelta;

        if (swipe.magnitude < minSwipeDistance)
            return;

        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            // Horizontal
            playerMovement.ChangeLane(
                swipe.x > 0 ? 1 : -1
            );
        }
        else
        {
            // Vertical
            if (swipe.y > 0)
            {
                playerMovement.Jump();
            }
            else
            {
                playerMovement.FastFall();
            }
        }
    }
}
