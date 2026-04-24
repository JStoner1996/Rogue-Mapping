using System;
using System.Collections.Generic;

[Serializable]
public class OwnedMapRecord
{
    public string instanceId;
    public string baseMapId;
    public string prefixName;
    public string suffixName;
    public List<string> extraAffixNames = new List<string>();
    public VictoryConditionType victoryConditionType;
    public int victoryTarget;
    public List<MapModifierValue> modifiers = new List<MapModifierValue>();
}
