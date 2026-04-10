using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private InputActionReference moveAction;
    private float moveSpeed;

    public Vector3 MoveDirection { get; private set; }
    public float MoveSpeed => moveSpeed;

    public void Configure(Rigidbody2D rbComponent, Animator animatorComponent, InputActionReference moveReference, float configuredMoveSpeed)
    {
        rb = rbComponent;
        animator = animatorComponent;
        moveAction = moveReference;
        moveSpeed = configuredMoveSpeed;
    }

    void Update()
    {
        if (moveAction == null)
        {
            return;
        }

        MoveDirection = moveAction.action.ReadValue<Vector2>().normalized;

        if (animator == null)
        {
            return;
        }

        animator.SetFloat("moveX", MoveDirection.x);
        animator.SetFloat("moveY", MoveDirection.y);
        animator.SetBool("moving", MoveDirection != Vector3.zero);
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = new Vector2(
            MoveDirection.x * moveSpeed,
            MoveDirection.y * moveSpeed
        );
    }
}
