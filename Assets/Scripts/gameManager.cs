using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject levelSelectPanel;
    public GameObject pauseMenu;
    public GameObject loseScreen;
    public GameObject winScreen;
    public Button[] levelButtons;

    private static GameManager instance;
    private EventSystem eventSystem;
    private GameObject playerObject;
    private GameObject flagObject;
    private Collider2D playerCollider;
    private Collider2D flagCollider;
    private bool isPaused;
    private bool gameEnded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SetUpEventSystem();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        FindSceneMenus();
        AddQuitButtonToPauseMenu();
        SetUpButtons();

        if (SceneManager.GetActiveScene().name == "startscreen")
        {
            ShowStartScreen();
        }
        else
        {
            HideAllMenus();
        }
    }

    private void Update()
    {
        if (startPanel != null && startPanel.activeSelf)
        {
            if (Input.anyKeyDown)
            {
                ShowLevelSelect();
            }

            return;
        }

        if (levelSelectPanel != null && levelSelectPanel.activeSelf)
        {
            return;
        }

        if (loseScreen != null && loseScreen.activeSelf)
        {
            return;
        }

        if (winScreen != null && winScreen.activeSelf)
        {
            return;
        }

        CheckFlag();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void ShowStartScreen()
    {
        isPaused = false;
        Time.timeScale = 1f;

        Show(startPanel);
        Hide(levelSelectPanel);
        Hide(pauseMenu);
        Hide(loseScreen);
        Hide(winScreen);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowLevelSelect()
    {
        Hide(startPanel);
        Show(levelSelectPanel);

        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] != null)
            {
                levelButtons[i].interactable = i + 1 <= reachedLevel;
            }
        }
    }

    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f;
        isPaused = false;
        gameEnded = false;
        SceneManager.LoadScene(sceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        gameEnded = false;
        SceneManager.LoadScene("startscreen");
    }

    public void TogglePause()
    {
        if (pauseMenu == null)
        {
            return;
        }

        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Hide(pauseMenu);
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void WinGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level1")
        {
            PlayerPrefs.SetInt("ReachedLevel", 2);
            PlayerPrefs.Save();
            LoadLevel("Level2");
            return;
        }

        if (sceneName == "Level2")
        {
            PlayerPrefs.SetInt("ReachedLevel", 3);
            PlayerPrefs.Save();
            LoadLevel("Level3");
            return;
        }

        PlayerPrefs.SetInt("ReachedLevel", 4);
        PlayerPrefs.Save();

        Show(winScreen);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Show(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void Hide(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void HideAllMenus()
    {
        isPaused = false;
        gameEnded = false;
        Time.timeScale = 1f;

        Hide(startPanel);
        Hide(levelSelectPanel);
        Hide(pauseMenu);
        Hide(loseScreen);
        Hide(winScreen);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetUpButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        levelButtons = FindLevelButtons(buttons);

        if (levelButtons.Length > 0 && levelButtons[0] != null)
        {
            levelButtons[0].onClick.RemoveAllListeners();
            levelButtons[0].onClick.AddListener(() => LoadLevel("Level1"));
        }

        if (levelButtons.Length > 1 && levelButtons[1] != null)
        {
            levelButtons[1].onClick.RemoveAllListeners();
            levelButtons[1].onClick.AddListener(() => LoadLevel("Level2"));
        }

        if (levelButtons.Length > 2 && levelButtons[2] != null)
        {
            levelButtons[2].onClick.RemoveAllListeners();
            levelButtons[2].onClick.AddListener(() => LoadLevel("Level3"));
        }

        foreach (Button button in buttons)
        {
            if (!button.gameObject.scene.IsValid())
            {
                continue;
            }

            if (button.name == "Back")
            {
                button.gameObject.SetActive(false);
            }
            else if (button.name == "Restart")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(RestartLevel);
            }
            else if (button.name == "Retry")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(RestartLevel);
            }
            else if (button.name == "Resume")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ResumeGame);
            }
            else if (button.name == "Back To Menu")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(BackToMenu);
            }
            else if (button.name == "Quit")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(QuitGame);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUpEventSystem();
        FindSceneMenus();
        AddQuitButtonToPauseMenu();
        SetUpButtons();
        FindPlayerAndFlag();

        if (scene.name == "startscreen")
        {
            ShowStartScreen();
        }
        else
        {
            HideAllMenus();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void FindSceneMenus()
    {
        GameObject foundStartPanel = FindObjectByName("StartPanel");
        GameObject foundLevelSelectPanel = FindObjectByName("LevelSelectPanel");
        GameObject foundPauseMenu = FindObjectByName("PauseMenu");
        GameObject foundLoseScreen = FindObjectByName("LoseScreen");
        GameObject foundWinScreen = FindObjectByName("WinScreen");

        if (foundStartPanel != null)
        {
            startPanel = foundStartPanel;
        }

        if (foundLevelSelectPanel != null)
        {
            levelSelectPanel = foundLevelSelectPanel;
        }

        if (foundPauseMenu != null)
        {
            pauseMenu = foundPauseMenu;
        }

        if (foundLoseScreen != null)
        {
            loseScreen = foundLoseScreen;
        }

        if (foundWinScreen != null)
        {
            winScreen = foundWinScreen;
        }
    }

    private void FindPlayerAndFlag()
    {
        playerObject = FindObjectByName("player");
        flagObject = FindObjectByName("flag");

        playerCollider = null;
        flagCollider = null;

        if (playerObject != null)
        {
            playerCollider = playerObject.GetComponent<Collider2D>();
        }

        if (flagObject != null)
        {
            flagCollider = flagObject.GetComponent<Collider2D>();
        }
    }

    private void CheckFlag()
    {
        if (gameEnded || SceneManager.GetActiveScene().name == "startscreen")
        {
            return;
        }

        if (playerObject == null || flagObject == null)
        {
            FindPlayerAndFlag();
        }

        if (playerObject == null || flagObject == null)
        {
            return;
        }

        if (playerCollider != null && flagCollider != null)
        {
            if (playerCollider.bounds.Intersects(flagCollider.bounds))
            {
                WinGame();
            }

            return;
        }

        if (Vector2.Distance(playerObject.transform.position, flagObject.transform.position) < 1f)
        {
            WinGame();
        }
    }

    private GameObject FindObjectByName(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject item in objects)
        {
            if (item.name == objectName && item.scene.IsValid())
            {
                return item;
            }
        }

        return null;
    }

    private void AddQuitButtonToPauseMenu()
    {
        if (pauseMenu == null)
        {
            return;
        }

        if (FindButtonInPauseMenu("Quit") != null)
        {
            return;
        }

        Button template = FindButtonInPauseMenu("Back To Menu");

        if (template == null)
        {
            template = FindButtonInPauseMenu("Restart");
        }

        if (template == null)
        {
            return;
        }

        Button quitButton = Instantiate(template, template.transform.parent);
        quitButton.name = "Quit";
        quitButton.gameObject.name = "Quit";
        quitButton.gameObject.SetActive(true);

        RectTransform rect = quitButton.GetComponent<RectTransform>();
        RectTransform templateRect = template.GetComponent<RectTransform>();

        if (rect != null && templateRect != null)
        {
            rect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0f, -70f);
        }

        TextMeshProUGUI label = quitButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            label.text = "Quit";
        }

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitGame);
    }

    private Button FindButtonInPauseMenu(string buttonName)
    {
        if (pauseMenu == null)
        {
            return null;
        }

        Button[] buttons = pauseMenu.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.name == buttonName)
            {
                return button;
            }
        }

        return null;
    }

    private Button[] FindLevelButtons(Button[] buttons)
    {
        Button[] foundButtons = new Button[3];

        foreach (Button button in buttons)
        {
            if (!button.gameObject.scene.IsValid())
            {
                continue;
            }

            if (button.name == "Level 1")
            {
                foundButtons[0] = button;
            }
            else if (button.name == "Level 2")
            {
                foundButtons[1] = button;
            }
            else if (button.name == "Level 3")
            {
                foundButtons[2] = button;
            }
        }

        return foundButtons;
    }

    private void SetUpEventSystem()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>(true);

        if (eventSystem == null)
        {
            if (systems.Length > 0)
            {
                eventSystem = systems[0];
                DontDestroyOnLoad(eventSystem.gameObject);
            }
            else
            {
                GameObject eventObject = new GameObject("EventSystem");
                eventSystem = eventObject.AddComponent<EventSystem>();
                eventObject.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(eventObject);
            }
        }

        foreach (EventSystem system in systems)
        {
            if (system != null && system != eventSystem)
            {
                Destroy(system.gameObject);
            }
        }
    }
}
