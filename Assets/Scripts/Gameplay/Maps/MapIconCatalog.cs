using UnityEngine;

public static class MapIconCatalog
{
    private const string PlaceholderMapIconPath = "UI/Icons/map_placeholder";

    private static Sprite placeholderMapIcon;
    private static bool hasLoadedPlaceholder;

    public static Sprite PlaceholderMapIcon
    {
        get
        {
            if (!hasLoadedPlaceholder)
            {
                placeholderMapIcon = Resources.Load<Sprite>(PlaceholderMapIconPath);
                hasLoadedPlaceholder = true;
            }

            return placeholderMapIcon;
        }
    }

    public static Sprite ResolveIcon(Sprite specificIcon)
    {
        return specificIcon != null ? specificIcon : PlaceholderMapIcon;
    }
}
