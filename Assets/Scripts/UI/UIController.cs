using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Controllers")]
    public LayoutController layoutController;


    // Ссылки на тексты теперь живут здесь, чтобы UIController мог ими управлять
    [Header("Text Fields")]
    public TMP_Text authorText;
    public TMP_Text leftCharacterName;
    public TMP_Text rightCharacterName;


    // ---------------------- HIDE ----------------------
    public void HideAll()
    {
        // LayoutController сам знает, как выключить свои панели
        layoutController.AuthorPanel.gameObject.SetActive(false);
        layoutController.LeftCharacterPanel.gameObject.SetActive(false);
        layoutController.RightCharacterPanel.gameObject.SetActive(false);
        HideChoices();
    }

    public void HideChoices()
    {
        if (layoutController.ButtonsContainer != null)
            layoutController.ButtonsContainer.gameObject.SetActive(false);

        // choiceButtons можно оставить в UIController или перенести в Layout
        foreach (var btn in layoutController.choiceButtons)
            btn.gameObject.SetActive(false);
    }

    // ---------------------- SHOW PANELS ----------------------
    
    public void ShowAuthor(string text)
    {
        HideChoices(); // Скрываем старые выборы при новом тексте
        // Передаем пустую строку в имя, так как у автора его нет
        layoutController.ApplyLayout("Narrator", "", text);
    }

    public void ShowLeftCharacter(string name, string text)
    {
        layoutController.ApplyLayout("LeftCharacter", name, text);
    }

    public void ShowRightCharacter(string name, string text)
    {
        layoutController.ApplyLayout("RightCharacter", name, text);
    }

    // ---------------------- SHOW CHOICES ----------------------
    public void ShowChoices(List<Choice> choices, Action<string> callback)
    {
        if (choices == null || choices.Count == 0)
        {
            HideChoices();
            return;
        }

        layoutController.ButtonsContainer.gameObject.SetActive(true);

        // Сначала выключаем все кнопки
        foreach (var btn in layoutController.choiceButtons)
            btn.gameObject.SetActive(false);

        // Включаем нужные
        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= layoutController.choiceButtons.Length) break;

            ChoiceButton btn = layoutController.choiceButtons[i];
            btn.gameObject.SetActive(true);

            string nextNode = choices[i].nextNode;
            btn.SetText(choices[i].text);
            btn.SetCallback(() => callback?.Invoke(nextNode));
        }

        // ПОСЛЕ того как кнопки включены, обновляем их позицию под активной панелью
        layoutController.RefreshButtonPosition();
    }
}