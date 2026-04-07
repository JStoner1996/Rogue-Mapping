using UnityEngine;
public class ChainLightningWeapon : TargetedWeapon
{
    protected override void InitializeProjectile(GameObject obj, Enemy target)
    {
        ChainLightningPrefab chain = obj.GetComponent<ChainLightningPrefab>();
        chain.Initialize(this, target);
    }
}


