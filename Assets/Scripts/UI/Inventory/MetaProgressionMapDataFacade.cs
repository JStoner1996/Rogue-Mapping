using System.Collections.Generic;

// Adapts MetaProgressionService to the reusable map staging data facade.
public class MetaProgressionMapDataFacade : IMapDataFacade
{
    public void EnsureStarterMaps(int desiredCount, VictoryConditionType defaultVictoryCondition, int defaultVictoryTarget)
    {
        MetaProgressionService.EnsureStarterMaps(desiredCount, defaultVictoryCondition, defaultVictoryTarget);
    }

    public List<MapInstance> GetOwnedMaps()
    {
        return MetaProgressionService.GetOwnedMaps();
    }
}
