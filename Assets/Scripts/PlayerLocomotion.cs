using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class PlayerLocomotion : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isWalk;
    public bool isAction;
    public bool isDodge;
    public float playerSpeed = 5f;
    public float horizontalInput;
    public float verticalInput;
    public InputManager inputManager;
    public float rotationDegrees = 90f;
    public Transform parentRootPosition;
    public bool freezeMovement = false;
    [Header("Jumping Attribute")]
    public float jumpPerformingTime = 1f;
    public float jumpForce = 5f;
    public float gravityForce = 5f;
    public float gravityScale = 1;
    public Transform jumpHeight;
    public Transform defaultPosition;
    public AnimationClip jumpingCLip;
    public Vector2 playerDirection = Vector2.zero;
    public Tilemap mapSize;
    public BoxCollider2D playerHitBox;
    //Inner system attribute
    PlayerManager playerManager;
    void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
        playerHitBox = GetComponent<BoxCollider2D>();
        playerManager = GetComponent<PlayerManager>();
        parentRootPosition = transform.parent;
        jumpPerformingTime = jumpingCLip.length;

    }
    public bool isGrounded = false;
    // Update is called once per frame
    void FixedUpdate()
    {
        HandleJump();
        HandleMovement();
        isGrounded = IsGrounded();
        parentRootPosition.transform.position = new Vector3(parentRootPosition.transform.position.x, parentRootPosition.transform.position.y, mapSize.WorldToCell(parentRootPosition.transform.position).y);
    }

    //Jumping Attribute
    bool isPerformingJump = false;
    void HandleJump()
    {
        ActionJumpUp();
        FallingDown();
        if (inputManager.jumpInput && IsGrounded())
        {
            isPerformingJump = true;

        }
    }

    //Jump at desired height
    void ActionJumpUp()
    {
        if (isPerformingJump)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, jumpHeight.position, Time.deltaTime * jumpForce);
            if (this.transform.position.y >= (jumpHeight.position.y - 0.2f))
            {
                isPerformingJump = false;
            }
        }
    }
    //Falling down by gravity force
    void FallingDown()
    {
        if (!isPerformingJump && !isGrounded)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, defaultPosition.position, Time.deltaTime * jumpForce * gravityScale);
            if (this.transform.position.y <= (defaultPosition.position.y + 0.2f))
            {
                transform.position = defaultPosition.position;
            }
        }
    }
    void HandleMovement()
    {

        Vector2 movementInput = inputManager.movementInput;
        Vector2 movement = new Vector2(-movementInput.x, movementInput.y);
        playerDirection = movement;
        movement.Normalize();
        if (movement != Vector2.zero)
        {
            if(!playerManager.isFreezeRotation)
            HandleRotation(movement);

        }

        movement = new Vector3(movementInput.x, movementInput.y, 0);
        movement *= playerSpeed;

        if (IsBlocked(movement)|| playerManager.isFreezePosition) return;
        parentRootPosition.Translate(movement * Time.deltaTime);


    }
    /// <summary>
    /// Return true when there is a contact of colliders (mean that direction is blocked)
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns> <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    bool IsBlocked(Vector3 direction)
    {
        float snappedX = Mathf.Clamp(direction.x, -(playerHitBox.size.x), (playerHitBox.size.x));
        float snappedY = Mathf.Clamp(direction.y, -(playerHitBox.size.y), (playerHitBox.size.y));
        Vector3 playerDirection = new Vector3(snappedX, snappedY, 0);
        Vector3 start = new Vector3(this.transform.parent.transform.position.x,
        this.transform.parent.transform.position.y + (playerHitBox.offset.y * 2), this.transform.parent.transform.position.z);
        Vector2 size = playerHitBox.size;
        if (playerDirection.normalized == Vector3.up || playerDirection.normalized == Vector3.down)
        {
            size = new Vector2(size.x * 2, size.y);
        }
        else if (playerDirection.normalized == Vector3.left || playerDirection.normalized == Vector3.right)
        {
            size = new Vector2(size.x, size.y * 2);
        }
        var listRayHit = Physics2D.BoxCastAll(start + (new Vector3(playerDirection.x, playerDirection.y, start.z) / 2), size, 0f, playerDirection, Vector2.Distance(Vector2.zero, playerDirection));
        if (listRayHit.GetLength(0) > 0)
        {
            foreach (var hit in listRayHit)
            {
                if (hit.transform.tag == "UnitBase")
                {
                    return true;
                }

            }
        }
        return false;

    }
    public void HandleRotation(Vector2 direction)
    {
        Vector3 right = Vector3.right;
        float angle =Vector3.Angle(direction, right);
        if(playerManager.isFreezePosition)
        {
            angle= Vector3.Angle((Vector3)playerManager.GetActionDirection(), right);
        }
        if (angle > 90f)
        {
            this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            this.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
    }
    bool IsGrounded()
    {
        return this.transform.position.y <= defaultPosition.position.y;
    }
    private void OnDrawGizmos()
    {
        Vector3 playerMove = this.playerDirection;
        float snappedX = Mathf.Clamp(playerMove.x, -(playerHitBox.size.x), (playerHitBox.size.x));
        float snappedY = Mathf.Clamp(playerMove.y, -(playerHitBox.size.y), (playerHitBox.size.y));
        Vector3 playerDirection = new Vector3(snappedX, snappedY, 0);
        Vector3 start = new Vector3(this.transform.parent.transform.position.x,
        this.transform.parent.transform.position.y + (playerHitBox.offset.y * 2), this.transform.parent.transform.position.z);
        Vector2 size = playerHitBox.size;
        if (playerDirection.normalized == Vector3.up || playerDirection.normalized == Vector3.down)
        {
            size = new Vector2(size.x * 2, size.y);
        }
        else if (playerDirection.normalized == Vector3.left || playerDirection.normalized == Vector3.right)
        {
            size = new Vector2(size.x, size.y * 2);
        }
        RaycastHit2D listRayHit = Physics2D.BoxCast(start + (playerDirection / 2), size, 0f, playerDirection, Vector2.Distance(Vector2.zero, playerDirection));
        if (listRayHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(start + (new Vector3(-playerDirection.x, playerDirection.y, start.z) / 2), size);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // Perform your ray cast here
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
        {
            // Draw a box around the ray cast hit point
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hit.point, new Vector3(1, 1, 1));
        }
    }
}
