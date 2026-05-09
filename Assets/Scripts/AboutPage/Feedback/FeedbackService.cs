using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FeedbackService : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private string webAppURL;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    [Header("Settings")]
    [SerializeField] private int requestTimeout = 10;

    private bool isSending;

    public IEnumerator SendFeedback(
        FeedbackResponse response,
        Action<bool> onComplete = null)
    {
        if (isSending)
        {
            Debug.LogWarning(
                "[FeedbackService] Request already in progress.");

            yield break;
        }

        if (string.IsNullOrWhiteSpace(webAppURL))
        {
            Debug.LogError(
                "[FeedbackService] Web App URL is empty.");

            onComplete?.Invoke(false);
            yield break;
        }

        if (Application.internetReachability ==
            NetworkReachability.NotReachable)
        {
            Debug.LogError(
                "[FeedbackService] No internet connection.");

            onComplete?.Invoke(false);
            yield break;
        }

        if (response == null)
        {
            Debug.LogError(
                "[FeedbackService] Response is NULL.");

            onComplete?.Invoke(false);
            yield break;
        }

        isSending = true;

        string json = JsonUtility.ToJson(response, true);

        if (enableLogs)
        {
            Debug.Log("[FeedbackService] Sending JSON:");
            Debug.Log(json);
        }

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request =
               new UnityWebRequest(webAppURL, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler =
                new UploadHandlerRaw(bodyRaw);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json");

            request.timeout = requestTimeout;

            yield return request.SendWebRequest();

            bool success =
                request.result ==
                UnityWebRequest.Result.Success &&
                request.responseCode == 200;

            if (success)
            {
                if (enableLogs)
                {
                    Debug.Log(
                        "[FeedbackService] Feedback sent successfully.");

                    Debug.Log(
                        $"[FeedbackService] Response: {request.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError(
                    $"[FeedbackService] Failed: {request.error}");

                Debug.LogError(
                    $"[FeedbackService] Response Code: {request.responseCode}");

                Debug.LogError(
                    $"[FeedbackService] Response: {request.downloadHandler.text}");
            }

            isSending = false;

            onComplete?.Invoke(success);
        }
    }
}