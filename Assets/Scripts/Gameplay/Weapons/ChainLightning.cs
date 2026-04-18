using UnityEngine;
public class ChainLightningWeapon : TargetedWeapon
{
    protected override void InitializeProjectile(GameObject obj, Enemy target)
    {
        ChainLightningAttack chain = obj.GetComponent<ChainLightningAttack>();
        chain.Initialize(this, target);
    }
}


