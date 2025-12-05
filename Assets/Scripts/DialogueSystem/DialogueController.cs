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
            SaveSystem.Save(save);
        }

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

        // Авто-сейв
        save.currentNodeId = id;
        SaveSystem.Save(save);

        ui.HideAll();

        if (!string.IsNullOrEmpty(currentNode.background))
            bg.SetBackground(currentNode.background);

        // Автор
        if (currentNode.character == "Автор" || string.IsNullOrEmpty(currentNode.character))
        {
            ui.authorPanel.SetActive(true);
            ui.authorText.text = currentNode.text;
            ui.HideChoices();
            return;
        }

        // Айназ
        if (currentNode.character == "Айназ")
        {
            ui.ainazPanel.SetActive(true);
            ui.ainazName.text = "Айназ";
            ui.ainazText.text = currentNode.text;

            leftEmotions.SetEmotion(currentNode.emotion);
            SetupChoices();
            return;
        }

        // Другой персонаж
        ui.otherPanel.SetActive(true);
        ui.otherName.text = currentNode.character;
        ui.otherText.text = currentNode.text;

        rightEmotions.SetEmotion(currentNode.emotion);
        SetupChoices();
    }

    private void SetupChoices()
    {
        if (currentNode.choices != null && currentNode.choices.Count > 0)
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
        if (!string.IsNullOrEmpty(currentNode.nextNode))
            ShowNode(currentNode.nextNode);
    }
}
