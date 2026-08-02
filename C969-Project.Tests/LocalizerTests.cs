using System.Collections.Generic;
using System.Globalization;
using C969_Project.Database;
using C969_Project.Localization;
using Xunit;

namespace C969_Project.Tests
{
    public class LocalizerTests
    {
        [Fact]
        public void ResolveDefault_ReturnsGerman_ForGermanSpeakingRegion()
        {
            var language = Localizer.ResolveDefault(new RegionInfo("DE"));

            Assert.Equal(Language.German, language);
        }

        [Fact]
        public void ResolveDefault_ReturnsGerman_ForAustrianRegion()
        {
            var language = Localizer.ResolveDefault(new RegionInfo("AT"));

            Assert.Equal(Language.German, language);
        }

        [Fact]
        public void ResolveDefault_ReturnsEnglish_ForNonGermanSpeakingRegion()
        {
            var language = Localizer.ResolveDefault(new RegionInfo("US"));

            Assert.Equal(Language.English, language);
        }

        [Fact]
        public void T_ReturnsEnglishString_ForKnownKey()
        {
            var text = Localizer.T("login.button", Language.English);

            Assert.Equal("Log In", text);
        }

        [Fact]
        public void T_ReturnsGermanString_ForKnownKey()
        {
            var text = Localizer.T("login.button", Language.German);

            Assert.Equal("Anmelden", text);
        }

        [Fact]
        public void T_ThrowsKeyNotFound_ForUnknownKey()
        {
            Assert.Throws<KeyNotFoundException>(
                () => Localizer.T("login.does.not.exist", Language.English));
        }
    }
}
