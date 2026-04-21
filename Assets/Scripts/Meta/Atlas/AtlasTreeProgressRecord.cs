using System;
using System.Collections.Generic;

[Serializable]
public class AtlasTreeProgressRecord
{
    public AtlasCategoryType category;
    public List<string> allocatedNodeIds = new List<string>();
}
