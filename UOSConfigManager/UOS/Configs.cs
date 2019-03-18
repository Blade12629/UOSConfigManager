using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOSConfigManager.UOS.Configs
{
    public class Launcher
    {
        public string LastUpdate { get; set; }
        public Path[] Installs { get; set; }
        public Client[] Client { get; set; }
        public Shard[] Servers { get; set; }
    }

    public class Path
    {
        public string Name { get; set; }
        public bool? Last { get; set; }
    }
    public class Client
    {
        public bool? Last { get; set; }
        public string Path { get; set; }
        public bool RemoveEncryption { get; set; }
    }
    public class Shard
    {
        public bool? Last { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
    }



    public class Profile
    {
        public KeyValuePair<string, bool>[] DataBools { get; set; }
        public KeyValuePair<string, int>[] DataInts { get; set; }
        public Friend[] Friends { get; set; }
        public Scavenger Scavenger { get; set; }
        public _Object[] Objects { get; set; }
        public Hotkey[] Hotkeys { get; set; }
        public Macro[] Macros { get; set; }
        public Autoloot AutoLoot { get; set; }
        public Counter[] Counters { get; set; }
        public OrganizerGroup[] Organizer { get; set; }
        public Dresslist[] Dresslists { get; set; }
    }

    public class Friend
    {
        public string Name { get; set; }
        public int ID { get; set; }
    }
    public class Scavenger
    {
        public bool Enabled { get; set; }
        public ScavengerItem[] ScavengerItems { get; set; }
    }
    public class ScavengerItem
    {
        public bool Enabled { get; set; }
        public int Graphic { get; set; }
        public int Color { get; set; }
    }
    public class _Object
    {
        public string Name { get; set; }
        public int ID { get; set; }
    }
    public class Hotkey
    {
        public string VirtualKey { get; set; }
        public string Key { get { return new System.Windows.Forms.KeysConverter().ConvertToString(Program.ParseHex(VirtualKey)); } }
        public bool Pass { get; set; }
        public string Action { get; set; }
        public string Param { get; set; }
    }
    public class Macro
    {
        public bool Interrupt { get; set; }
        public bool Loop { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
    public class Autoloot
    {
        public bool Enabled { get; set; }
        public int Container { get; set; }
        public bool Guards { get; set; }
    }
    /* TODO: Vendors */
    public class Counter
    {
        public string Format { get; set; }
        public bool Enabled { get; set; }
        public bool Image { get; set; }
        public string Name { get; set; }
        public int Graphic { get; set; }
        public int Color { get; set; }
    }
    public class OrganizerGroup
    {
        public bool Stack { get; set; }
        public int Target { get; set; }
        public bool Loop { get; set; }
        public string Name { get; set; }
        public int Source { get; set; }
        public bool Complete { get; set; }
    }
    public class Dresslist
    {
        public int ContainerID { get; set; }
        public string Name { get; set; }
        public Item[] Items { get; set; }

        public class Item
        {
            public int ID { get; set; }
            public int Layer { get; set; }
        }
    }
}
