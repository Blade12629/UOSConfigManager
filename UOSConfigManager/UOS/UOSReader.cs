using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace UOSConfigManager.UOS
{

    public class UOSReader
    {
        public FileInfo ConfigFile { get; private set; }
        public ConfigType Config { get; private set; }
        public object Result { get; set; }
        public enum ConfigType
        {
            Profile,
            Launcher
        }

        public UOSReader(FileInfo configFile, ConfigType config = ConfigType.Profile)
        {
            ConfigFile = configFile;
            Config = config;
        }
        
        public async Task<object> Read()
        {
            if (!ConfigFile.Exists)
                throw new FileNotFoundException(ConfigFile.FullName);

            XmlSerializer serializer = null;
            object result = null;

            switch (Config)
            {
                default:
                case ConfigType.Profile:
                    serializer = new XmlSerializer(typeof(XML.Profile.Profile));
                    break;
                case ConfigType.Launcher:
                    serializer = new XmlSerializer(typeof(XML.Launcher.Launcher));
                    break;
            }

            using (FileStream fstream = ConfigFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                result = serializer.Deserialize(fstream);
            
            return result;
        }
    }
    public class UOSWriter
    {
        public UOSReader.ConfigType Config { get; private set; }
        public object CfgFile { get; private set; }
        public int ProfileID { get; private set; }

        public UOSWriter(object cfgfile, int profileID, UOSReader.ConfigType config = UOSReader.ConfigType.Profile)
        {
            Config = config;
            CfgFile = cfgfile;
            ProfileID = profileID;
        }

        public void Write()
        {
            XmlSerializer serializer;
            FileInfo destFile;

            switch (Config)
            {
                case UOSReader.ConfigType.Profile:
                    XML.Profile.Profile profile = CfgFile as XML.Profile.Profile;
                    serializer = new XmlSerializer(typeof(XML.Profile.Profile));
                    destFile = Program.profiles[ProfileID];

                    destFile.CopyTo(destFile.FullName + ".backup");
                    using (FileStream fstream = destFile.OpenWrite())
                    {
                        serializer.Serialize(fstream, profile);
                    }
                        break;
                case UOSReader.ConfigType.Launcher:
                    XML.Launcher.Launcher launcher = CfgFile as XML.Launcher.Launcher;
                    serializer = new XmlSerializer(typeof(XML.Launcher.Launcher));
                    destFile = new FileInfo(new DirectoryInfo(Program.Config.UOSPath).Parent.FullName + @"\launcher.xml");

                    destFile.CopyTo(destFile.FullName + ".backup");
                    using (FileStream fstream = destFile.OpenWrite())
                    {
                        serializer.Serialize(fstream, launcher);
                    }
                    break;
            }
        }
    }
}
