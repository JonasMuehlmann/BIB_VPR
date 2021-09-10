using System;

namespace Messenger.Core.Models
{
    [Flags]
    public enum EmojiCategory
    {
        None                 = 0,
        Activities           = 1 << 1,
        AnimalsNature        = 1 << 2,
        Component            = 1 << 3,
        Flags                = 1 << 4,
        FoodDrink            = 1 << 5,
        Objects              = 1 << 6,
        PeopleBody           = 1 << 7,
        Smileys              = 1 << 8,
        SubdivisionFlag      = 1 << 9,
        Symbols              = 1 << 10,
        TravelPlaces         = 1 << 11,
    }
}
