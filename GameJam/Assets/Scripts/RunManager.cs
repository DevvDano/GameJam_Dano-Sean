using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public static RunManager I { get; private set; }
    [SerializeField] int maxRuns = 3;
    public int CurrentRun { get; private set; } = 1;

    public static event Action<int> OnRunChanged;  

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (I == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnable()
    {
        GameEvents.PlayerDied += AdvanceLoop;
        GameEvents.BossDefeated += AdvanceLoop;
    }

    void OnDisable()
    {
        GameEvents.PlayerDied -= AdvanceLoop;
        GameEvents.BossDefeated -= AdvanceLoop;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        OnRunChanged?.Invoke(CurrentRun);
    }

    void AdvanceLoop()
    {
        if (CurrentRun >= maxRuns)
        {
            EndGame();
            return;
        }

        CurrentRun++;
        OnRunChanged?.Invoke(CurrentRun);
        ReloadCurrentScene();
    }

    public void ReloadCurrentScene()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }

    void EndGame()
    {
        try { SceneManager.LoadScene("Ending"); }
        catch { Debug.LogWarning("Replace this with own UI"); }
    }
}
