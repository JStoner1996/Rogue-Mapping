using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EquipmentCatalogSeeder
{
    private const string RootFolder = "Assets/Resources/Equipment";
    private const string BasesFolder = RootFolder + "/Bases";
    private const string AffixesFolder = RootFolder + "/Affixes";
    private const string CatalogsFolder = RootFolder + "/Catalogs";

    [MenuItem("Tools/Equipment/Create Starter Catalogs")]
    public static void CreateStarterCatalogs()
    {
        EnsureFolder("Assets", "Resources");
        EnsureFolder("Assets/Resources", "Equipment");
        EnsureFolder(RootFolder, "Bases");
        EnsureFolder(RootFolder, "Affixes");
        EnsureFolder(RootFolder, "Catalogs");

        List<EquipmentBaseDefinition> bases = CreateBaseDefinitions();
        List<EquipmentAffixDefinition> affixes = CreateAffixDefinitions();

        CreateOrUpdateBaseCatalog(bases);
        CreateOrUpdateAffixCatalog(affixes);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Starter equipment catalogs created/refreshed in Assets/Resources/Equipment.");
    }

    private static List<EquipmentBaseDefinition> CreateBaseDefinitions()
    {
        return new List<EquipmentBaseDefinition>
        {
            CreateOrUpdateBase(
                "Iron Helm",
                EquipmentSlotType.Head,
                1,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.Armor,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 12f,
                    maxValue = 18f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 3f
                }),
            CreateOrUpdateBase(
                "Plate Vest",
                EquipmentSlotType.Chest,
                1,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.Armor,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 20f,
                    maxValue = 30f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 5f
                }),
            CreateOrUpdateBase(
                "Chain Leggings",
                EquipmentSlotType.Legs,
                1,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.Armor,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 14f,
                    maxValue = 22f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 4f
                }),
            CreateOrUpdateBase(
                "Leather Boots",
                EquipmentSlotType.Feet,
                1,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.MovementSpeed,
                    modifierKind = EquipmentModifierKind.Percent,
                    minValue = 0.03f,
                    maxValue = 0.05f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 0.005f
                }),
            CreateOrUpdateBase(
                "Iron Gloves",
                EquipmentSlotType.Hands,
                1,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.Armor,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 10f,
                    maxValue = 16f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 2f
                }),
            CreateOrUpdateBase(
                "Copper Ring",
                EquipmentSlotType.Ring,
                1,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.HealthRegen,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 1f,
                    maxValue = 2f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 0.5f
                }),
            CreateOrUpdateBase(
                "Silver Necklace",
                EquipmentSlotType.Necklace,
                1,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.HealthRegen,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 2f,
                    maxValue = 4f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 0.75f
                }),
        };
    }

    private static List<EquipmentAffixDefinition> CreateAffixDefinitions()
    {
        return new List<EquipmentAffixDefinition>
        {
            CreateOrUpdateAffix(
                "Curing",
                EquipmentAffixType.Prefix,
                1,
                "health",
                new[] { EquipmentSlotType.Ring, EquipmentSlotType.Necklace },
                1,
                4,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.HealthRegen,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 8f,
                    maxValue = 12f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 2f
                }),
            CreateOrUpdateAffix(
                "Healing",
                EquipmentAffixType.Prefix,
                2,
                "health",
                new[] { EquipmentSlotType.Ring, EquipmentSlotType.Necklace },
                5,
                7,
                40,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.HealthRegen,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 14f,
                    maxValue = 18f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 2f
                }),
            CreateOrUpdateAffix(
                "Doctor's",
                EquipmentAffixType.Prefix,
                3,
                "health",
                new[] { EquipmentSlotType.Ring, EquipmentSlotType.Necklace },
                8,
                10,
                70,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.HealthRegen,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 20f,
                    maxValue = 28f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 3f
                }),
            CreateOrUpdateAffix(
                "Stout",
                EquipmentAffixType.Prefix,
                1,
                "health",
                new[] { EquipmentSlotType.Head, EquipmentSlotType.Chest, EquipmentSlotType.Legs, EquipmentSlotType.Hands },
                1,
                10,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.MaximumHealth,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 30f,
                    maxValue = 50f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 5f
                }),
            CreateOrUpdateAffix(
                "Swift",
                EquipmentAffixType.Prefix,
                1,
                "movementspeed",
                new[] { EquipmentSlotType.Feet },
                1,
                10,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.MovementSpeed,
                    modifierKind = EquipmentModifierKind.Percent,
                    minValue = 0.04f,
                    maxValue = 0.08f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 0.005f
                }),
            CreateOrUpdateAffix(
                "of Guarding",
                EquipmentAffixType.Suffix,
                1,
                "armor",
                new[] { EquipmentSlotType.Head, EquipmentSlotType.Chest, EquipmentSlotType.Legs, EquipmentSlotType.Hands },
                1,
                10,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.Armor,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 10f,
                    maxValue = 16f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 3f
                }),
            CreateOrUpdateAffix(
                "of the Boar",
                EquipmentAffixType.Suffix,
                1,
                "health",
                new[] { EquipmentSlotType.Head, EquipmentSlotType.Chest, EquipmentSlotType.Legs, EquipmentSlotType.Hands, EquipmentSlotType.Ring, EquipmentSlotType.Necklace },
                1,
                10,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.MaximumHealth,
                    modifierKind = EquipmentModifierKind.Flat,
                    minValue = 40f,
                    maxValue = 70f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 6f
                }),
            CreateOrUpdateAffix(
                "of Force",
                EquipmentAffixType.Suffix,
                1,
                "damage",
                new[] { EquipmentSlotType.Hands, EquipmentSlotType.Ring, EquipmentSlotType.Necklace },
                1,
                10,
                10,
                new EquipmentModifierDefinition
                {
                    statType = EquipmentStatType.Damage,
                    modifierKind = EquipmentModifierKind.Percent,
                    minValue = 0.05f,
                    maxValue = 0.09f,
                    tierScalingMode = EquipmentTierScalingMode.FlatPerTier,
                    tierScalingAmount = 0.01f
                }),
        };
    }

    private static EquipmentBaseDefinition CreateOrUpdateBase(
        string baseName,
        EquipmentSlotType slotType,
        int minTier,
        int maxTier,
        EquipmentModifierDefinition implicitModifier)
    {
        string assetPath = $"{BasesFolder}/{SanitizeFileName(baseName)}.asset";
        EquipmentBaseDefinition asset = AssetDatabase.LoadAssetAtPath<EquipmentBaseDefinition>(assetPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<EquipmentBaseDefinition>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        SerializedObject serializedObject = new SerializedObject(asset);
        serializedObject.FindProperty("baseName").stringValue = baseName;
        serializedObject.FindProperty("slotType").enumValueIndex = (int)slotType;
        serializedObject.FindProperty("minItemTier").intValue = minTier;
        serializedObject.FindProperty("maxItemTier").intValue = maxTier;

        SerializedProperty implicitModifiers = serializedObject.FindProperty("implicitModifiers");
        implicitModifiers.arraySize = 1;
        WriteModifier(implicitModifiers.GetArrayElementAtIndex(0), implicitModifier);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static EquipmentAffixDefinition CreateOrUpdateAffix(
        string affixName,
        EquipmentAffixType affixType,
        int affixTier,
        string affixTag,
        EquipmentSlotType[] allowedSlots,
        int minTier,
        int maxTier,
        int requiredItemLevel,
        EquipmentModifierDefinition modifier)
    {
        string assetPath = $"{AffixesFolder}/{SanitizeFileName(affixName)}.asset";
        EquipmentAffixDefinition asset = AssetDatabase.LoadAssetAtPath<EquipmentAffixDefinition>(assetPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<EquipmentAffixDefinition>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        SerializedObject serializedObject = new SerializedObject(asset);
        serializedObject.FindProperty("affixName").stringValue = affixName;
        serializedObject.FindProperty("affixType").enumValueIndex = (int)affixType;
        serializedObject.FindProperty("affixTier").intValue = affixTier;
        serializedObject.FindProperty("affixTag").stringValue = affixTag;
        serializedObject.FindProperty("minItemTier").intValue = minTier;
        serializedObject.FindProperty("maxItemTier").intValue = maxTier;
        serializedObject.FindProperty("requiredItemLevel").intValue = requiredItemLevel;

        SerializedProperty allowedSlotsProperty = serializedObject.FindProperty("allowedSlots");
        allowedSlotsProperty.arraySize = allowedSlots.Length;
        for (int i = 0; i < allowedSlots.Length; i++)
        {
            allowedSlotsProperty.GetArrayElementAtIndex(i).enumValueIndex = (int)allowedSlots[i];
        }

        SerializedProperty modifiers = serializedObject.FindProperty("modifiers");
        modifiers.arraySize = 1;
        WriteModifier(modifiers.GetArrayElementAtIndex(0), modifier);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void CreateOrUpdateBaseCatalog(List<EquipmentBaseDefinition> bases)
    {
        string assetPath = $"{CatalogsFolder}/EquipmentBaseCatalog.asset";
        EquipmentBaseCatalog catalog = AssetDatabase.LoadAssetAtPath<EquipmentBaseCatalog>(assetPath);

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<EquipmentBaseCatalog>();
            AssetDatabase.CreateAsset(catalog, assetPath);
        }

        SerializedObject serializedObject = new SerializedObject(catalog);
        SerializedProperty baseDefinitions = serializedObject.FindProperty("baseDefinitions");
        baseDefinitions.arraySize = bases.Count;

        for (int i = 0; i < bases.Count; i++)
        {
            baseDefinitions.GetArrayElementAtIndex(i).objectReferenceValue = bases[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
    }

    private static void CreateOrUpdateAffixCatalog(List<EquipmentAffixDefinition> affixes)
    {
        string assetPath = $"{CatalogsFolder}/EquipmentAffixCatalog.asset";
        EquipmentAffixCatalog catalog = AssetDatabase.LoadAssetAtPath<EquipmentAffixCatalog>(assetPath);

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<EquipmentAffixCatalog>();
            AssetDatabase.CreateAsset(catalog, assetPath);
        }

        SerializedObject serializedObject = new SerializedObject(catalog);
        SerializedProperty affixDefinitions = serializedObject.FindProperty("affixDefinitions");
        affixDefinitions.arraySize = affixes.Count;

        for (int i = 0; i < affixes.Count; i++)
        {
            affixDefinitions.GetArrayElementAtIndex(i).objectReferenceValue = affixes[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
    }

    private static void WriteModifier(SerializedProperty property, EquipmentModifierDefinition modifier)
    {
        property.FindPropertyRelative("statType").enumValueIndex = (int)modifier.statType;
        property.FindPropertyRelative("modifierKind").enumValueIndex = (int)modifier.modifierKind;
        property.FindPropertyRelative("minValue").floatValue = modifier.minValue;
        property.FindPropertyRelative("maxValue").floatValue = modifier.maxValue;
        property.FindPropertyRelative("tierScalingMode").enumValueIndex = (int)modifier.tierScalingMode;
        property.FindPropertyRelative("tierScalingAmount").floatValue = modifier.tierScalingAmount;
    }

    private static string SanitizeFileName(string input)
    {
        foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
        {
            input = input.Replace(invalidChar, '_');
        }

        return input.Replace("'", string.Empty).Replace(" ", "_");
    }

    private static void EnsureFolder(string parentFolder, string newFolderName)
    {
        string targetPath = $"{parentFolder}/{newFolderName}";

        if (!AssetDatabase.IsValidFolder(targetPath))
        {
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }
    }
}
