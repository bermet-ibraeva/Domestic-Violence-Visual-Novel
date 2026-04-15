using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TestController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressBarFill;

    [SerializeField] private Transform answersContainer;
    [SerializeField] private AnswerButton answerPrefab;

    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    [Header("Result Popup")]
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text resultMessageText;

    [Header("Result Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private TMP_Text retryButtonText;
    [SerializeField] private GameObject retryIcon; // иконка внутри кнопки

    [SerializeField] private Button homeButton;

    private TestData currentTest;
    private int currentQuestionIndex = 0;
    private int score = 0;

    private List<AnswerButton> spawnedAnswers = new List<AnswerButton>();
    private AnswerButton selectedAnswer;
    private bool isTransitioning = false;

    private Coroutine progressRoutine;
    private List<QuestionData> questionsPool;

    private int totalQuestions;

    // ================= INIT =================


    [SerializeField] private TextAsset testsJson;

    private TestDatabase testDatabase;

    private void Start()
    {
        LoadDatabase();
        LoadSelectedTest();

        Debug.Log("Received test: " + TestSession.SelectedTestId);
    }

    void LoadDatabase()
    {
        testDatabase = JsonUtility.FromJson<TestDatabase>(testsJson.text);
    }

    void LoadSelectedTest()
    {
        string testId = TestSession.SelectedTestId;
        Debug.Log("TestSession ID: " + testId);

        if (string.IsNullOrEmpty(testId))
        {
            Debug.LogError("No Test ID passed!");
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

        if (currentTest == null || currentTest.questions == null || currentTest.questions.Count == 0)
        {
            Debug.LogError("Test data missing");
            return;
        }

        totalQuestions = Mathf.Min(test.questionsPerRun, test.questions.Count);

        // копия вопросов
        questionsPool = new List<QuestionData>(currentTest.questions);

        ShuffleQuestions();
        if (totalQuestions <= 0) return;
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

    // ================= LOAD QUESTION =================

    void LoadQuestion()
    {
        ClearAnswers();

        selectedAnswer = null;

        QuestionData question = questionsPool[currentQuestionIndex];
        questionText.text = question.questionText;

        // прогресс текст
        int current = currentQuestionIndex + 1;
        progressText.text = $"Вопрос {current} из {totalQuestions}";

        // 🔥 анимация прогресса
        float target = (float)(currentQuestionIndex + 1) / totalQuestions;
        if (progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
            progressRoutine = null;
        }

        progressRoutine = StartCoroutine(AnimateProgress(target));

        // ответы
        List<AnswerData> shuffledAnswers = new List<AnswerData>(question.answers);

        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            int rand = Random.Range(i, shuffledAnswers.Count);
            var temp = shuffledAnswers[i];
            shuffledAnswers[i] = shuffledAnswers[rand];
            shuffledAnswers[rand] = temp;
        }

        foreach (var answer in shuffledAnswers)
        {
            var btn = Instantiate(answerPrefab, answersContainer);
            btn.Setup(answer, this);
            StartCoroutine(ResizeNextFrame(btn));
            spawnedAnswers.Add(btn);
        }

        continueButton.interactable = false;
        UpdateContinueVisual(false);

        // текст кнопки
        continueButtonText.text = (currentQuestionIndex == totalQuestions - 1)
            ? "Завершить"
            : "Продолжить";

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
            var temp = questionsPool[i];
            int randomIndex = Random.Range(i, questionsPool.Count);
            questionsPool[i] = questionsPool[randomIndex];
            questionsPool[randomIndex] = temp;
        }
    }

    // ================= ANSWER CLICK =================

    public void OnAnswerSelected(AnswerButton btn, AnswerData data)
    {
        if (selectedAnswer != null) return;

        selectedAnswer = btn;

        // блокируем все
        foreach (var answer in spawnedAnswers)
            answer.Lock();

        if (data.isCorrect)
        {
            btn.SetCorrect();
            score++;
        }
        else
        {
            btn.SetWrong();

            // подсветка правильного
            foreach (var answer in spawnedAnswers)
            {
                if (answer.IsCorrect())
                {
                    answer.SetCorrect();
                    break;
                }
            }
        }

        // 🔥 небольшая задержка перед активацией кнопки
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
        {
            resize.RefreshSize();
        }
    }

    // ================= PROGRESS ANIMATION =================

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
        if (isTransitioning) return;
        if (selectedAnswer == null) return;

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

        scoreText.text =
            $"<size=117%><color=#8F79C6>{reward}</color></size>" +
            $"<size=85%><color=#D1CBDE>/{currentTest.maxReward}</color></size>";

        ApplyReward();

        if (score < totalQuestions)
        {
            retryButtonText.text = "Пройти заново";
            if (retryIcon != null)
                retryIcon.SetActive(true);
        }
        else
        {
            retryButtonText.text = "К заметкам";
            if (retryIcon != null)
                retryIcon.SetActive(false);
        }
    }

    
    int CalculateReward()
    {
        return Mathf.RoundToInt(
            (float)score / totalQuestions * currentTest.maxReward
        );
    }

    void SetResultMessage()
    {
        if (score == 0)
        {
            resultMessageText.text = "Попробуй ещё раз";
        }
        else if (score == 1)
        {
            resultMessageText.text = "Неплохо, но есть над чем поработать";
        }
        else if (score == totalQuestions - 1)
        {
            resultMessageText.text = "Почти получилось";
        }
        else if (score == totalQuestions)
        {
            resultMessageText.text = "Отлично!!!";
        }
    }

    void ApplyReward()
    {
        SaveData save = SaveSystem.Load();

        NoteState note = save.GetOrCreateNote(currentTest.noteId);
        TestBestScore test = save.GetOrCreateTest(currentTest.testId);

        int reward = CalculateReward();

        bool shouldSave = false;

        if (score > test.bestScore)
        {
            test.bestScore = score;
            shouldSave = true;
        }

        if (note.rewardClaimed)
        {
            if (shouldSave)
                SaveSystem.Save(save);
            return;
        }

        if (reward > 0)
        {
            save.sparksTotal += reward;

            if (reward == currentTest.maxReward)
                note.rewardClaimed = true;

            note.testUnlocked = true;

            SaveSystem.Save(save);
        }
    }

    void UpdateContinueVisual(bool isActive)
    {
        if (isActive)
        {
            continueButtonText.color = new Color32(253, 253, 249, 255); // #FDFDF9
        }
        else
        {
            continueButtonText.color = new Color32(143, 121, 198, 255); // #8F79C6
        }
    }

    public void OnRetry()
    {
        if (score < totalQuestions)
        {
            currentQuestionIndex = 0;
            score = 0;

            resultPopup.SetActive(false);
            progressBarFill.fillAmount = 0f;

            // вернуть кнопку
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = false;
            UpdateContinueVisual(false);

            // остановить анимацию
            if (progressRoutine != null)
            {
                StopCoroutine(progressRoutine);
                progressRoutine = null;
            }

            LoadQuestion();
        }
        else
        {
            GoToNotesList();
        }
    }

    public void GoToNotesList()
    {
        SceneManager.LoadScene("NotesListPage"); // поменяй на своё имя сцены
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}