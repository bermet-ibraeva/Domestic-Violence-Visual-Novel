using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset testsJson;

    [Header("UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TMP_Text[] answerTexts;

    [Header("Result UI")]
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private TMP_Text resultText;

    private SaveData save;
    private TestDatabase database;
    private TestData currentTest;

    private List<QuestionData> currentQuestions = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;

    private void Start()
    {
        save = SaveSystem.Load();
        LoadDatabase();
        LoadTest();

        if (save == null)
        {
            Debug.LogError("[TestController] SaveData is null");
            return;
        }

        if (database == null)
        {
            Debug.LogError("[TestController] TestDatabase is null");
            return;
        }

        if (currentTest == null)
        {
            Debug.LogError("[TestController] currentTest is null");
            return;
        }

        GenerateQuestions();
        ShowQuestion();
    }

    private void LoadDatabase()
    {
        if (testsJson == null)
        {
            Debug.LogError("[TestController] testsJson is null");
            return;
        }

        database = JsonUtility.FromJson<TestDatabase>(testsJson.text);
    }

    private void LoadTest()
    {
        string testId = TestSession.SelectedTestId;

        if (string.IsNullOrEmpty(testId))
        {
            Debug.LogError("[TestController] SelectedTestId is null");
            return;
        }

        currentTest = database.GetTestById(testId);

        if (currentTest == null)
        {
            Debug.LogError("[TestController] Test not found: " + testId);
        }
    }

    private void GenerateQuestions()
    {
        if (currentTest == null)
            return;

        if (currentTest.questions == null || currentTest.questions.Count == 0)
        {
            Debug.LogError("[TestController] Test has no questions: " + currentTest.testId);
            return;
        }

        List<QuestionData> pool = new List<QuestionData>(currentTest.questions);

        for (int i = 0; i < pool.Count; i++)
        {
            int rand = Random.Range(i, pool.Count);
            var temp = pool[i];
            pool[i] = pool[rand];
            pool[rand] = temp;
        }

        int count = Mathf.Min(currentTest.questionsPerRun, pool.Count);
        currentQuestions = pool.GetRange(0, count);
    }

    private void ShowQuestion()
    {
        if (currentQuestionIndex >= currentQuestions.Count)
        {
            FinishTest();
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
            correctAnswers++;

        currentQuestionIndex++;
        ShowQuestion();
    }

    private void FinishTest()
    {
        int total = currentQuestions.Count;

        if (total <= 0)
        {
            Debug.LogError("[TestController] No questions were generated.");
            return;
        }

        int score = Mathf.RoundToInt((float)correctAnswers / total * currentTest.maxReward);

        var test = save.GetOrCreateTest(currentTest.testId);
        int oldScore = test.bestScore;
        int rewardDelta = 0;

        if (score > oldScore)
        {
            rewardDelta = score - oldScore;
            save.sparksTotal += rewardDelta;
            TempGameContext.CurrentEpisode.sparks += rewardDelta;
            test.bestScore = score;
        }

        SaveSystem.Save(save);
        ShowResult(score, currentTest.maxReward, rewardDelta);
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