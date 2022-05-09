using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefensePersistence
{
    /// <summary>
    /// Olyankor dobódik amikor nem megfelelő pálya töltődik be
    /// </summary>
    public class ModelLoadException :Exception
    {

        public ModelLoadException()
        {

        }
    }
}
