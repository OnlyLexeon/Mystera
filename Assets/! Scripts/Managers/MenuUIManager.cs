using UnityEngine;

public enum MenuState
{
    NoMenu,
    Pause,
    Settings,
    Volume,
    ConfirmQuit,
    Offset,
    Tutorial,
}

public class MenuUIManager : MonoBehaviour
{
    public MenuState menuState;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject volumeMenu;
    public GameObject confirmQuitMenu;
    public GameObject offsetMenu;
    public GameObject tutorialMenu;
    
    public static MenuUIManager instance;

    public bool isMenuOpen = false;

    private void Awake()
    {
        instance = this;
    }

    public void TogglePauseMenu()
    {
        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            OpenPauseMenu();
        }
        else
        {
            ClosePauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        DoMenu(MenuState.Pause);
    }
    public void OpenSettingsMenu()
    {
        DoMenu(MenuState.Settings);
    }
    public void OpenVolumeMenu()
    {
        DoMenu(MenuState.Volume);
    }
    public void ClosePauseMenu()
    {
        DoMenu(MenuState.NoMenu);
    }
    public void DoConfirmQuitMenu()
    {
        DoMenu(MenuState.ConfirmQuit);
    }
    public void OpenOffsetMenu()
    {
        DoMenu(MenuState.Offset);
    }
    public void OpenTutorialMenu()
    {
        DoMenu(MenuState.Tutorial);
    }

    public void DoMenu(MenuState menuToOpen)
    {
        CloseAllMenus();

        isMenuOpen = true;

        switch (menuToOpen)
        {
            case MenuState.NoMenu:
                Time.timeScale = 1;
                menuState = MenuState.NoMenu;

                isMenuOpen = false;

                break;

            case MenuState.Pause:
                Time.timeScale = 0;
                menuState = MenuState.Pause;
                pauseMenu.SetActive(true);
                break;

            case MenuState.Settings:
                menuState = MenuState.Settings;
                settingsMenu.SetActive(true);
                break;

            case MenuState.Volume:
                menuState = MenuState.Volume;
                volumeMenu.SetActive(true);
                break;

            case MenuState.ConfirmQuit:
                menuState = MenuState.ConfirmQuit;
                confirmQuitMenu.SetActive(true);
                break;

            case MenuState.Offset:
                menuState = MenuState.Offset;
                offsetMenu.SetActive(true);
                break;

            case MenuState.Tutorial:
                menuState = MenuState.Tutorial;
                tutorialMenu.SetActive(true);
                break;
        }
    }

    //Close All
    public void CloseAllMenus()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        volumeMenu.SetActive(false);
        confirmQuitMenu.SetActive(false);
        offsetMenu.gameObject.SetActive(false);
        tutorialMenu.gameObject.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();

        // This line is just for testing in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
