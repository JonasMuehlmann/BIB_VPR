using System;

namespace Messenger.Core.Models
{
    [Flags]
    public enum EmojiCategorieFilter
    {
        None                 = 0,
        ActivitiesArtscrafts = 1 << 1,
        Animals              = 1 << 2,
        Component            = 1 << 3,
        Flags                = 1 << 4,
        FoodDrink            = 1 << 5,
        Objects              = 1 << 6,
        PeopleBody           = 1 << 7,
        SmileysEmotion       = 1 << 8,
        SubdivisionFlag      = 1 << 9,
        SymbolsAlphanum      = 1 << 10,
        TravelPlaces         = 1 << 11,
    }
}
