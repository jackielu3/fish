using System;
using Unity.Cinemachine;
using UnityEditor;
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
            LaunchHook();
        }
    }

    private void LaunchHook()
    {
        hookMovement = boatMovement.LaunchHook().GetComponent<HookMovement>();        
        cameraController.FollowTarget(hookMovement.transform);
        boatMap.Disable();
        hookMap.Enable();

        boatMovement.SetMoveInput(Vector2.zero);
    }
    public void SwitchToBoatControls()
    {
        hookMovement = null;
        cameraController.FollowTarget(boatMovement.transform);

        hookMap.Disable();
        boatMap.Enable();
    }

    public void ControlSwitch(Component sender, object data)
    {
        if (data is not string type) { return; }
        if ((string)data == "Boat")
        {
            SwitchToBoatControls();
        }
    }

    // MENU INPUTS
    public void OnPause(InputValue value)
    {
        onGamePause.Raise(this, true);
    }
}
