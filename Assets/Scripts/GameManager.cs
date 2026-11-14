using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Tries / Highscore")]
    public int currentTries = 1;
    public TextMeshProUGUI triesText;

    [Header("Timer")]
    public float timeElapsed = 0f;
    public TextMeshProUGUI timeText;
    private bool isTimerRunning = true;

    private void Awake()
    {
        // Singleton setzen -> andere Skripte leicht zugreifen 
        if (Instance == null || triesText != null || timeText != null)
        {
            Instance = this;
        }

        // Werte zurücksetzen
        currentTries = 1;
        timeElapsed = 0f;
        FindTextsIfMissing();
    }

    private void Start()
    {
        // Tries-Anzeige zu Beginn aktualisieren
        UpdateTryUI();
    }

    private void Update()
    {
        // Timer jeder Frame mit + updatet Text im UI
        if (isTimerRunning)
        {
            timeElapsed += Time.deltaTime;
            UpdateTimeUI();
        }
    }

    private void FindTextsIfMissing()
    {
        if (triesText != null) return;

        // sucht automatisch Try-Textfeld falls im Inspector nicht zugewiesen 
        foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (tmp.name.ToLower().Contains("try") || tmp.text.ToLower().Contains("try"))
            {
                triesText = tmp;
                break;
            }
        }
    }

    private void UpdateTryUI()
    {
        // Anzahl Versuche ins UI schreiben
        FindTextsIfMissing();
        if (triesText != null)
        {
            triesText.text = "Try: " + currentTries;
        }
    }

    private void UpdateTimeUI()
    {
        // aktuelle Zeit formatiert ins UI schreiben
        if (timeText != null)
        {
            timeText.text = FormatTime(timeElapsed);
        }
    }

    // wird aufgerufen wenn man alle 3 Herzen verliert -> Try +1
    public void AddTryAndSave()
    {
        currentTries++;
        UpdateTryUI();
    }

    // stoppt Timer im Ziel + speichert Zeit & Tries für Level in den PlayerPrefs
    public void SaveLevelStats(int levelNumber)
    {
        isTimerRunning = false;
        PlayerPrefs.SetFloat("Level" + levelNumber + "_Time", timeElapsed);
        PlayerPrefs.SetInt("Level" + levelNumber + "_Tries", currentTries);
        PlayerPrefs.Save();
    }

    // rechnet Sekunden-> Format 00:00.00 
    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}   