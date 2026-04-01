using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button openButton;

    private string quizId;
    private TestsSceneController controller;

    public void Setup(QuizData quiz, int bestScore, TestsSceneController owner)
    {
        quizId = quiz.quizId;
        controller = owner;

        if (titleText != null)
            titleText.text = quiz.title;

        if (progressText != null)
        {
            if (bestScore <= 0)
                progressText.text = "Не пройден";
            else if (bestScore >= quiz.maxReward)
                progressText.text = "Максимум";
            else
                progressText.text = $"{bestScore}/{quiz.maxReward}";
        }

        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OnClickOpen);
        }
    }

    private void OnClickOpen()
    {
        if (controller != null)
            controller.OpenQuiz(quizId);
    }
}