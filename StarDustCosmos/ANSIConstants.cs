using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDustCosmos
{
    public static class ANSIConstants
    {
        public static Dictionary<string, Color> Foregrounds = new()
        {
            {"39",Color.LightGray},
            {"30",Color.Black},
            {"31",Color.Red},
            {"32",Color.Green},
            {"33",Color.Yellow},
            {"34",Color.Blue},
            {"35",Color.Magenta},
            {"36",Color.Cyan},
            {"37",Color.White},
        };

        public static Dictionary<string, Color> Backgrounds = new()
        {
            {"49",Color.Transparent},
            {"40",Color.Black},
            {"41",Color.Red},
            {"42",Color.Green},
            {"43",Color.Yellow},
            {"44",Color.Blue},
            {"45",Color.Magenta},
            {"46",Color.Cyan},
            {"47",Color.White},
        };
    }
}
