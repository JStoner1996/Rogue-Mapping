using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    protected Enemy target;

    public void SetTarget(Enemy newTarget)
    {
        target = newTarget;
    }

    protected virtual void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        MoveToTarget();
    }

    protected void MoveToTarget(float speed = 20f)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
        {
            OnHit();
        }
    }

    protected abstract void OnHit();
}