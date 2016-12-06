using System.Drawing;
using System.Linq;

namespace Picross
{
    public static class GameMath
    {
        // Float stuff
        public static float Clamp(float min, float max, float value) {
            if (value <= min)
                return min;
            if (value >= max)
                return max;
            return value;
        }

        public static float Lerp(float from, float to, float t) {
            return from + (to - from) * GameMath.Clamp(0, 1, t);
        }

        // Color stuff
        public static Color Clamp(float min, float max, Color color, bool alpha = false) {
            return Color.FromArgb(
                alpha ? (byte)Clamp(min, max, color.A) : color.A,
                (byte)Clamp(min, max, color.R),
                (byte)Clamp(min, max, color.G),
                (byte)Clamp(min, max, color.B)
            );
        }

        public static Color Lerp(Color from, Color to, float t, bool alpha = false) {
            return Color.FromArgb(
                alpha ? (byte)Lerp(from.A, to.A, t) : from.A,
                (byte)Lerp(from.R, to.R, t),
                (byte)Lerp(from.G, to.G, t),
                (byte)Lerp(from.B, to.B, t)
            );
        }

        public static bool IsOneOf<T>(this T enumerator, params T[] values) {
            return values.Contains(enumerator);
        }
    }
}
