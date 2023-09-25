using Newtonsoft.Json;

namespace Touhou;

public class Settings {

    public Settings(string settingsPath) {
        var jsonSource = File.ReadAllText(settingsPath);

        JsonConvert.PopulateObject(jsonSource, this);
    }

    [JsonRequired]
    public bool UseSteam { get; private set; }

    [JsonRequired]
    public string Address { get; private set; }

    [JsonRequired]
    public int Port { get; private set; }

    [JsonRequired]
    public ulong SteamID { get; private set; }

    [JsonRequired]
    public float SoundVolume { get; private set; }
}