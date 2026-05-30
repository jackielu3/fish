using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject gameplayMenuRoot;
    [SerializeField] private GameObject defaultMenuToOpen;
    [SerializeField] private MenuSwitcher defaultSwitcher;

    private GameObject currentMenu;
    private MenuSwitcher currentSwitcher;

    public bool IsOpen => gameplayMenuRoot.activeSelf;

    private void Start()
    {
        gameplayMenuRoot.SetActive(true);

        SwitchMenu(defaultMenuToOpen, defaultSwitcher, true);

        gameplayMenuRoot.SetActive(false);
    }

    public void SwitchMenu(GameObject menuToOpen, MenuSwitcher switcher)
    {
        SwitchMenu(menuToOpen, switcher, false);
    }

    private void SwitchMenu(GameObject menuToOpen, MenuSwitcher switcher, bool snap)
    {
        if (menuToOpen == null) return;

        gameplayMenuRoot.SetActive(true);

        if (currentMenu != null && currentMenu != menuToOpen)
            currentMenu.SetActive(false);

        if (currentSwitcher != null && currentSwitcher != switcher)
            currentSwitcher.SetSelected(false, snap);

        menuToOpen.SetActive(true);
        currentMenu = menuToOpen;

        currentSwitcher = switcher;
        currentSwitcher.SetSelected(true, snap);
    }

    public void OpenDefaultMenu()
    {
        if (currentMenu == null)
            SwitchMenu(defaultMenuToOpen, defaultSwitcher, true);
        else
            gameplayMenuRoot.SetActive(true);
    }

    public void CloseMenu()
    {
        gameplayMenuRoot.SetActive(false);

        if (currentSwitcher != null)
            currentSwitcher.SnapToTarget();
    }

    public void ToggleDefaultMenu()
    {
        if (IsOpen)
            CloseMenu();
        else
            OpenDefaultMenu();
    }
}