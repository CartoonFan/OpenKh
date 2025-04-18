using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class ConfigurationService
    {
        private class Config
        {
            private static readonly IDeserializer _deserializer =
                new DeserializerBuilder()
                .IgnoreFields()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            private static readonly ISerializer _serializer =
                new SerializerBuilder()
                .IgnoreFields()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            public bool IsFirstRunComplete { get; set; }
            public string ModCollectionPath { get; internal set; }
            public string GameModPath { get; internal set; }
            public string GameDataPath { get; internal set; }
            public int GameEdition { get; internal set; }
            public string IsoLocation { get; internal set; }
            public string OpenKhGameEngineLocation { get; internal set; }
            public string Pcsx2Location { get; internal set; }
            public string PcReleaseLocation { get; internal set; }
            public int RegionId { get; internal set; }
            public bool BypassLauncher { get; internal set; }
            public string EpicGamesUserID { get; internal set; }

            public void Save(string fileName)
            {
                using var writer = new StreamWriter(fileName);
                _serializer.Serialize(writer, this);
            }

            public static Config Open(string fileName)
            {
                if (!File.Exists(fileName))
                    return new Config();

                using var reader = new StreamReader(fileName);
                return _deserializer.Deserialize<Config>(reader);
            }
        }

        private static string StoragePath = Directory.GetCurrentDirectory().IndexOf("system32") >= 0 ?
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) :
            Directory.GetCurrentDirectory();
        private static string ConfigPath = Path.Combine(StoragePath, "mods-manager.yml");
        private static string EnabledModsPath = Path.Combine(StoragePath, "mods.txt");
        private static readonly Config _config = Config.Open(ConfigPath);

        static ConfigurationService()
        {
            if (!Directory.Exists(ModCollectionPath))
                Directory.CreateDirectory(ModCollectionPath);

            Task.Run(async () =>
            {
                using var client = new HttpClient();
                var response = await client
                    .GetAsync("https://raw.githubusercontent.com/OpenKH/mods-manager-feed/main/featured.txt");
                FeaturedMods = (await response.Content.ReadAsStringAsync())
                    .Split("\n")
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            });

            Task.Run(async () =>
            {
                using var client = new HttpClient();
                var response = await client
                    .GetAsync("https://raw.githubusercontent.com/OpenKH/mods-manager-feed/main/deny.txt");
                BlacklistedMods = (await response.Content.ReadAsStringAsync())
                    .Split("\n")
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            });
        }

        public static ICollection<string> EnabledMods
        {
            get => File.Exists(EnabledModsPath) ? File.ReadAllLines(EnabledModsPath) : new string[0];
            set => File.WriteAllLines(EnabledModsPath, value);
        }

        public static ICollection<string> FeaturedMods { get; private set; }
        public static ICollection<string> BlacklistedMods { get; private set; }

        public static bool IsFirstRunComplete
        {
            get => _config.IsFirstRunComplete;
            set
            {
                _config.IsFirstRunComplete = value;
                _config.Save(ConfigPath);
            }
        }

        public static string ModCollectionPath
        {
            get => _config.ModCollectionPath ?? Path.GetFullPath(Path.Combine(StoragePath, "mods"));
            set
            {
                _config.ModCollectionPath = value;
                _config.Save(ConfigPath);
            }
        }

        public static string GameModPath
        {
            get => _config.GameModPath ?? Path.GetFullPath(Path.Combine(StoragePath, "mod"));
            set
            {
                _config.GameModPath = value;
                _config.Save(ConfigPath);
            }
        }

        public static string GameDataLocation
        {
            get => _config.GameDataPath ?? Path.GetFullPath(Path.Combine(StoragePath, "data"));
            set
            {
                _config.GameDataPath = value;
                _config.Save(ConfigPath);
            }
        }

        public static int GameEdition
        {
            get => _config.GameEdition;
            set
            {
                _config.GameEdition = value;
                _config.Save(ConfigPath);
            }
        }

        public static string IsoLocation
        {
            get => _config.IsoLocation;
            set
            {
                _config.IsoLocation = value;
                _config.Save(ConfigPath);
            }
        }

        public static string OpenKhGameEngineLocation
        {
            get => _config.OpenKhGameEngineLocation ?? Path.GetFullPath("OpenKh.Game.exe");
            set
            {
                _config.OpenKhGameEngineLocation = value;
                _config.Save(ConfigPath);
            }
        }

        public static string Pcsx2Location
        {
            get => _config.Pcsx2Location;
            set
            {
                _config.Pcsx2Location = value;
                _config.Save(ConfigPath);
            }
        }

        public static string PcReleaseLocation
        {
            get => _config.PcReleaseLocation;
            set
            {
                _config.PcReleaseLocation = value;
                _config.Save(ConfigPath);
            }
        }

        public static int RegionId
        {
            get => _config.RegionId;
            set
            {
                _config.RegionId = value;
                _config.Save(ConfigPath);
            }
        }

        public static bool BypassLauncher
        {
            get => _config.BypassLauncher;
            set
            {
                _config.BypassLauncher = value;
                _config.Save(ConfigPath);
            }
        }

        public static string EpicGamesUserID
        {
            get => _config.EpicGamesUserID;
            set
            {
                _config.EpicGamesUserID = value;
                _config.Save(ConfigPath);
            }
        }
    }
}
