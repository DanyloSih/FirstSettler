using UnityEngine;

namespace FirstSettler.Extensions
{
    public static class UnityColorExtensions
    {
        public static Color Randomize(this Color color, bool randomizeAlphaChanel = false)
        {
            if (randomizeAlphaChanel)
            {
                color.a = Random.value;
            }

            color.r = Random.value;
            color.g = Random.value;
            color.b = Random.value;

            return color;
        }
    }
}