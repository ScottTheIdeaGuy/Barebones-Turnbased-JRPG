using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Microsoft.Xna.Framework;

namespace JRPG.Data
{   
    public class MapProjectData
    {
        public string name{get; set;}
        public string backgroundColor{get; set;}
        public Point layerGridDefaultSize{get; set;}
        public Point levelDefaultSize{get; set;}
        public string[] entityTags{get; set;}
        public string[] entities{get; set;}     // placeholder
        public TilesetData[] tilesets{get; set;}
    }
}
