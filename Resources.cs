using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    [StaticConstructorOnStartup]
    public class Resources
    {
        public static Texture2D FriendlyFireIcon = ContentFinder<Texture2D>.Get("UI/Commands/ReleaseAnimals");
    }
}