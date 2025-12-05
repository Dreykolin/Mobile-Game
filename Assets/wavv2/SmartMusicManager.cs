using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MusicTrack
{
    public string nombreReferencia;
    public AudioClip clip;
    [Range(0f, 1f)] public float volumen = 0.5f;
    [Tooltip("Escribe aquí los nombres EXACTOS de las escenas")]
    public List<string> escenas;
}

public class SmartMusicManager : MonoBehaviour
{
    public static SmartMusicManager Instance;

    [Header("Configuración")]
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Biblioteca de Música")]
    [SerializeField] private List<MusicTrack> bibliotecaMusical;

    private AudioSource _source;
    private Coroutine _musicRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        MusicTrack trackParaEstaEscena = null;

        // Buscar coincidencia en la lista
        foreach (var track in bibliotecaMusical)
        {
            if (track.escenas.Contains(sceneName))
            {
                trackParaEstaEscena = track;
                break;
            }
        }

        if (trackParaEstaEscena != null)
        {
            if (_source.isPlaying && _source.clip == trackParaEstaEscena.clip)
                return;

            CambiarMusica(trackParaEstaEscena);
        }
        else
        {
            StopMusica();
        }
    }

    public void CambiarMusica(MusicTrack nuevoTrack)
    {
        if (_musicRoutine != null) StopCoroutine(_musicRoutine);
        _musicRoutine = StartCoroutine(CrossfadeRoutine(nuevoTrack));
    }

    public void StopMusica()
    {
        if (_musicRoutine != null) StopCoroutine(_musicRoutine);
        _musicRoutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator CrossfadeRoutine(MusicTrack nuevoTrack)
    {
        if (_source.isPlaying)
        {
            float startVol = _source.volume;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                _source.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
                yield return null;
            }
            _source.Stop();
        }

        _source.clip = nuevoTrack.clip;
        _source.volume = 0f;
        _source.Play();

        float tIn = 0f;
        while (tIn < fadeDuration)
        {
            tIn += Time.deltaTime;
            _source.volume = Mathf.Lerp(0f, nuevoTrack.volumen, tIn / fadeDuration);
            yield return null;
        }
        _source.volume = nuevoTrack.volumen;
    }

    private IEnumerator FadeOutAndStop()
    {
        if (!_source.isPlaying) yield break;

        float startVol = _source.volume;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _source.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }
        _source.Stop();
    }
}