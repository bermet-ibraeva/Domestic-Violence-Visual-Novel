using UnityEngine;
using UnityEngine.UI;

public class EmotionsController : MonoBehaviour
{
    [Header("UI Image для отображения персонажа")]
    public Image characterImage;

    [Header("Спрайты эмоций")]
    public Sprite calmSprite;
    public Sprite sadSprite;
    public Sprite scaredSprite;
    public Sprite happySprite;

    public void SetCalm()
    {
        if (characterImage != null && calmSprite != null)
            characterImage.sprite = calmSprite;
    }

    public void SetSad()
    {
        if (characterImage != null && sadSprite != null)
            characterImage.sprite = sadSprite;
    }

    public void SetScared()
    {
        if (characterImage != null && scaredSprite != null)
            characterImage.sprite = scaredSprite;
    }

    public void SetHappy()
    {
        if (characterImage != null && happySprite != null)
            characterImage.sprite = happySprite;
    }
}
