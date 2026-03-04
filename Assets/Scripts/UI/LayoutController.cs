using UnityEngine;
using System.Collections;

public class LayoutController : MonoBehaviour
{
    [Header("Dialogue Panels")]
    public SimpleCenterPanel AuthorPanel;
    public CharacterDialoguePanel LeftCharacterPanel;
    public CharacterDialoguePanel RightCharacterPanel;


    [Header("Choice Buttons")]
    public RectTransform ButtonsContainer;
    public ChoiceButton[] choiceButtons; // Перенесли сюда для удобства позиционирования
    public float buttonOffset = 20f;
    
    // Метод для вызова диалога (теперь принимает имя)
    public void ApplyLayout(string characterType, string nameText = "", string dialogueText = "")
    {
        // Скрыть все
        AuthorPanel.gameObject.SetActive(false);
        LeftCharacterPanel.gameObject.SetActive(false);
        RightCharacterPanel.gameObject.SetActive(false);

        RectTransform activeRect = null;

        switch (characterType)
        {
            case "Narrator":
                AuthorPanel.gameObject.SetActive(true);
                AuthorPanel.targetText.text = dialogueText;
                AuthorPanel.RefreshSize();
                activeRect = AuthorPanel.GetComponent<RectTransform>();
                break;

            case "LeftCharacter":
                LeftCharacterPanel.gameObject.SetActive(true);
                LeftCharacterPanel.SetDialogue(nameText, dialogueText);
                activeRect = LeftCharacterPanel.GetComponent<RectTransform>();
                break;

            case "RightCharacter":
                RightCharacterPanel.gameObject.SetActive(true);
                RightCharacterPanel.SetDialogue(nameText, dialogueText);
                activeRect = RightCharacterPanel.GetComponent<RectTransform>();
                break;
        }

        if (activeRect != null)
        {
            StartCoroutine(PositionButtonsNextFrame(activeRect));
        }
    }

    private IEnumerator PositionButtonsNextFrame(RectTransform activeRect)
    {
        // Ждем конца кадра, чтобы Unity применила SetSizeWithCurrentAnchors
        yield return new WaitForEndOfFrame();

        if (activeRect == null || ButtonsContainer == null) yield break;

        // Получаем углы панели в мировых координатах
        Vector3[] corners = new Vector3[4];
        activeRect.GetWorldCorners(corners);

        // corners[0] - левый низ, corners[1] - левый верх, corners[3] - правый низ
        float bottomY = corners[0].y;
        float centerX = (corners[0].x + corners[3].x) / 2f;

        // Устанавливаем позицию контейнера кнопок
        // Важно: у ButtonsContainer Pivot Y должен быть 1 (Top), чтобы он рос вниз от панели
        ButtonsContainer.position = new Vector3(centerX, bottomY - buttonOffset, activeRect.position.z);
    }

    public void RefreshButtonPosition()
    {
        RectTransform activeRect = null;
        if (AuthorPanel.gameObject.activeSelf) activeRect = AuthorPanel.GetComponent<RectTransform>();
        else if (LeftCharacterPanel.gameObject.activeSelf) activeRect = LeftCharacterPanel.GetComponent<RectTransform>();
        else if (RightCharacterPanel.gameObject.activeSelf) activeRect = RightCharacterPanel.GetComponent<RectTransform>();

        if (activeRect != null)
            StartCoroutine(PositionButtonsNextFrame(activeRect));
    }
}