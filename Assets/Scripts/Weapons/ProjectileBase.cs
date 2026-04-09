using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    protected Enemy target;
    protected float projectileSpeed = 20f;

    [SerializeField] protected ProjectileMovementType movementType;

    protected Vector3 direction;

    public void SetTarget(Enemy newTarget)
    {
        target = newTarget;
    }

    protected virtual void Update()
    {
        switch (movementType)
        {
            case ProjectileMovementType.Homing:
                MoveHoming();
                break;

            case ProjectileMovementType.Straight:
                MoveStraight();
                break;
        }
    }

    protected void MoveHoming()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.transform.position - transform.position).normalized;

        Rotate(dir);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            projectileSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
        {
            OnHit();
        }
    }

    protected void MoveStraight()
    {
        transform.position += direction * projectileSpeed * Time.deltaTime;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.2f);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                target = enemy;
                OnHit();
                return;
            }
        }
    }


    protected void Rotate(Vector3 dir, float offset = 0f)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + offset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected abstract void OnHit();
}