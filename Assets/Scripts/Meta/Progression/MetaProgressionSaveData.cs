using System;
using System.Collections.Generic;

[Serializable]
public class MetaProgressionSaveData
{
    public int version = 1;
    public int unspentAtlasPoints;
    public List<AtlasTreeProgressRecord> atlasTreeProgress = new List<AtlasTreeProgressRecord>();
    public List<string> completedBaseMapIds = new List<string>();
    public List<OwnedMapRecord> ownedMaps = new List<OwnedMapRecord>();
    public List<OwnedEquipmentRecord> ownedEquipment = new List<OwnedEquipmentRecord>();
    public EquipmentLoadoutData equipmentLoadout = new EquipmentLoadoutData();
}
