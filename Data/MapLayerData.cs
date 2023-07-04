using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JRPG.Data
{
    public class MapLayerData
    {
        public string name {get; set;}

        public int offsetX {get; set;}
        public int offsetY {get; set;}
        public int gridCellWidth {get; set;}
        public int gridCellHeight {get; set;}
        public int gridCellsX {get; set;}
        public int gridCellsY {get; set;}
        public string tileset {get; set;}
        public int[] data {get; set;}
    }
}
