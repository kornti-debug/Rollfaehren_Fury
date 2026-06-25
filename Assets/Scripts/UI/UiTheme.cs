using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public static class UiTheme
    {
        public static readonly Color Hull = Hex("111C25");
        public static readonly Color HullSoft = Hex("172833");
        public static readonly Color River = Hex("0A3D4A");
        public static readonly Color Foam = Hex("EDF7F5");
        public static readonly Color Muted = Hex("A9C1BD");
        public static readonly Color Steel = Hex("78949E");
        public static readonly Color Warning = Hex("FFB627");
        public static readonly Color WarningDark = Hex("E48C08");
        public static readonly Color Siren = Hex("F04D3A");
        public static readonly Color Success = Hex("43C59E");
        public static readonly Color Progress = Hex("5DB7DE");

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static Color Hex(string value)
        {
            return ColorUtility.TryParseHtmlString($"#{value}", out Color color)
                ? color
                : Color.white;
        }
    }
}
