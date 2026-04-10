using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerExperience : MonoBehaviour
{
    private int maxLevel;
    private int startingLevel;
    private int startingExperience;
    private bool initialized;

    public event Action LevelUpAvailable;

    public int CurrentLevel { get; private set; }
    public int CurrentExperience { get; private set; }
    public List<int> LevelThresholds { get; private set; } = new List<int>();
    public bool HasPendingLevelUps => pendingLevelUps > 0;

    private int pendingLevelUps;

    public void Configure(int configuredStartingLevel, int configuredMaxLevel, List<int> configuredThresholds, int configuredStartingExperience)
    {
        startingLevel = configuredStartingLevel;
        maxLevel = configuredMaxLevel;
        startingExperience = configuredStartingExperience;
        LevelThresholds = new List<int>(configuredThresholds ?? new List<int>());
        CurrentLevel = startingLevel;
        CurrentExperience = startingExperience;
        pendingLevelUps = 0;
        initialized = true;
    }

    void Start()
    {
        if (!initialized)
        {
            return;
        }

        BuildExperienceCurve();
        UIController.Instance.UpdateExperienceSlider();
    }

    void OnEnable()
    {
        ExpCrystal.onExpCrystalCollect += AddExperience;
    }

    void OnDisable()
    {
        ExpCrystal.onExpCrystalCollect -= AddExperience;
    }

    public void AddExperience(int addedExperience)
    {
        CurrentExperience += addedExperience;

        while (CurrentLevel < LevelThresholds.Count && CurrentExperience >= LevelThresholds[CurrentLevel - 1])
        {
            CurrentExperience -= LevelThresholds[CurrentLevel - 1];
            CurrentLevel++;
            pendingLevelUps++;
        }

        UIController.Instance.UpdateExperienceSlider();

        if (pendingLevelUps > 0)
        {
            LevelUpAvailable?.Invoke();
        }
    }

    public bool TryConsumePendingLevelUp()
    {
        if (pendingLevelUps <= 0)
        {
            return false;
        }

        pendingLevelUps--;
        return true;
    }

    private void BuildExperienceCurve()
    {
        if (LevelThresholds.Count == 0)
        {
            LevelThresholds.Add(15);
        }

        for (int i = LevelThresholds.Count; i < maxLevel; i++)
        {
            LevelThresholds.Add(
                Mathf.CeilToInt(LevelThresholds[LevelThresholds.Count - 1] * 1.1f + 15)
            );
        }
    }
}
