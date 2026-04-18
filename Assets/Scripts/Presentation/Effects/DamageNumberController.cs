using UnityEngine;

public class DamageNumberController : SingletonBehaviour<DamageNumberController>
{
    [SerializeField] private DamageNumber prefab;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }
    }

    public void CreateNumber(float value, Vector3 location)
    {
        if (prefab == null)
        {
            return;
        }

        DamageNumber damageNumber = Instantiate(prefab, location, transform.rotation, transform);

        damageNumber.SetValue(Mathf.RoundToInt(value));
    }
}
