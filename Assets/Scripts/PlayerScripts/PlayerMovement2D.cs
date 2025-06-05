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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        FlipSprite();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 inputVector = Move.action.ReadValue<Vector2>();
        moveInputHorizontal = inputVector.x;
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInputHorizontal = 0f;
    }
    private void OnEnable()
    {
        Move.action.Enable();
        Move.action.performed += OnMovePerformed;

        Move.action.canceled += OnMoveCanceled;
    }
    private void OnDisable()
    {
        if (Move.action != null) 
        {

            Move.action.Disable();

            Move.action.performed -= OnMovePerformed;
            Move.action.canceled -= OnMoveCanceled;
        }
    }
    private void FixedUpdate()
    {
        Vector2 targetVelocity = new Vector2(moveInputHorizontal * moveSpeed, rb.linearVelocity.y);

        
        rb.linearVelocity = targetVelocity;
        animator.SetFloat("xVelocity", rb.linearVelocity.x);
    }

    private void FlipSprite()
    {
        if (isFacingRight && moveInputHorizontal < 0f || !isFacingRight && moveInputHorizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }
}
