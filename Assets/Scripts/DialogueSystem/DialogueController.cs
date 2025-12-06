using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public UIController ui;

    [Header("Characters (спрайты)")]
    public GameObject LeftCharacter;              // Айназ
    public EmotionsController LeftEmotions;       // эмоции Айназ

    public GameObject RightCharacter;             // контейнер справа
    public RightCharacterController RightCharacterCtrl; // НОВЫЙ контроллер справа

    [Header("Text Colors")]
    public Color AuthorColor = Color.gray;
    public Color AinazColor = Color.white;
    public Color OtherColor = Color.white;

    [Header("Backgrounds")]
    public BackgroundController backgroundController;

    [Header("Episode Settings")]
    public string episodePath = "Episodes/episode_1";
    public string startNodeId = "scene_1_start";

    private EpisodeData episode;
    private Variables vars;
    private Dictionary<string, DialogueNode> nodeDict;

    private DialogueNode currentNode;
    private bool waitingForAdvance = false;


    void Start()
    {
        LoadEpisode();

        if (nodeDict == null || nodeDict.Count == 0)
        {
            Debug.LogError("DialogueController: nodeDict is empty, cannot start dialogue");
            return;
        }

        ShowNode(startNodeId);
    }

    //--------------------------------------
    // UPDATE — простая версия
    //--------------------------------------
    void Update()
    {
        if (waitingForAdvance && Input.GetMouseButtonDown(0))
        {
            waitingForAdvance = false;

            if (currentNode == null)
                return;

            // Концовка
            if (currentNode.requirements != null &&
                currentNode.requirements.Length > 0 &&
                string.IsNullOrEmpty(currentNode.nextNode))
            {
                HandleEnding(currentNode);
                return;
            }

            // Следующая нода
            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                ShowNode(currentNode.nextNode);
            }
            else
            {
                ui.HideChoices();
            }
        }
    }

    //--------------------------------------
    void LoadEpisode()
    {
        episode = EpisodeLoader.LoadEpisode(episodePath, out nodeDict);

        if (episode == null)
        {
            Debug.LogError("DialogueController: Episode failed to load");
            return;
        }

        vars = episode.variables ?? new Variables();
    }

    //--------------------------------------
    void ShowNode(string nodeId)
    {
        if (!nodeDict.ContainsKey(nodeId))
        {
            Debug.LogError("Node not found: " + nodeId);
            return;
        }

        DialogueNode node = nodeDict[nodeId];
        currentNode = node;
        waitingForAdvance = false;

        Debug.Log($"[DIALOGUE] ShowNode: {nodeId}, background = {node.background}");

        // Эффекты
        ApplyEffects(node);

        // Фон
        if (backgroundController != null && !string.IsNullOrEmpty(node.background))
            backgroundController.SetBackground(node.background);

        // Скрыть спрайты
        LeftCharacter?.SetActive(false);
        RightCharacter?.SetActive(false);

        //----------------------------
        // Автор
        //----------------------------
        if (node.character == "Автор" || string.IsNullOrEmpty(node.character))
        {
            ui.ShowAuthor(node.text);
            ui.authorText.color = AuthorColor;
        }
        //----------------------------
        // Айназ
        //----------------------------
        else if (node.character == "Айназ")
        {
            ui.ShowAinaz(node.character, node.text);
            ui.ainazText.color = AinazColor;

            LeftCharacter?.SetActive(true);
            ApplyEmotion(LeftEmotions, node.emotion);
        }
        //----------------------------
        // Другой персонаж (справа)
        //----------------------------
        else
        {
            ui.ShowOther(node.character, node.text);
            ui.otherText.color = OtherColor;

            RightCharacter?.SetActive(true);

            // вместо EmotionsController справа
            if (RightCharacterCtrl != null)
            {
                // подставляем нужного персонажа и эмоцию
                RightCharacterCtrl.Show(node.character, node.emotion);
            }
            else
            {
                Debug.LogWarning("[DialogueController] RightCharacterCtrl не назначен в инспекторе.");
            }
        }

        SetupChoices(node);
    }

    //--------------------------------------
    void ApplyEmotion(EmotionsController controller, string emotion)
    {
        if (controller == null) return;
        controller.SetEmotion(emotion);
    }

    //--------------------------------------
    void SetupChoices(DialogueNode node)
    {
        bool hasChoices = node.choices != null && node.choices.Count > 0;

        if (hasChoices)
        {
            ui.ShowChoices(node.choices, OnChoicePicked);
            waitingForAdvance = false;
        }
        else
        {
            ui.HideChoices();
            waitingForAdvance = true;
        }
    }

    void OnChoicePicked(string nextNodeId)
    {
        waitingForAdvance = false;

        if (!string.IsNullOrEmpty(nextNodeId))
            ShowNode(nextNodeId);
        else
            ui.HideChoices();
    }

    //--------------------------------------
    void ApplyEffects(DialogueNode node)
    {
        if (node.effects == null) return;

        vars.Сострадание   += node.effects.Сострадание;
        vars.Послушание    += node.effects.Послушание;
        vars.Сопротивление += node.effects.Сопротивление;
        vars.Тревога       += node.effects.Тревога;
        vars.Доверие       += node.effects.Доверие;
    }

    //--------------------------------------
    void HandleEnding(DialogueNode node)
    {
        if (node.requirements == null || node.requirements.Length == 0)
        {
            Debug.Log("ENDING: no requirements");
            ui.HideChoices();
            return;
        }

        if (vars.Сопротивление >= vars.Послушание)
            Debug.Log("GOOD ENDING: " + node.requirements[0].ending);
        else
            Debug.Log("SILENT ENDING: " + node.requirements[1].ending);

        ui.HideChoices();
    }
}
