using System.Collections.Generic;

public class PlayerStatUpgradeResult
{
    public readonly Dictionary<PlayerStatType, float> percentStats = new Dictionary<PlayerStatType, float>();
    public readonly Dictionary<PlayerStatType, float> flatStats = new Dictionary<PlayerStatType, float>();
    public UpgradeRarity rarity;
    public float weight;

    public void AddStat(PlayerStatType type, float value, bool usesFlatValue)
    {
        Dictionary<PlayerStatType, float> targetDictionary = usesFlatValue ? flatStats : percentStats;

        if (!targetDictionary.ContainsKey(type))
        {
            targetDictionary[type] = 0f;
        }

        targetDictionary[type] += value;
    }

    public IEnumerable<PlayerStatUpgradeEntry> GetEntries()
    {
        foreach (KeyValuePair<PlayerStatType, float> stat in flatStats)
        {
            yield return new PlayerStatUpgradeEntry(stat.Key, stat.Value, usesFlatValue: true);
        }

        foreach (KeyValuePair<PlayerStatType, float> stat in percentStats)
        {
            yield return new PlayerStatUpgradeEntry(stat.Key, stat.Value, usesFlatValue: false);
        }
    }

    public readonly struct PlayerStatUpgradeEntry
    {
        public PlayerStatUpgradeEntry(PlayerStatType statType, float value, bool usesFlatValue)
        {
            this.statType = statType;
            this.value = value;
            this.usesFlatValue = usesFlatValue;
        }

        public readonly PlayerStatType statType;
        public readonly float value;
        public readonly bool usesFlatValue;
    }
}
