using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Picross.Properties;

namespace Picross.Helpers
{
    public enum StatusIcon { None, Success, Warning, Error };

    public static class ExtensionHelper
    {
        public static bool IsOneOf<T>(this T enumerator, params T[] values) {
            return values.Contains(enumerator);
        }

        public static MessageBoxIcon ToMessageBoxIcon(this StatusIcon icon) {
            switch (icon) {
            case StatusIcon.Success:
                return MessageBoxIcon.Information;
            case StatusIcon.Warning:
                return MessageBoxIcon.Warning;
            case StatusIcon.Error:
                return MessageBoxIcon.Error;
            default:
                return MessageBoxIcon.None;
            }
        }

        public static Bitmap ToBitmap(this StatusIcon icon) {
            switch (icon) {
            case StatusIcon.Success:
                return Resources.Success;
            case StatusIcon.Warning:
                return Resources.Warning;
            case StatusIcon.Error:
                return Resources.Error;
            default:
                return null;
            }
        }
    }
}
