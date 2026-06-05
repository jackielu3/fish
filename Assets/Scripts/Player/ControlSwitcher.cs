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
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private LaunchAimIndicator launchAimIndicator;
    [SerializeField] private LaunchRockClearer launchRockClearer;
    [SerializeField] private InitialLaunchChargeUI launchChargeUI;
    [SerializeField] private BoatManager boatManager;

    private HookBaitDropper hookBaitDropper;

    [Header("UI")]
    [SerializeField] private HookTutorialManager tutorialManager;
    [SerializeField] private BoatModeUI boatModeUI;
    [SerializeField] private LineLengthUI lineLengthUI;

    [Header("Initial Launch")]
    [SerializeField] private float minimumLaunchHoldTime = 2f;
    [SerializeField] private float launchChargeSegmentTime = 1f;
    [SerializeField] private float baseInitialLaunchDistance = 8f;

    private bool isChargingLaunch;
    private bool isAimingLaunch;
    private float launchHoldTimer;
    private Vector2 boatInput;

    [Header("Events")]
    public GameEvent onGamePause;

    private PlayerInput playerInput;

    private InputActionMap boatMap;
    private InputActionMap hookMap;
    private InputActionMap menuMap;

    private InputAction shootAction;

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

        shootAction = boatMap.FindAction("Shoot");

        boatModeUI.Show();
    }

    private void Update()
    {
        if (boatMap.enabled && boatManager != null && boatManager.HasActiveBoat)
        {
            if (shootAction.WasPressedThisFrame())
                StartLaunchCharge();

            if (shootAction.WasReleasedThisFrame())
                ReleaseLaunchCharge();
        }

        if (!isChargingLaunch)
            return;

        if (isChargingLaunch && PressedNonMovementCancelInput())
        {
            CancelLaunchCharge();
            return;
        }

        launchHoldTimer += Time.deltaTime;

        if (launchChargeUI != null)
        {
            launchChargeUI.UpdateCharge(
                launchHoldTimer,
                minimumLaunchHoldTime,
                launchChargeSegmentTime,
                GetMaxLaunchSegment()
            );
        }

        boatMovement.SetMoveInput(Vector2.zero);

        if (CanAimInitialLaunch())
            launchAimIndicator.UpdateAim(boatInput.x);
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();

        if (boatMap.enabled)
        {
            if (boatManager == null || !boatManager.HasActiveBoat)
            {
                boatMovement.SetMoveInput(Vector2.zero);
                return;
            }

            boatInput = inputVector;

            if (isChargingLaunch)
            {
                boatMovement.SetMoveInput(Vector2.zero);
                return;
            }

            if (inputVector.y < -0.5f)
            {
                isAimingLaunch = true;

                if (CanAimInitialLaunch())
                    launchAimIndicator.UpdateAim(inputVector.x);

                boatMovement.SetMoveInput(Vector2.zero);
                return;
            }

            if (isAimingLaunch && inputVector.y >= -0.5f && !isChargingLaunch)
            {
                isAimingLaunch = false;
                launchAimIndicator.HideAndReset();
            }

            boatMovement.SetMoveInput(inputVector);
        }
        else if (hookMap.enabled)
        {
            if (hookMovement == null) return;

            hookMovement.SetMoveInput(inputVector);

            if (hookBaitDropper != null)
                hookBaitDropper.SetBaitInput(inputVector.y);
        }
    }

    private void LaunchHook(Vector2 launchDirection, float initialDashDistance)
    {
        Debug.Log("LAUNCHING HOOK");

        if (launchChargeUI != null)
            launchChargeUI.Hide();

        hookMovement = boatMovement.LaunchHook(launchDirection, initialDashDistance).GetComponent<HookMovement>();
        hookBaitDropper = hookMovement.GetComponent<HookBaitDropper>();

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
        if (launchRockClearer != null)
            launchRockClearer.RestoreRocks();

        if (BaitLifetimeManager.Instance != null)
            BaitLifetimeManager.Instance.OnReturnedToBoat();

        hookBaitDropper = null;

        cameraController.FollowTarget(boatMovement.transform);

        hookMap.Disable();
        boatMap.Enable();

        hookMovement = null;

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

    private void StartLaunchCharge()
    {
        if (boatManager == null || !boatManager.HasActiveBoat)
            return;

        isChargingLaunch = true;
        isAimingLaunch = true;
        launchHoldTimer = 0f;

        boatMovement.SetMoveInput(Vector2.zero);

        if (CanAimInitialLaunch())
            launchAimIndicator.Show();
    }

    private void ReleaseLaunchCharge()
    {
        if (!isChargingLaunch)
            return;

        if (launchChargeUI != null)
            launchChargeUI.Hide();

        bool canLaunch = launchHoldTimer >= minimumLaunchHoldTime;

        if (!canLaunch)
        {
            isChargingLaunch = false;

            if (boatInput.y >= -0.5f)
            {
                isAimingLaunch = false;
                launchAimIndicator.HideAndReset();
            }

            return;
        }

        Vector2 direction = CanAimInitialLaunch()
            ? launchAimIndicator.Direction
            : Vector2.down;

        float distance = GetInitialLaunchDistance();

        isChargingLaunch = false;
        isAimingLaunch = false;

        launchAimIndicator.HideAndReset();

        if (launchRockClearer != null)
        {
            launchRockClearer.ClearRocks(
                boatMovement.HookSpawnPosition,
                direction,
                distance
            );
        }

        tutorialManager.OnHookLaunched();
        LaunchHook(direction, distance);
    }

    private void CancelLaunchCharge()
    {
        if (launchChargeUI != null)
            launchChargeUI.Hide();

        isChargingLaunch = false;
        isAimingLaunch = false;
        launchHoldTimer = 0f;

        boatMovement.SetMoveInput(Vector2.zero);
        launchAimIndicator.HideAndReset();
    }

    private bool CanAimInitialLaunch()
    {
        UpgradeData upgrade = upgradeManager.GetUpgrade(UpgradeType.InitialDiveLaunch);

        if (upgrade == null)
            return false;

        return upgrade.currentTierIndex >= 1;
    }

    private float GetInitialLaunchDistance()
    {
        UpgradeData upgrade = upgradeManager.GetUpgrade(UpgradeType.InitialDiveLaunch);

        int purchasedTierCount = 0;

        if (upgrade != null)
            purchasedTierCount = upgrade.currentTierIndex;

        int chargedSegment = Mathf.FloorToInt(launchHoldTimer / launchChargeSegmentTime) - 1;

        if (chargedSegment <= 0)
            return baseInitialLaunchDistance;

        if (upgrade == null || upgrade.tiers.Count == 0)
            return baseInitialLaunchDistance;

        int upgradeTierIndex = Mathf.Clamp(chargedSegment - 1, 0, purchasedTierCount - 1);

        return upgrade.tiers[upgradeTierIndex].value;
    }

    private bool PressedNonMovementCancelInput()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame ||
                mouse.rightButton.wasPressedThisFrame ||
                mouse.middleButton.wasPressedThisFrame)
                return true;
        }

        if (keyboard == null)
            return false;

        bool pressedMovement =
            keyboard.wKey.wasPressedThisFrame ||
            keyboard.aKey.wasPressedThisFrame ||
            keyboard.sKey.wasPressedThisFrame ||
            keyboard.dKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame ||
            keyboard.downArrowKey.wasPressedThisFrame ||
            keyboard.leftArrowKey.wasPressedThisFrame ||
            keyboard.rightArrowKey.wasPressedThisFrame;

        bool pressedShoot = keyboard.spaceKey.wasPressedThisFrame;

        if (pressedMovement || pressedShoot)

            return false;

        return keyboard.anyKey.wasPressedThisFrame;
    }

    private int GetMaxLaunchSegment()
    {
        UpgradeData upgrade = upgradeManager.GetUpgrade(UpgradeType.InitialDiveLaunch);

        if (upgrade == null)
            return 0;

        return Mathf.Clamp(upgrade.currentTierIndex, 0, upgrade.tiers.Count);
    }
}
