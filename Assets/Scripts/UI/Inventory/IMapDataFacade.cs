using System.Collections.Generic;

// Provides the maps staging layer with map data without binding it to a specific save system.
public interface IMapDataFacade
{
    void EnsureStarterMaps(int desiredCount, VictoryConditionType defaultVictoryCondition, int defaultVictoryTarget);
    List<MapInstance> GetOwnedMaps();
}
