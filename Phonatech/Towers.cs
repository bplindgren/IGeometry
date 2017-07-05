using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonatech
{
    public class Towers
    {
        public Towers()
        {
            Items = new List<Tower>();
        }

        /// <summary>
        /// the list of towers
        /// </summary>
        public List<Tower> Items { get; set; }
    }
}
