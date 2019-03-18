using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UOSConfigManager.CFG
{
    public class Config
    {
        [JsonProperty("path")]
        public string UOSPath { get; set; }
        [JsonProperty("pId")]
        public int DefaultProfileID { get; set; }
    }
}
