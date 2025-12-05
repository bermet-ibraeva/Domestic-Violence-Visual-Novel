using UnityEngine;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    [Header("Controllers")]
    public UIController ui;
    public BackgroundController bg;
    public EmotionsController leftEmotions;
    public EmotionsController rightEmotions;

    private EpisodeData episode;
    private Dictionary<string, DialogueNode> nodeDict;
    private DialogueNode currentNode;
    private SaveData save;

    void Start()
    {
        // получаем сейв
        save = TempGameContext.saveToLoad;

        if (save == null)
        {
            Debug.LogWarning("DialogueController: save was null, creating default");
            save = new SaveData
            {
                episodePath = "Episodes/episode_1",
                currentNodeId = "scene_1_start",
                chapterNumber = 1
            };
            SaveManager.Save(save);
        }

        // загружаем эпизод
        episode = EpisodeLoader.LoadEpisode(save.episodePath, out nodeDict);

        if (episode == null)
        {
            Debug.LogError("DialogueController: episode failed to load");
            return;
        }

        ShowNode(save.currentNodeId);
    }

    public void ShowNode(string id)
    {
        if (!nodeDict.TryGetValue(id, out currentNode))
        {
            Debug.LogError("DialogueController: NODE NOT FOUND: " + id);
            return;
        }

        // Автосейв
        save.currentNodeId = id;
        SaveManager.Save(save);

        // Скрываем UI перед отображением нового
        ui.HideAll();

        // Фон
        if (!string.IsNullOrEmpty(currentNode.background))
            bg.SetBackground(currentNode.background);

        // ===== ОБРАБОТКА АВТОРА =====
        if (currentNode.character == "Автор" || string.IsNullOrEmpty(currentNode.character))
        {
            ui.authorPanel.SetActive(true);
            ui.authorText.text = currentNode.text;
            ui.HideChoices();
            return;
        }

        // ===== АЙНАЗ =====
        if (currentNode.character == "Айназ")
        {
            ui.ainazPanel.SetActive(true);
            ui.ainazName.text = "Айназ";
            ui.ainazText.text = currentNode.text;
            leftEmotions.Set(currentNode.emotion);
            SetupChoices();
            return;
        }

        // ===== ЛЮБОЙ ДРУГОЙ ПЕРСОНАЖ =====
        ui.otherPanel.SetActive(true);
        ui.otherName.text = currentNode.character;
        ui.otherText.text = currentNode.text;
        rightEmotions.Set(currentNode.emotion);

        SetupChoices();
    }

    private void SetupChoices()
    {
        // Если есть choices → показываем кнопки
        if (currentNode.choices != null && currentNode.choices.Length > 0)
        {
            ui.ShowChoices(currentNode.choices, OnChoicePicked);
        }
        else
        {
            ui.HideChoices();
        }
    }

    public void OnChoicePicked(string nextNodeId)
    {
        ShowNode(nextNodeId);
    }

    public void Next()
    {
        // Если нет nextNode — конец сцены
        if (string.IsNullOrEmpty(currentNode.nextNode))
            return;

        ShowNode(currentNode.nextNode);
    }
}