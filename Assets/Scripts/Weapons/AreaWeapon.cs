using UnityEngine;

public class AreaWeapon : Weapon
{

    [SerializeField] private GameObject prefab;
    private float spawnCounter;

    // Update is called once per frame
    void Update()
    {
        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0)
        {
            spawnCounter = stats[weaponLevel].cooldown;
            Instantiate(prefab, transform.position, transform.rotation, transform);
        }
    }
}
