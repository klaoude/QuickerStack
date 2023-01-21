using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class FavoritingMode
    {
        private static bool hasCurrentlyToggledFavoriting = false;

        private const string blackStar = "\u2605";
        private const string whiteStar = "\u2606";

        internal static bool HasCurrentlyToggledFavoriting
        {
            get => hasCurrentlyToggledFavoriting;
            set
            {
                hasCurrentlyToggledFavoriting = value;

                if (ButtonRenderer.favoritingTogglingButtonText != null)
                {
                    var color = ColorUtility.ToHtmlStringRGB(FavoriteConfig.BorderColorFavoritedItem.Value);

                    ButtonRenderer.favoritingTogglingButtonText.text = $"<color=#{color}>{(value ? blackStar : whiteStar)}</color>";
                }
            }
        }

        internal static bool IsInFavoritingMode()
        {
            return HasCurrentlyToggledFavoriting
                || Input.GetKey(FavoriteConfig.FavoritingModifierKey1.Value)
                || Input.GetKey(FavoriteConfig.FavoritingModifierKey2.Value);
        }
    }
}