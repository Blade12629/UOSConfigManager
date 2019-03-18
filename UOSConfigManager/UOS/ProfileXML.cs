using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace UOSConfigManager.UOS.XML.Profile
{
    [XmlRoot(ElementName = "data")]
    public class Data
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "friend")]
    public class Friend
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "friends")]
    public class Friends
    {
        [XmlElement(ElementName = "friend")]
        public List<Friend> Friend { get; set; }
    }

    [XmlRoot(ElementName = "counter")]
    public class Counter
    {
        [XmlAttribute(AttributeName = "format")]
        public string Format { get; set; }
        [XmlAttribute(AttributeName = "enabled")]
        public string Enabled { get; set; }
        [XmlAttribute(AttributeName = "image")]
        public string Image { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "graphic")]
        public string Graphic { get; set; }
        [XmlAttribute(AttributeName = "color")]
        public string Color { get; set; }
    }

    [XmlRoot(ElementName = "counters")]
    public class Counters
    {
        [XmlElement(ElementName = "counter")]
        public List<Counter> Counter { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlAttribute(AttributeName = "amount")]
        public string Amount { get; set; }
        [XmlAttribute(AttributeName = "graphic")]
        public string Graphic { get; set; }
        [XmlAttribute(AttributeName = "layer")]
        public string Layer { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "group")]
    public class Group
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
        [XmlAttribute(AttributeName = "stack")]
        public string Stack { get; set; }
        [XmlAttribute(AttributeName = "target")]
        public string Target { get; set; }
        [XmlAttribute(AttributeName = "loop")]
        public string Loop { get; set; }
        [XmlAttribute(AttributeName = "complete")]
        public string Complete { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "source")]
        public string Source { get; set; }
    }

    [XmlRoot(ElementName = "organizer")]
    public class Organizer
    {
        [XmlElement(ElementName = "group")]
        public List<Group> Group { get; set; }
    }

    [XmlRoot(ElementName = "scavenge")]
    public class Scavenge
    {
        [XmlAttribute(AttributeName = "enabled")]
        public string Enabled { get; set; }
        [XmlAttribute(AttributeName = "graphic")]
        public string Graphic { get; set; }
        [XmlAttribute(AttributeName = "color")]
        public string Color { get; set; }
    }

    [XmlRoot(ElementName = "scavenger")]
    public class Scavenger
    {
        [XmlElement(ElementName = "scavenge")]
        public List<Scavenge> Scavenge { get; set; }
        [XmlAttribute(AttributeName = "enabled")]
        public string Enabled { get; set; }
    }

    [XmlRoot(ElementName = "obj")]
    public class Obj
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "objects")]
    public class Objects
    {
        [XmlElement(ElementName = "obj")]
        public List<Obj> Obj { get; set; }
    }

    [XmlRoot(ElementName = "hotkey")]
    public class Hotkey
    {
        [XmlAttribute(AttributeName = "action")]
        public string Action { get; set; }
        [XmlAttribute(AttributeName = "pass")]
        public string Pass { get; set; }
        [XmlAttribute(AttributeName = "param")]
        public string Param { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
    }

    [XmlRoot(ElementName = "hotkeys")]
    public class Hotkeys
    {
        [XmlElement(ElementName = "hotkey")]
        public List<Hotkey> Hotkey { get; set; }
    }

    [XmlRoot(ElementName = "macro")]
    public class Macro
    {
        [XmlAttribute(AttributeName = "interrupt")]
        public string Interrupt { get; set; }
        [XmlAttribute(AttributeName = "loop")]
        public string Loop { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "macros")]
    public class Macros
    {
        [XmlElement(ElementName = "macro")]
        public List<Macro> Macro { get; set; }
    }

    [XmlRoot(ElementName = "autoloot")]
    public class Autoloot
    {
        [XmlElement(ElementName = "enabled")]
        public string Enabled { get; set; }
        [XmlElement(ElementName = "container")]
        public string Container { get; set; }
        [XmlElement(ElementName = "guards")]
        public string Guards { get; set; }
    }

    [XmlRoot(ElementName = "buystate")]
    public class Buystate
    {
        [XmlAttribute(AttributeName = "enabled")]
        public string Enabled { get; set; }
        [XmlAttribute(AttributeName = "list")]
        public string List { get; set; }
    }

    [XmlRoot(ElementName = "sellstate")]
    public class Sellstate
    {
        [XmlAttribute(AttributeName = "enabled")]
        public string Enabled { get; set; }
        [XmlAttribute(AttributeName = "list")]
        public string List { get; set; }
    }

    [XmlRoot(ElementName = "vendors")]
    public class Vendors
    {
        [XmlElement(ElementName = "buystate")]
        public Buystate Buystate { get; set; }
        [XmlElement(ElementName = "sellstate")]
        public Sellstate Sellstate { get; set; }
    }

    [XmlRoot(ElementName = "dresslist")]
    public class Dresslist
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "container")]
        public string Container { get; set; }
    }

    [XmlRoot(ElementName = "profile")]
    public class Profile
    {
        [XmlElement(ElementName = "data")]
        public List<Data> Data { get; set; }
        [XmlElement(ElementName = "friends")]
        public Friends Friends { get; set; }
        [XmlElement(ElementName = "counters")]
        public Counters Counters { get; set; }
        [XmlElement(ElementName = "spellgrid")]
        public string Spellgrid { get; set; }
        [XmlElement(ElementName = "organizer")]
        public Organizer Organizer { get; set; }
        [XmlElement(ElementName = "scavenger")]
        public Scavenger Scavenger { get; set; }
        [XmlElement(ElementName = "autosearchexemptions")]
        public string Autosearchexemptions { get; set; }
        [XmlElement(ElementName = "objects")]
        public Objects Objects { get; set; }
        [XmlElement(ElementName = "hotkeys")]
        public Hotkeys Hotkeys { get; set; }
        [XmlElement(ElementName = "macros")]
        public Macros Macros { get; set; }
        [XmlElement(ElementName = "autoloot")]
        public Autoloot Autoloot { get; set; }
        [XmlElement(ElementName = "vendors")]
        public Vendors Vendors { get; set; }
        [XmlElement(ElementName = "dresslist")]
        public List<Dresslist> Dresslist { get; set; }
    }

}
