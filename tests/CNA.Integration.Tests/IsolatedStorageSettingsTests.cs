using System.IO.IsolatedStorage;
using Xunit;

namespace CnaCs.Integration.Tests.Compat;

/// <summary>
/// CSX-132: Windows Phone's IsolatedStorageSettings, which Windows Phone 7 Game Development's
/// GameFramework (chapters 9 and 10) keeps its settings and high scores in.
/// </summary>
[Collection(global::CNA.Integration.Tests.OwnGameCollection.Name)]
public class IsolatedStorageSettingsTests
{
    public class HighScore
    {
        public string Name { get; set; } = "";

        public int Score { get; set; }
    }

    [Fact]
    public void Settings_BehaveAsTheirDictionaryDid()
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        settings.Clear();
        try
        {
            settings.Add("volume", 7);
            Assert.True(settings.Contains("volume"));
            Assert.Throws<ArgumentException>(() => settings.Add("volume", 8));
            Assert.Throws<KeyNotFoundException>(() => settings["missing"]);
            settings["volume"] = 9;
            Assert.True(settings.TryGetValue("volume", out int volume));
            Assert.Equal(9, volume);
            Assert.Throws<InvalidCastException>(() => settings.TryGetValue("volume", out string _));
            Assert.False(settings.TryGetValue("missing", out int _));
            Assert.True(settings.Remove("volume"));
            Assert.Equal(0, settings.Count);
        }
        finally
        {
            settings.Clear();
        }
    }

    [Fact]
    public void SavedSettings_AreWhatTheNextRunReads()
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        settings.Clear();
        try
        {
            settings["name"] = "Cosmic Rocks";
            settings["level"] = 3;
            settings["best"] = new HighScore { Name = "ABC", Score = 1200 };
            settings.Save();

            Dictionary<string, object> read = IsolatedStorageSettings.Load();
            Assert.Equal("Cosmic Rocks", read["name"]);
            Assert.Equal(3, read["level"]);
            var best = Assert.IsType<HighScore>(read["best"]);
            Assert.Equal(("ABC", 1200), (best.Name, best.Score));
        }
        finally
        {
            settings.Clear();
            settings.Save();
        }
    }
}
