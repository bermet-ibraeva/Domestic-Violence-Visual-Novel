using UnityEngine;
using UnityEngine.UI;

public class EmotionsController : MonoBehaviour
{
    public Image characterImage;      // UI Image на Canvas
    public Sprite happySprite;
    public Sprite sadSprite;
    public Sprite scaredSprite;
    public Sprite calmSprite;

    public void SetSad()
    {
        characterImage.sprite = sadSprite;
    }

    public void SetHappy()
    {
        characterImage.sprite = happySprite;
    }

    public void SetScared()
    {
        characterImage.sprite = scaredSprite;
    }

    public void SetCalm()
    {
        characterImage.sprite = calmSprite; // дефолтное состояние
    }
}
