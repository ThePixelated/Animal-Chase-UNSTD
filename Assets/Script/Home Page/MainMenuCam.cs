
using System.Collections;
using UnityEngine;

public class MainMenuCam : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform maincam;
    [SerializeField] private Transform runCam;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform centerLineRunPivot;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 2f;

    // Smooth start and end
    [SerializeField]
    private AnimationCurve transitionCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine transitionRoutine;

    public void PlayInnitiate()
    {
        // Prevent multiple transitions
        if (transitionRoutine != null)
            return;

        transitionRoutine = StartCoroutine(ShortCutScene());
    }

    private IEnumerator ShortCutScene()
    {
        // Validate references
        if (maincam == null || runCam == null ||
            player == null || centerLineRunPivot == null)
        {
            Debug.LogError("MainMenuCam: Missing Transform reference!");
            transitionRoutine = null;
            yield break;
        }

        // Capture initial positions
        Vector3 camStartPos = maincam.position;
        Quaternion camStartRot = maincam.rotation;

        Vector3 playerStartPos = player.position;
        Quaternion playerStartRot = player.rotation;

        // Capture target positions
        Vector3 camTargetPos = runCam.position;
        Quaternion camTargetRot = runCam.rotation;

        Vector3 playerTargetPos = centerLineRunPivot.position;
        Quaternion playerTargetRot = centerLineRunPivot.rotation;

        float elapsed = 0f;
        float duration = Mathf.Max(transitionDuration, 0.01f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Normalized progress (0 - 1)
            float t = Mathf.Clamp01(elapsed / duration);

            // Apply easing
            float smoothT = transitionCurve.Evaluate(t);

            // Camera movement
            maincam.position = Vector3.Lerp(
                camStartPos, camTargetPos, smoothT
            );

            maincam.rotation = Quaternion.Slerp(
                camStartRot, camTargetRot, smoothT
            );

            // Player movement
            player.position = Vector3.Lerp(
                playerStartPos, playerTargetPos, smoothT
            );

            player.rotation = Quaternion.Slerp(
                playerStartRot, playerTargetRot, smoothT
            );

            yield return null;
        }

        // Ensure exact final transforms
        maincam.SetPositionAndRotation(
            camTargetPos, camTargetRot
        );

        player.SetPositionAndRotation(
            playerTargetPos, playerTargetRot
        );

        // Both have reached their targets
        transitionRoutine = null;
        GameManager.StartGame();
    }
}
