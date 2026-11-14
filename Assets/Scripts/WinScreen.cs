using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Daten für einzelnen Run in Rangliste
[System.Serializable]
public class HighscoreEntry
{
    public string playerName;
    public float l1Time;
    public int l1Tries;
    public float l2Time;
    public int l2Tries;
    public float totalTime;
    public int totalTries;
}

// Liste aller Einträge (JsonUtility speichern kann)
[System.Serializable]
public class HighscoreList
{
    public List<HighscoreEntry> entries = new List<HighscoreEntry>();
}

public class WinScreen : MonoBehaviour
{
    [Header("UI Elemente")]
    public TextMeshProUGUI currentRunText;
    public TextMeshProUGUI leaderboardText;
    public TMP_InputField nameInputField;
    public Button saveButton;

    private float l1Time, l2Time;
    private int l1Tries, l2Tries;
    private HighscoreList highscoreList = new HighscoreList();
    private const string HighscoreKey = "Top10Highscores";

    private void Start()
    {
        // Spielzeit normalisieren + Mauszeiger für Menu sichtbar
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Werte lvl1 und lvl2 aus PlayerPrefs holen
        l1Time = PlayerPrefs.GetFloat("Level1_Time", 0f);
        l1Tries = PlayerPrefs.GetInt("Level1_Tries", 1);
        l2Time = PlayerPrefs.GetFloat("Level2_Time", 0f);
        l2Tries = PlayerPrefs.GetInt("Level2_Tries", 1);

        // current Run Ergebnis
        if (currentRunText != null)
        {
            currentRunText.text =
                $"<b>your run:</b>\n" +
                $"Level 1: {GameManager.FormatTime(l1Time)} ({l1Tries} Tries)\n" +
                $"Level 2: {GameManager.FormatTime(l2Time)} ({l2Tries} Tries)\n" +
                $"Gesamt: {GameManager.FormatTime(l1Time + l2Time)} ({l1Tries + l2Tries} Tries)";
        }

        LoadHighscores();
        UpdateLeaderboardUI();
    }

    public void SaveScore()
    {
        // Name 
        string playerName = (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
            ? nameInputField.text.Trim()
            : "unknown";

        // Highscore-Entry mit Run-Werten
        HighscoreEntry newEntry = new HighscoreEntry
        {
            playerName = playerName,
            l1Time = l1Time,
            l1Tries = l1Tries,
            l2Time = l2Time,
            l2Tries = l2Tries,
            totalTime = l1Time + l2Time,
            totalTries = l1Tries + l2Tries
        };

        highscoreList.entries.Add(newEntry);

        // Sortiert schnellster Gesamtzeit -> wenn Gleichstand dann trys
        highscoreList.entries.Sort((a, b) =>
        {
            int timeCompare = a.totalTime.CompareTo(b.totalTime);
            return timeCompare != 0 ? timeCompare : a.totalTries.CompareTo(b.totalTries);
        });

        // Liste abschneiden falls zu viele Einträge
        if (highscoreList.entries.Count > 10)
        {
            highscoreList.entries.RemoveRange(5, highscoreList.entries.Count - 5);
        }

        // Liste in JSON-Text umwandeln
        PlayerPrefs.SetString(HighscoreKey, JsonUtility.ToJson(highscoreList));
        PlayerPrefs.Save();

        // Button deaktivieren wenn einmal eingegeben
        if (saveButton != null) saveButton.interactable = false;
        if (nameInputField != null) nameInputField.interactable = false;

        UpdateLeaderboardUI();
    }

    private void LoadHighscores()
    {
        // check ob Highscores gespeichert sind + aus JSON laden
        if (PlayerPrefs.HasKey(HighscoreKey))
        {
            string json = PlayerPrefs.GetString(HighscoreKey);
            highscoreList = JsonUtility.FromJson<HighscoreList>(json) ?? new HighscoreList();
        }
    }

    private void UpdateLeaderboardUI()
    {
        if (leaderboardText == null) return;

        // falls keine Einträge 
        if (highscoreList.entries.Count == 0)
        {
            leaderboardText.text = "<b></b>\nNothing on the leaderboard yet..";
            return;
        }

        // Einträge durchgehen + Text für das Leaderboard 
        string text = "<b></b>\n";
        for (int i = 0; i < highscoreList.entries.Count; i++)
        {
            var e = highscoreList.entries[i];
            text += $"{i + 1}. <b>{e.playerName}</b> - {GameManager.FormatTime(e.totalTime)} ({e.totalTries} Tries)\n" +
                    $"   <size=75%>L1: {GameManager.FormatTime(e.l1Time)} ({e.l1Tries}T) | L2: {GameManager.FormatTime(e.l2Time)} ({e.l2Tries}T)</size>\n";
        }

        leaderboardText.text = text;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(1); 
    }

    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}