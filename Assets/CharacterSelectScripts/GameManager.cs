using UnityEngine;

public class GameManagerChar : MonoBehaviour
{
    public static GameManagerChar Instance;
    public CharacterData selectedCharacter;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }
}