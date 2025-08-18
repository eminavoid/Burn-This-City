using System;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private InputActionReference Move, PointerPosition;
    [SerializeField] private float moveSpeed = 7f;
    private Vector2 pointerInput, movementInput;

    private Animator animator;
    private Rigidbody2D rb;

    private float moveInputHorizontal = 0f;
    private bool isFacingRight = false;
    private bool canMove = true;

    public bool hasKey = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (canMove)
            FlipSprite();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        Vector2 input = Move.action.ReadValue<Vector2>();
        moveInputHorizontal = input.x;
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        moveInputHorizontal = 0f;
    }
    private void OnEnable()
    {
        DialogueRunner.DialogueStarted += OnDialogueStarted;
        DialogueRunner.DialogueEnded += OnDialogueEnded;

        Move.action.Enable();
        Move.action.performed += OnMovePerformed;
        Move.action.canceled += OnMoveCanceled;
    }
    private void OnDisable()
    {
        DialogueRunner.DialogueStarted -= OnDialogueStarted;
        DialogueRunner.DialogueEnded -= OnDialogueEnded;

        if (Move.action != null) 
        {

            Move.action.Disable();

            Move.action.performed -= OnMovePerformed;
            Move.action.canceled -= OnMoveCanceled;
        }
    }
    private void OnDialogueStarted()
    {
        canMove = false;
        moveInputHorizontal = 0f;

        if (animator != null)
            animator.SetFloat("xVelocity", 0f);
    }

    private void OnDialogueEnded()
    {
        canMove = true;
    }
    private void FixedUpdate()
    {
        float vx = canMove ? moveInputHorizontal * moveSpeed : 0f;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);

        if (animator != null)
            animator.SetFloat("xVelocity", rb.linearVelocity.x);
    }
    private void FlipSprite()
    {
        bool shouldFaceRight = moveInputHorizontal > 0f;
        bool shouldFaceLeft = moveInputHorizontal < 0f;

        if ((isFacingRight && shouldFaceLeft) ||
            (!isFacingRight && shouldFaceRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }
}
