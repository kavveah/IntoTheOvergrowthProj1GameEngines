using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [Header("Level(1/2)")]
    public int levelNumber = 1;

    [Header("NextScene")]
    public string nextLevelName = "WinScreen";

    // check ob Player in Ziel-Trigger läuft
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsPlayer(collision.gameObject))
        {
            LoadNextScene();
        }
    }

    // check falls Collider kein Trigger, sondern normale Kollision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayer(collision.gameObject))
        {
            LoadNextScene();
        }
    }

    // prüft Objekt ob Player ist 
    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") ||
               obj.GetComponent<PlayerMovement>() != null ||
               obj.GetComponent<PlayerHealth>() != null;
    }

    private void LoadNextScene()
    {
        // Zeit + Tries vom aktuellen Level im GameManager speichern
        foreach (GameManager gm in FindObjectsByType<GameManager>(FindObjectsSortMode.None))
        {
            gm.SaveLevelStats(levelNumber);
        }

        // nächstes Level oder WinScreen laden
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName.Trim());
        }
    }
}