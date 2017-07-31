using UnityEngine;
using Verse;

namespace AvoidFriendlyFire
{
    [StaticConstructorOnStartup]
    public class Resources
    {
        public static readonly Texture2D FriendlyFireIcon = ContentFinder<Texture2D>.Get("AvoidFF");
    }
}