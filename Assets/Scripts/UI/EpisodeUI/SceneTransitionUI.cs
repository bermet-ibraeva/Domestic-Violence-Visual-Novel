using System.Collections;
using TMPro;
using UnityEngine;

public class SceneTransitionUI : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Texts")]
    [SerializeField] private TMP_Text sceneLabelText;

    [SerializeField] private TMP_Text sceneTitleText;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    [SerializeField] private float holdDuration = 0.5f;

    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Show(
        int sceneIndex,
        string sceneTitleKey)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine =
            StartCoroutine(
                ShowRoutine(sceneIndex, sceneTitleKey));
    }

    private IEnumerator ShowRoutine(
        int sceneIndex,
        string sceneTitleKey)
    {
        string sceneLabel = $"Scene {sceneIndex}";

        if (LocalizationManager.Instance != null)
        {
            string template =
                LocalizationManager.Instance.GetText(
                    "Episode",
                    "scene");

            sceneLabel =
                string.Format(template, sceneIndex);
        }

        if (sceneLabelText != null)
        {
            sceneLabelText.text = sceneLabel;
        }

        if (sceneTitleText != null)
        {
            sceneTitleText.text =
                LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText(
                    "Episode",
                    sceneTitleKey)
                : sceneTitleKey;
        }

        // Fade In
        yield return Fade(0f, 1f);

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade Out
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(
        float from,
        float to)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t = time / fadeDuration;

            if (canvasGroup != null)
            {
                canvasGroup.alpha =
                    Mathf.SmoothStep(from, to, t);

                canvasGroup.blocksRaycasts =
                    canvasGroup.alpha > 0.5f;
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = to;

            canvasGroup.blocksRaycasts =
                to > 0.5f;
        }
    }
}