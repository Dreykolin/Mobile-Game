using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Referencias UI")]
    public LevelUI levelUI;

    [Header("Configuración de Niveles")]
    public int currentLevel = 1;
    public int maxLevel = 3;

    public int[] xpToNextLevelList = new int[] { 100, 300 };

    [Header("Progreso Actual")]
    public int currentXP = 0;

    [Header("Configuración de Ganancia")]
    public int xpPerBlock = 10;
    public int xpPerKill = 50;
    public int xpPerTime = 5;
    public float timeInterval = 5f;

    [Header("Audio")]
    public AudioClip levelUpClip;
    public float levelUpVolume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.clip = levelUpClip;
    }

    void Start()
    {
        UpdateUI();
        StartCoroutine(GainXpOverTime());
    }

    IEnumerator GainXpOverTime()
    {
        while (currentLevel < maxLevel)
        {
            yield return new WaitForSeconds(timeInterval);
            AddXP(xpPerTime);
        }
    }

    public void AddXP(int amount)
    {
        if (currentLevel >= maxLevel) return;

        currentXP += amount;

        int xpNeeded = xpToNextLevelList[currentLevel - 1];

        Debug.Log($"✨ +{amount} XP | Nivel: {currentLevel} | Progreso: {currentXP}/{xpNeeded}");

        while (currentLevel < maxLevel && currentXP >= xpNeeded)
        {
            LevelUp(xpNeeded);
            if (currentLevel < maxLevel)
                xpNeeded = xpToNextLevelList[currentLevel - 1];
        }

        UpdateUI();
    }

    void LevelUp(int xpCost)
    {
        currentXP -= xpCost;
        currentLevel++;

        Debug.Log($"🎉 ¡SUBISTE AL NIVEL {currentLevel}! 🎉");

        // 🔊 Reproducir sonido usando AudioSource del jugador
        if (levelUpClip != null && audioSource != null)
        {
            audioSource.volume = levelUpVolume;
            audioSource.PlayOneShot(levelUpClip, levelUpVolume);
        }

        if (currentLevel >= maxLevel)
        {
            Debug.Log("👑 ¡NIVEL MÁXIMO ALCANZADO!");
            currentXP = 0;
        }

        // AQUÍ TUS RECOMPENSAS
    }

    void UpdateUI()
    {
        if (levelUI != null)
        {
            if (currentLevel >= maxLevel)
            {
                levelUI.UpdateXPBar(1, 1);
                levelUI.levelText.text = "Lvl.MAX";
            }
            else
            {
                int xpNeeded = xpToNextLevelList[currentLevel - 1];
                levelUI.UpdateXPBar(currentXP, xpNeeded);
                levelUI.UpdateLevelText(currentLevel);
            }
        }
    }
}
