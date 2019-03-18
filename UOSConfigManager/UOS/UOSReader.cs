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
        public FileInfo ConfigFile { get; private set; }
        public UOSReader.ConfigType Config { get; private set; }

        public UOSWriter(FileInfo configFile, UOSReader.ConfigType config = UOSReader.ConfigType.Profile)
        {
            ConfigFile = configFile;
            Config = config;
        }

        public static void Write(Configs.Profile profile, string path)
        {
            XDocument xdoc = new XDocument();
            List<XElement> xelems = new List<XElement>();
            XElement main = new XElement(XName.Get("profile"));

            XElement friends = new XElement(XName.Get("friends"));
            XElement organizer = new XElement(XName.Get("organizer"));
            XElement scavenger = new XElement(XName.Get("scavenger"));
            XElement objects = new XElement(XName.Get("objects"));
            XElement hotkeys = new XElement(XName.Get("hotkeys"));
            XElement macros = new XElement(XName.Get("macros"));
            XElement autoloot = new XElement(XName.Get("autoloot"));
            XElement counters = new XElement(XName.Get("counters"));
            List<XElement> dataElems = new List<XElement>();
            List<XElement> dresslists = new List<XElement>();

            foreach (var x in profile.DataBools)
            {
                XElement data = new XElement(XName.Get("data"));
                XAttribute dataattrib = new XAttribute(XName.Get("name"), x.Value);
                data.Add(dataattrib);
                dataElems.Add(data);
            }
            foreach(var x in profile.DataInts)
            {
                XElement data = new XElement(XName.Get("data"));
                XAttribute dataattrib = new XAttribute(XName.Get("name"), x.Value);
                data.Add(dataattrib);
                dataElems.Add(data);
            }
            dataElems.ForEach(e => main.Add(e));

            foreach(var x in profile.Friends)
            {
                XElement friend = new XElement(XName.Get("friend"), x.ID.ToString("x"));
                XAttribute name = new XAttribute(XName.Get("name"), x.Name ?? "");
                friend.Add(name);
                friends.Add(friend);
            }

            foreach(var x in profile.Counters)
            {
                XElement counter = new XElement(XName.Get("counter"));

                List<XAttribute> attribs = new List<XAttribute>()
                {
                    new XAttribute(XName.Get("format"), x.Format),
                    new XAttribute(XName.Get("enabled"), x.Enabled),
                    new XAttribute(XName.Get("image"), x.Image),
                    new XAttribute(XName.Get("name"), x.Name),
                    new XAttribute(XName.Get("graphic"), x.Graphic.ToString("x")),
                    new XAttribute(XName.Get("color"), x.Color.ToString("x")),
                };
                attribs.ForEach(a => counter.Add(a));
                counters.Add(counter);
            }

            XElement group = new XElement(XName.Get("group"));
            foreach (var x in profile.Organizer)
            {
                XElement organizergroup = new XElement(XName.Get("group"));

                List<XAttribute> attributes = new List<XAttribute>()
                {
                    new XAttribute(XName.Get("stack"), x.Stack),
                    new XAttribute(XName.Get("target"), x.Target),
                    new XAttribute(XName.Get("loop"), x.Loop),
                    new XAttribute(XName.Get("complete"), x.Complete),
                    new XAttribute(XName.Get("name"), x.Name),
                    new XAttribute(XName.Get("source"), x.Source),
                };
                attributes.ForEach(a => organizergroup.Add(a));

                //foreach(var y in x.)
            }








            xelems.ForEach(e => main.Add(e));
            xdoc.Add(main);
            xdoc.Save(path);
        }
    }
}
