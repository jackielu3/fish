using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private PauseMenu pauseMenu;

    [Header("State")]
    [SerializeField] private bool isBoatMode = true;

    public void SetBoatMode(bool value)
    {
        isBoatMode = value;

        if (!isBoatMode)
            menuManager.CloseMenu();
    }

    public void OnEscape(InputValue value)
    {
        if (!value.isPressed) return;

        if (isBoatMode)
        {
            menuManager.ToggleDefaultMenu();
        }
        else
        {
            pauseMenu.TogglePause();
        }
    }
}