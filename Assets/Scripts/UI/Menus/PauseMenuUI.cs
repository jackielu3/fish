using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        pauseMenuUI.SetActive(false);
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void OnRestartButtonClicked()
    {
        Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}