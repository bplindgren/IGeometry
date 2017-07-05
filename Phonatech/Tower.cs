using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonatech
{
    public class Tower
    {
        public string ID { get; set; }

        public string TowerType { get; set; }

        public string NetworkBand { get; set; }

        public double TowerCost { get; set; }

        public double TowerCoverage { get; set; }

        public double TowerHeight { get; set; }

        public double TowerBaseArea { get; set; }

        public IPoint TowerLocation { get; set; }

    }
}
