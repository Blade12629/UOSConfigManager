using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace UOSConfigManager.UOS.XML.Launcher
{
    [XmlRoot(ElementName = "option")]
    public class Option
    {
        [XmlAttribute(AttributeName = "checked")]
        public string Checked { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "path")]
    public class Path
    {
        [XmlAttribute(AttributeName = "last")]
        public string Last { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlAttribute(AttributeName = "removeEncryption")]
        public string RemoveEncryption { get; set; }
    }

    [XmlRoot(ElementName = "installs")]
    public class Installs
    {
        [XmlElement(ElementName = "path")]
        public Path Path { get; set; }
    }

    [XmlRoot(ElementName = "clients")]
    public class Clients
    {
        [XmlElement(ElementName = "path")]
        public List<Path> Path { get; set; }
    }

    [XmlRoot(ElementName = "shard")]
    public class Shard
    {
        [XmlAttribute(AttributeName = "login")]
        public string Login { get; set; }
        [XmlAttribute(AttributeName = "last")]
        public string Last { get; set; }
        [XmlAttribute(AttributeName = "port")]
        public string Port { get; set; }
    }

    [XmlRoot(ElementName = "servers")]
    public class Servers
    {
        [XmlElement(ElementName = "shard")]
        public List<Shard> Shard { get; set; }
    }

    [XmlRoot(ElementName = "launcher")]
    public class Launcher
    {
        [XmlElement(ElementName = "option")]
        public Option Option { get; set; }
        [XmlElement(ElementName = "installs")]
        public Installs Installs { get; set; }
        [XmlElement(ElementName = "clients")]
        public Clients Clients { get; set; }
        [XmlElement(ElementName = "servers")]
        public Servers Servers { get; set; }
    }
}
