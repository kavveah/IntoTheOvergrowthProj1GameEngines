using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    // Options-menu einblenden
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    //  ausblenden
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    // Lautstärke über Slider anpassen
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void ExitGame()
    {
        Debug.Log("Game closing"); 
        Application.Quit();
    }
}