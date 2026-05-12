using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TestController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text pageTitleText;
    [SerializeField] private TMP_Text progressLabelText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressBarFill;

    [SerializeField] private Transform answersContainer;
    [SerializeField] private AnswerButton answerPrefab;

    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    [Header("Result Popup")]
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text resultMessageText;

    [Header("Result Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private TMP_Text retryButtonText;
    [SerializeField] private GameObject retryIcon;
    [SerializeField] private Button homeButton;
    [SerializeField] private TMP_Text homeButtonText;

    [Header("Data")]
    [SerializeField] private TextAsset testsJson;

    [Header("Scene Names")]
    [SerializeField] private string notesSceneName = "NotesListPage";
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    private TestDatabase testDatabase;
    private TestData currentTest;

    private int currentQuestionIndex = 0;
    private int score = 0;
    private int totalQuestions;

    private List<QuestionData> questionsPool;
    private List<AnswerButton> spawnedAnswers = new();

    private AnswerButton selectedAnswer;
    private bool isTransitioning = false;
    private Coroutine progressRoutine;

    // ================= INIT =================

    private void Start()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("[Test] LocalizationManager is NULL");
            return;
        }

        LoadDatabase();
        UpdateStaticTexts();
        LoadSelectedTest();
    }

    void LoadDatabase()
    {
        if (testsJson == null)
        {
            Debug.LogError("[Test] testsJson is NULL");
            return;
        }

        testDatabase = JsonUtility.FromJson<TestDatabase>(testsJson.text);

        if (testDatabase == null)
        {
            Debug.LogError("[Test] Failed to parse test database");
            return;
        }

        Debug.Log($"[Test] Loaded tests: {testDatabase.tests.Count}");
    }

    void LoadSelectedTest()
    {
        string testId = TestSession.SelectedTestId;

        if (string.IsNullOrEmpty(testId))
        {
            Debug.LogError("No Test ID passed!");
            return;
        }

        if (testDatabase == null)
        {
            Debug.LogError("[Test] Database is NULL");
            return;
        }

        TestData test = testDatabase.GetTestById(testId);

        if (test == null)
        {
            Debug.LogError("Test not found: " + testId);
            return;
        }

        Init(test);
    }

    public void Init(TestData test)
    {
        currentTest = test;

        totalQuestions = Mathf.Min(test.questionsPerRun, test.questions.Count);

        if (totalQuestions <= 0)
        {
            Debug.LogError("[Test] totalQuestions <= 0");
            return;
        }

        questionsPool = new List<QuestionData>(currentTest.questions);
        ShuffleQuestions();
        questionsPool = questionsPool.GetRange(0, totalQuestions);

        currentQuestionIndex = 0;
        score = 0;

        resultPopup.SetActive(false);

        continueButton.interactable = false;
        UpdateContinueVisual(false);

        progressBarFill.fillAmount = 0f;

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinue);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(OnRetry);

        homeButton.onClick.RemoveAllListeners();
        homeButton.onClick.AddListener(GoToMainMenu);

        LoadQuestion();
    }

    // ================= QUESTION =================
    void LoadQuestion()
    {
        ClearAnswers();
        selectedAnswer = null;

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= questionsPool.Count)
        {
            Debug.LogError("[Test] Invalid question index");
            return;
        }

        QuestionData question = questionsPool[currentQuestionIndex];

        questionText.text = L(question.questionKey);

        int current = currentQuestionIndex + 1;

        progressText.text = L("test_progress")
            .Replace("{0}", current.ToString())
            .Replace("{1}", totalQuestions.ToString());

        float target = (float)current / totalQuestions;

        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        progressRoutine = StartCoroutine(AnimateProgress(target));

        // ответы
        List<AnswerData> shuffled = new(question.answers);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int rand = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rand]) = (shuffled[rand], shuffled[i]);
        }

        foreach (var answer in shuffled)
        {
            var btn = Instantiate(answerPrefab, answersContainer);
            btn.Setup(answer, this);
            StartCoroutine(ResizeNextFrame(btn));
            spawnedAnswers.Add(btn);
        }

        continueButton.interactable = false;
        UpdateContinueVisual(false);

        continueButtonText.text =
            (currentQuestionIndex == totalQuestions - 1)
            ? L("test_finish")
            : L("test_continue");

        isTransitioning = false;
    }

    void ClearAnswers()
    {
        foreach (var btn in spawnedAnswers)
            Destroy(btn.gameObject);

        spawnedAnswers.Clear();
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < questionsPool.Count; i++)
        {
            int rand = Random.Range(i, questionsPool.Count);
            (questionsPool[i], questionsPool[rand]) = (questionsPool[rand], questionsPool[i]);
        }
    }


    // ================= ANSWER =================
    public void OnAnswerSelected(AnswerButton btn, AnswerData data)
    {
        if (selectedAnswer != null) return;

        selectedAnswer = btn;

        foreach (var a in spawnedAnswers)
            a.Lock();

        if (data.isCorrect)
        {
            btn.MarkCorrect();
            score++;
        }
        else
        {
            btn.MarkWrong();

            foreach (var a in spawnedAnswers)
            {
                if (a.IsCorrect())
                {
                    a.MarkCorrect();
                    break;
                }
            }
        }

        StartCoroutine(EnableContinueDelay());
    }

    IEnumerator EnableContinueDelay()
    {
        yield return new WaitForSeconds(0.4f);
        continueButton.interactable = true;
        UpdateContinueVisual(true);
    }

    IEnumerator ResizeNextFrame(AnswerButton btn)
    {
        yield return null;
        yield return null;

        var resize = btn.GetComponent<SimpleCenterPanel>();
        if (resize != null)
            resize.RefreshSize();
    }

    IEnumerator AnimateProgress(float target)
    {
        float start = progressBarFill.fillAmount;
        float time = 0f;

        while (time < 0.3f)
        {
            time += Time.deltaTime;
            progressBarFill.fillAmount = Mathf.Lerp(start, target, time / 0.3f);
            yield return null;
        }

        progressBarFill.fillAmount = target;
    }

    // ================= CONTINUE =================
    public void OnContinue()
    {
        if (isTransitioning || selectedAnswer == null) return;

        isTransitioning = true;

        continueButton.interactable = false;
        UpdateContinueVisual(false);

        currentQuestionIndex++;

        if (currentQuestionIndex >= totalQuestions)
        {
            ShowResult();
            return;
        }

        LoadQuestion();
    }

    // ================= RESULT =================
    void ShowResult()
    {
        progressBarFill.fillAmount = 1f;
        resultPopup.SetActive(true);

        SetResultMessage();

        continueButton.gameObject.SetActive(false);

        int reward = CalculateReward();

        TestBestScore test =
            SaveManager.Instance.Data.GetOrCreateTest(currentTest.testId);

        int rewardDifference =
            Mathf.Max(0, reward - test.claimedReward);

        if (rewardDifference > 0)
        {
            scoreText.text =
                $"<size=117%><color=#8F79C6>{rewardDifference}</color></size>" +
                $"<size=85%><color=#D1CBDE>/{currentTest.maxReward}</color></size>";
        }
        else
        {
            scoreText.text =
                $"<size=90%><color=#D1CBDE>{L("test_reward_claimed")}</color></size>";
        }

        ApplyReward();

        bool completedPerfectly = test.claimedReward >= currentTest.maxReward;
        
        retryButton.onClick.RemoveAllListeners();

        if (!completedPerfectly)
        {
            retryButtonText.text = L("test_retry");

            retryIcon.SetActive(true);

            retryButton.onClick.AddListener(OnRetry);
        }
        else
        {
            retryButtonText.text = L("test_to_notes");

            retryIcon.SetActive(false);

            retryButton.onClick.AddListener(GoToNotesList);
        }
    }

    int CalculateReward()
    {
        return Mathf.RoundToInt((float)score / totalQuestions * currentTest.maxReward);
    }

    void SetResultMessage()
    {
        float percent = (float)score / totalQuestions;

        if (score == 0)
            resultMessageText.text = L("test_result_0");
        else if (percent < 0.5f)
            resultMessageText.text = L("test_result_1");
        else if (percent < 1f)
            resultMessageText.text = L("test_result_almost");
        else
            resultMessageText.text = L("test_result_perfect");
    }

    void ApplyReward()
    {

        if (SaveManager.Instance.Data == null)
        {
            Debug.LogError("[Test] SaveManager.Instance.Data is NULL");
            return;
        }

        int reward = CalculateReward();

        TestBestScore test = SaveManager.Instance.Data.GetOrCreateTest(currentTest.testId);

        bool changed = false;

        // ================= BEST SCORE =================

        if (score > test.bestScore)
        {
            test.bestScore = score;

            Debug.Log(
                $"[Test] New best score: {test.bestScore}/{totalQuestions}"
            );

            changed = true;
        }

        // ================= REWARD DIFFERENCE =================

        int rewardDifference = reward - test.claimedReward;

        if (rewardDifference > 0)
        {
            test.claimedReward = reward;

            if (StatSystem.Instance != null)
            {
                StatSystem.Instance.AddEpisodeReward(rewardDifference);
            }
            else
            {
                SaveManager.Instance.Data.AddSparks(rewardDifference);
            }

            Debug.Log(
                $"[Test] Reward gained: +{rewardDifference} sparks"
            );

            Debug.Log(
                $"[Test] Total sparks: {SaveManager.Instance.Data.sparksTotal}"
            );

            changed = true;
        }

        if (changed)
        {
            SaveManager.Instance.AutoSave();
        }
    }


    // ================= UI =================
    void UpdateContinueVisual(bool isActive)
    {
        continueButtonText.color = isActive
            ? new Color32(253, 253, 249, 255)
            : new Color32(143, 121, 198, 255);
    }

    public void OnRetry()
    {
        currentQuestionIndex = 0;
        score = 0;

        questionsPool = new List<QuestionData>(currentTest.questions);
        ShuffleQuestions();
        questionsPool = questionsPool.GetRange(0, totalQuestions);

        resultPopup.SetActive(false);
        continueButton.gameObject.SetActive(true);
        progressBarFill.fillAmount = 0f;

        continueButton.interactable = false;
        UpdateContinueVisual(false);

        LoadQuestion();
    }

    public void GoToNotesList()
    {
        SceneManager.LoadScene(notesSceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ================= LOCALIZATION =================
    string L(string key)
    {
        return LocalizationManager.Instance.GetText("Tests", key);
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(Language lang)
    {
        UpdateStaticTexts();
        RefreshDynamicTexts();
    }

    private void UpdateStaticTexts()
    {
        if (LocalizationManager.Instance == null)
            return;

        pageTitleText.text = L("test_page_title");
        progressLabelText.text = L("test_progress_label");
        instructionText.text = L("test_instruction");
        resultTitleText.text = L("test_result_title");
        homeButtonText.text = L("test_home");
    }

    private void RefreshDynamicTexts()
    {
        if (currentTest == null || questionsPool == null)
            return;

        if (currentQuestionIndex >= questionsPool.Count)
            return;

        QuestionData question = questionsPool[currentQuestionIndex];

        questionText.text = L(question.questionKey);

        int current = currentQuestionIndex + 1;

        progressText.text = L("test_progress")
            .Replace("{0}", current.ToString())
            .Replace("{1}", totalQuestions.ToString());

        continueButtonText.text =
            (currentQuestionIndex == totalQuestions - 1)
            ? L("test_finish")
            : L("test_continue");

        if (resultPopup.activeSelf)
        {
            SetResultMessage();

            TestBestScore test =
            SaveManager.Instance.Data.GetOrCreateTest(currentTest.testId);

            bool completedPerfectly =
                test.bestScore >= totalQuestions;

            retryButtonText.text =
                completedPerfectly
                ? L("test_to_notes")
                : L("test_retry");
                    }

        foreach (var btn in spawnedAnswers)
            btn.RefreshText();
    }
}