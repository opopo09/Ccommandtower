using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button homeButton;
    public Button quitButton;
    public Button resetButton;

    [Header("�z�[���ɖ߂��̃V�[����")]
    public string homeSceneName = "HomeScene"; // �� �ǉ��F�C���X�y�N�^�[�Őݒ�\��

    private bool isPaused = false;
    private PauseInputActions inputActions;

    void Awake()
    {
        inputActions = new PauseInputActions();
        inputActions.UI.Pause.performed += ctx => TogglePauseMenu();
    }

    void OnEnable()
    {
        inputActions?.Enable();
    }

    void OnDisable()
    {
        inputActions?.Disable();
    }

    void Start()
    {
        pauseMenuPanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        homeButton.onClick.AddListener(GoToHome);
        quitButton.onClick.AddListener(QuitGame);
        resetButton.onClick.AddListener(ResetGame);
    }

    void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
    }

    void GoToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName); // �� �C���F�C���X�y�N�^�[�̃V�[�������g�p
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
