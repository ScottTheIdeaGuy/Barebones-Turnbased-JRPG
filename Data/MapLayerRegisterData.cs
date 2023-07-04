using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace JRPG.Data
{
    public class MapLayerRegisterData
    {
        public string definition;
        public string name;
        public Point gridSize;
        private string exportID;
        private int exportMode;
        private int arrayMode;
        public string defaultTileset;
    }
}
