using System.Globalization;
using UnityEngine;

namespace _Project.Scripts.Core
{
    /// <summary>
    /// Long persistence via string (PlayerPrefs has no long API).
    /// </summary>
    public static class PlayerPrefsLong
    {
        public static long Get(string key, long defaultValue)
        {
            var raw = PlayerPrefs.GetString(key, string.Empty);
            return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : defaultValue;
        }

        public static void Set(string key, long value)
        {
            PlayerPrefs.SetString(key, value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
