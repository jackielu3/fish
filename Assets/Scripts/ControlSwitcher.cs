using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class ControlSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatMovement boatMovement;
    [ReadOnly] private HookMovement hookMovement;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        foreach (InputActionMap map in playerInput.actions.actionMaps)
        {
            map.Disable();
        }

        playerInput.SwitchCurrentActionMap("Boat");
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();

        if (playerInput.currentActionMap.name == "Boat")
        {
            boatMovement.SetMoveInput(inputVector);
        }
        else if (playerInput.currentActionMap.name == "Hook")
        {
            Debug.Log("Setting hook move input " + inputVector);
            hookMovement.SetMoveInput(inputVector);
        }
    }

    public void OnShoot(InputValue value)
    {
        Debug.Log("SHOT: " + playerInput.currentActionMap.name);

        if (value.isPressed && playerInput.currentActionMap.name == "Boat")
        {
            Debug.Log("Shooting hook from boat controls");
            LaunchHook();
        }
    }

    private void LaunchHook()
    {
        hookMovement = boatMovement.LaunchHook().GetComponent<HookMovement>();
        playerInput.SwitchCurrentActionMap("Hook");
        boatMovement.SetMoveInput(Vector2.zero);
    }
}
