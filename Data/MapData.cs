using System;
using System.Collections.Generic;

namespace JRPG.Data
{
    public class MapData
    {
        public int width {get; set;}
        public int height {get; set;}

        public int offsetX {get; set;}
        public int offsetY {get; set;}

        public MapLayerData[] layers {get; set;}
    }
}
