using Fragsurf.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Bunnyhop : BaseGamemode
    {

        protected override void _Load(FSGameLoop game)
        {
        }
        protected override void _Unload(FSGameLoop game)
        {
        }

        public static string FormatTime(int milliseconds)
        {
            return FormatTime(milliseconds / 1000f);
        }

        public static string FormatTime(float time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"mm\:ss\:fff");
        }

    }
}

