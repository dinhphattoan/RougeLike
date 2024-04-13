using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : MonoBehaviour
{
    PlayerInput playerInput;
    public Vector2 movementInput = Vector2.zero;
    public bool jumpInput = false;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnEnable()
    {
        if (playerInput == null)
        {
            playerInput = new PlayerInput();
            // Tell the "gameplay" action map that we want to get told about
            // when actions get triggered.
            playerInput.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerInput.Enable();
        }
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        jumpInput = Input.GetKey(KeyCode.Space);
    }
}
