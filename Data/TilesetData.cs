using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRPG.Data
{
    public class TilesetData
    {
        public string label {get; set;}
        public string path {get; set;}
        public string image {get; set;}

        public int tileWidth {get; set;}
        public int tileHeight {get; set;}
        public int tileSeparationX {get; set;}
        public int tileSeparationY {get; set;}
        public int tileMarginX {get; set;}
        public int tileMarginY {get; set;}
    }
}
