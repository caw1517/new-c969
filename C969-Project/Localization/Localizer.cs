using System;
using System.Collections.Generic;
using System.Globalization;
using C969_Project.Database;

namespace C969_Project.Localization
{
    //Login form translations. In-memory dictionaries, not .resx - only the login
    //form is localized, the rest of the app stays English.
    public static class Localizer
    {
        //Regions where German is an official language
        private static readonly HashSet<string> GermanRegions =
            new(StringComparer.OrdinalIgnoreCase) { "DE", "AT", "CH", "LI" };

        private static readonly Dictionary<string, string> English = new()
        {
            ["login.title"] = "Login",
            ["login.username"] = "Username",
            ["login.password"] = "Password",
            ["login.button"] = "Log In",
            ["login.error.empty"] = "Please enter both a username and a password.",
            //One generic message - never reveals whether the username or the password was wrong
            ["login.error.invalid"] = "Invalid username or password.",
        };

        private static readonly Dictionary<string, string> German = new()
        {
            ["login.title"] = "Anmeldung",
            ["login.username"] = "Benutzername",
            ["login.password"] = "Passwort",
            ["login.button"] = "Anmelden",
            ["login.error.empty"] = "Bitte geben Sie einen Benutzernamen und ein Passwort ein.",
            ["login.error.invalid"] = "Ungültiger Benutzername oder ungültiges Passwort.",
        };

        //Takes the region as a parameter rather than reading RegionInfo.CurrentRegion,
        public static Language ResolveDefault(RegionInfo region)
        {
            ArgumentNullException.ThrowIfNull(region);

            return GermanRegions.Contains(region.TwoLetterISORegionName)
                ? Language.German
                : Language.English;
        }

        public static string T(string key, Language lang)
        {
            var table = lang == Language.German ? German : English;

            //A missing key is a programmer error, so fail loudly rather than
            //silently returning the key or an empty string.
            if (!table.TryGetValue(key, out var value))
            {
                throw new KeyNotFoundException($"No {lang} translation for login key '{key}'.");
            }

            return value;
        }
    }
}
