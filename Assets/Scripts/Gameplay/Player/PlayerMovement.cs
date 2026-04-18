using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private InputActionReference moveAction;
    private float baseMoveSpeed;
    private float moveSpeedMultiplier;

    public Vector3 MoveDirection { get; private set; }
    public float MoveSpeed => baseMoveSpeed * (1f + moveSpeedMultiplier);

    public void Configure(Rigidbody2D rbComponent, Animator animatorComponent, InputActionReference moveReference, float configuredMoveSpeed)
    {
        rb = rbComponent;
        animator = animatorComponent;
        moveAction = moveReference;
        baseMoveSpeed = configuredMoveSpeed;
        moveSpeedMultiplier = 0f;
    }

    public void ApplyMoveSpeedModifier(float value)
    {
        moveSpeedMultiplier += value;
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
            MoveDirection.x * MoveSpeed,
            MoveDirection.y * MoveSpeed
        );
    }
}
