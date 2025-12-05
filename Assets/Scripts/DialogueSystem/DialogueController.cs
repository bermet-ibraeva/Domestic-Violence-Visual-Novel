using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public UIController ui;
    public BackgroundController bg;
    public EmotionsController leftEmotions;
    public EmotionsController rightEmotions;

    EpisodeData ep;
    DialogueNode node;

    void Start()
    {
        ep = EpisodeLoader.LoadEpisode(GameContext.currentEpisodePath);

        if (ep == null)
        {
            Debug.LogError("DialogueController: episode failed to load");
            return;
        }

        ShowNode(GameContext.currentNodeId);
    }

    public void ShowNode(string id)
    {
        if (!ep.nodeDict.TryGetValue(id, out node))
        {
            Debug.LogError("NODE NOT FOUND: " + id);
            return;
        }

        GameContext.currentNodeId = id;
        SaveSystem.Save();

        ui.HideAll();

        // Фон
        if (!string.IsNullOrEmpty(node.background))
            bg.SetBackground(node.background);

        // Автор
        if (node.character == "Автор" || string.IsNullOrEmpty(node.character))
        {
            ui.authorPanel.SetActive(true);
            ui.authorText.text = node.text;
            return;
        }

        // Айназ
        if (node.character == "Айназ")
        {
            ui.ainazPanel.SetActive(true);
            ui.ainazName.text = "Айназ";
            ui.ainazText.text = node.text;

            leftEmotions.Set(node.emotion);
            return;
        }

        // Другой
        ui.otherPanel.SetActive(true);
        ui.otherName.text = node.character;
        ui.otherText.text = node.text;
        rightEmotions.Set(node.emotion);
    }

    public void Next()
    {
        if (!string.IsNullOrEmpty(node.nextNode))
        {
            ShowNode(node.nextNode);
        }
    }
}
