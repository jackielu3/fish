using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class ControlSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private BoatMovement boatMovement;
    [ReadOnly] private HookMovement hookMovement;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIInputHandler gameplayUIInputHandler;

    [Header("UI")]
    [SerializeField] private HookTutorialManager tutorialManager;
    [SerializeField] private BoatModeUI boatModeUI;
    [SerializeField] private LineLengthUI lineLengthUI;

    [Header("Events")]
    public GameEvent onGamePause;

    private PlayerInput playerInput;

    private InputActionMap boatMap;
    private InputActionMap hookMap;
    private InputActionMap menuMap;

    private void Awake()
    {
        cameraController.FollowTarget(boatMovement.transform);

        playerInput = GetComponent<PlayerInput>();

        boatMap = playerInput.actions.FindActionMap("Boat");
        hookMap = playerInput.actions.FindActionMap("Hook");
        menuMap = playerInput.actions.FindActionMap("Menu");

        boatMap.Disable();
        hookMap.Disable();

        menuMap.Enable();
        boatMap.Enable();

        boatModeUI.Show();
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();

        if (boatMap.enabled)
        {
            boatMovement.SetMoveInput(inputVector);
        }
        else if (hookMap.enabled)
        {
            if (hookMovement == null) return;
            hookMovement.SetMoveInput(inputVector);
        }
    }

    public void OnShoot(InputValue value)
    {
        if (value.isPressed && boatMap.enabled)
        {
            Debug.Log("Shooting hook from boat controls");
            tutorialManager.OnHookLaunched();
            LaunchHook();
        }
    }

    private void LaunchHook()
    {
        hookMovement = boatMovement.LaunchHook().GetComponent<HookMovement>();
        cameraController.FollowTarget(hookMovement.transform);
        boatMap.Disable();
        hookMap.Enable();

        HookPathTracker tracker = hookMovement.GetComponent<HookPathTracker>();
        lineLengthUI.SetHook(tracker);

        boatModeUI.Hide();
        
        boatMovement.SetMoveInput(Vector2.zero);
        gameplayUIInputHandler.SetBoatMode(false);
    }
    public void SwitchToBoatControls()
    {
        hookMovement = null;
        cameraController.FollowTarget(boatMovement.transform);

        hookMap.Disable();
        boatMap.Enable();
        lineLengthUI.ClearHook();

        boatModeUI.Show();
        gameplayUIInputHandler.SetBoatMode(true);
    }

    public void ControlSwitch(Component sender, object data)
    {
        if (data is not string type) { return; }
        if ((string)data == "Boat")
        {
            SwitchToBoatControls();
        }
    }
}
