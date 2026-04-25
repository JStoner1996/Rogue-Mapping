using TMPro;
using UnityEngine;

public class CharacterStatsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text characterStats;

    void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        PlayerController playerController = PlayerController.Instance;

        if (playerController == null)
        {
            SetDisplay("PLAYER STATS", string.Empty);
            return;
        }

        SetDisplay("PLAYER STATS", BuildStatsText(playerController));
    }

    private void SetDisplay(string title, string statsText)
    {
        if (characterName != null)
        {
            characterName.text = title;
        }

        if (characterStats != null)
        {
            characterStats.text = statsText;
        }
    }

    private string BuildStatsText(PlayerController playerController)
    {
        PlayerHealth health = playerController.PlayerHealthComponent;
        PlayerMovement movement = playerController.PlayerMovementComponent;
        PlayerCollector collector = playerController.PlayerCollectorComponent;
        PlayerStats playerStats = playerController.PlayerStatsComponent;

        if (health == null && movement == null && collector == null && playerStats == null)
        {
            return string.Empty;
        }

        string desc = string.Empty;

        if (health != null)
        {
            desc += $"Health: {health.CurrentHealth:F1}/{health.MaxHealth:F1}\n";
            desc += $"Armor: {health.Armor:F1} ({health.ArmorMitigationFraction * 100f:F1}% mitigation)\n";
            desc += $"Evasion: {health.Evasion:F1} ({health.EvadeChance * 100f:F1}% evade)\n";
            desc += $"Health Regen: {health.HealthRegenPerSecond:F2}/s\n";
        }

        if (movement != null)
        {
            desc += $"Move Speed: {movement.MoveSpeed:F2}\n";
        }

        if (collector != null)
        {
            desc += $"Pickup Range: {collector.PickupRadius:F2}\n";
        }

        if (playerStats != null)
        {
            AppendPercentStat(ref desc, "Damage Bonus", playerStats.GetTotal(PlayerStatType.Damage));
            AppendPercentStat(ref desc, "Attack Speed Bonus", playerStats.GetTotal(PlayerStatType.AttackSpeed));
            AppendPercentStat(ref desc, "Range Bonus", playerStats.GetTotal(PlayerStatType.Range));
            AppendPercentStat(ref desc, "Knockback Bonus", playerStats.GetTotal(PlayerStatType.Knockback));
        }

        return desc.TrimEnd('\n');
    }

    private void AppendPercentStat(ref string desc, string label, float value)
    {
        desc += $"{label}: {value * 100f:F1}%\n";
    }
}
