using System;
using System.Collections.Generic;

[Serializable]
public class OwnedMapRecord
{
    public string instanceId;
    public string baseMapId;
    public MapAffixTier rarity;
    public List<OwnedMapAffixRecord> prefixAffixes = new List<OwnedMapAffixRecord>();
    public List<OwnedMapAffixRecord> suffixAffixes = new List<OwnedMapAffixRecord>();
    public List<OwnedMapAffixRecord> additionalAffixes = new List<OwnedMapAffixRecord>();
    public string displayPrefixAffixName;
    public string displaySuffixAffixName;
    public VictoryConditionType victoryConditionType;
    public int victoryTarget;
}

[Serializable]
public class OwnedMapAffixRecord
{
    public string affixName;
    public List<MapModifierValue> modifierRolls = new List<MapModifierValue>();
}
