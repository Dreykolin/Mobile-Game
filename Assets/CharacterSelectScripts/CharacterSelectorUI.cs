// CharacterSelectorUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectorUI : MonoBehaviour
{
    [Header("Characters")]
    public CharacterData[] characters;
    int currentIndex = 0;

    [Header("UI")]
    public Image portrait;
    public TextMeshProUGUI nameText;
    public Slider speedSlider, bombsSlider, rangeSlider;

    [Header("Flow")]
    public string gameSceneName = "GameScene"; // <-- pon aquí el nombre real

    void Start()
    {
        if (characters != null && characters.Length > 0) Show(0);
    }

    public void Prev()
    {
        if (characters.Length == 0) return;
        currentIndex = (currentIndex - 1 + characters.Length) % characters.Length;
        Show(currentIndex);
    }

    public void Next()
    {
        if (characters.Length == 0) return;
        currentIndex = (currentIndex + 1) % characters.Length;
        Show(currentIndex);
    }

    void Show(int i)
    {
        var c = characters[i];
        if (portrait) portrait.sprite = c.portrait;
        if (nameText) nameText.text = c.characterName;
        if (speedSlider) speedSlider.value = c.speed;
        if (bombsSlider) bombsSlider.value = c.maxBombs;
        if (rangeSlider) rangeSlider.value = c.explosionRange;
    }

    public void Confirm()
    {
        if (characters.Length == 0) return;
        var chosen = characters[currentIndex];

        if (GameManagerChar.Instance != null)
            GameManagerChar.Instance.selectedCharacter = chosen;

        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogWarning("Asigna gameSceneName en el inspector.");
    }
}
