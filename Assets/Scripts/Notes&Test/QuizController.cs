using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset quizzesJson;

    [Header("UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TMP_Text[] answerTexts;

    [Header("Result UI")]
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private TMP_Text resultText;

    private SaveData save;
    private QuizDatabase database;
    private QuizData currentQuiz;

    private List<QuestionData> currentQuestions = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;

    private void Start()
    {
        save = SaveSystem.Load();
        LoadDatabase();
        LoadQuiz();

        if (save == null)
        {
            Debug.LogError("[QuizController] SaveData is null");
            return;
        }

        if (database == null)
        {
            Debug.LogError("[QuizController] QuizDatabase is null");
            return;
        }

        if (currentQuiz == null)
        {
            Debug.LogError("[QuizController] currentQuiz is null");
            return;
        }

        GenerateQuestions();
        ShowQuestion();
    }

    private void LoadDatabase()
    {
        database = JsonUtility.FromJson<QuizDatabase>(quizzesJson.text);
    }

    private void LoadQuiz()
    {
        string quizId = QuizSession.SelectedQuizId;

        if (string.IsNullOrEmpty(quizId))
        {
            Debug.LogError("[QuizController] SelectedQuizId is NULL");
            return;
        }

        currentQuiz = database.GetQuizById(quizId);

        if (currentQuiz == null)
        {
            Debug.LogError("[QuizController] Quiz not found: " + quizId);
        }
    }

    // gets and randomize questions
    private void GenerateQuestions()
    {
        if (currentQuiz == null)
            return;

        if (currentQuiz.questions == null || currentQuiz.questions.Count == 0)
        {
            Debug.LogError("[QuizController] Quiz has no questions: " + currentQuiz.quizId);
            return;
        }

        List<QuestionData> pool = new List<QuestionData>(currentQuiz.questions);

        for (int i = 0; i < pool.Count; i++)
        {
            int rand = Random.Range(i, pool.Count);
            var temp = pool[i];
            pool[i] = pool[rand];
            pool[rand] = temp;
        }

        int count = Mathf.Min(currentQuiz.questionsPerRun, pool.Count);
        currentQuestions = pool.GetRange(0, count);
    }

    private void ShowQuestion()
    {
        if (currentQuestionIndex >= currentQuestions.Count)
        {
            FinishQuiz();
            return;
        }

        var q = currentQuestions[currentQuestionIndex];

        questionText.text = q.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < q.answers.Count)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerTexts[i].text = q.answers[i].text;

                int index = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnAnswerSelected(int index)
    {
        var q = currentQuestions[currentQuestionIndex];

        if (q.answers[index].isCorrect)
        {
            correctAnswers++;
        }

        currentQuestionIndex++;
        ShowQuestion();
    }

    private void FinishQuiz()
    {
        int total = currentQuestions.Count;

        if (total <= 0)
        {
            Debug.LogError("[QuizController] No questions were generated.");
            return;
        }

        // score is calculated as a proportion of correct answers to total, multiplied by maxReward, and rounded to nearest integer
        int score = Mathf.RoundToInt((float)correctAnswers / total * currentQuiz.maxReward); 

        var test = save.GetOrCreateTest(currentQuiz.quizId);
        int oldScore = test.bestScore;
        int rewardDelta = 0;

        if (score > oldScore)
        {
            rewardDelta = score - oldScore;
            save.sparksTotal += rewardDelta;
            save.episodeSparks += rewardDelta;

            test.bestScore = score;
        }

        SaveSystem.Save(save);
        ShowResult(score, currentQuiz.maxReward, rewardDelta);
    }

    private void ShowResult(int score, int max, int rewardDelta)
    {
        resultRoot.SetActive(true);

        if (questionText != null)
            questionText.gameObject.SetActive(false);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
                answerButtons[i].gameObject.SetActive(false);
        }

        resultText.text = $"Вы получили {score} из {max}\n+{rewardDelta} искорок";
    }
}