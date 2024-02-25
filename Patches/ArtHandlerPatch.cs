using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace CustomArts.Patches
{
    [HarmonyPatch(typeof(ArtHandler), "NextArt")]
    class ArtHandlerNextArtPatch
    {
        public static bool Prefix()
        {
            CustomArts.instance.LoadRandomArt();
            ArtHandler.instance.volume.profile = ArtHandler.instance.arts[0].profile;
            return false;
        }
    }
}
