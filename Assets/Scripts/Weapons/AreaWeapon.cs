using UnityEngine;

public class AreaWeapon : Weapon
{
    [SerializeField] private GameObject prefab;

    protected override void Fire()
    {
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity, transform);

        AreaWeaponPrefab area = obj.GetComponent<AreaWeaponPrefab>();
        area.Initialize(this);
    }
}