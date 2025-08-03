using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    // Movement Actions
    public static Vector2 Movement;
    public static bool RunIsHeld;

    // Jump Actions
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;

    // --- NEW INTERACTION ACTION ---
    public static bool InteractWasPressed;
    // ----------------------------

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _interactAction; // --- NEW ---

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            //Debug.LogError("InputManager: PlayerInput component not found on this GameObject!");
            return;
        }

        // --- ADDED ERROR CHECKING ---
        // This will give a clear error if the action names in your asset are wrong.
        _moveAction = GetAction("Move");
        _jumpAction = GetAction("Jump");
        _runAction = GetAction("Run");
        _interactAction = GetAction("Interact");
    }

    private void Update()
    {
        // Read values every frame
        Movement = _moveAction.ReadValue<Vector2>();
        //Debug.Log("SUMBER INPUT MANAGER membaca Movement.x = " + Movement.x);
        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();

        InteractWasPressed = _interactAction.WasPressedThisFrame();

        // --- ADDED DEBUG LOG ---
        // This will print the movement input to the console.
        // If you see this value change when you press A/D, the input is being read correctly.
        //Debug.Log("Movement Input: " + Movement);
    }

    // Helper function to find actions and provide a clear error message if not found.
    private InputAction GetAction(string name)
    {
        var action = playerInput.actions[name];
        if (action == null)
        {
            Debug.LogError($"InputManager: Action '{name}' not found in the Input Actions asset!");
        }
        return action;
    }
}
