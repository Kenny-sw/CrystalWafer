using CrystalTable.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrystalTable.Logic
{
    internal class CrystalManager
    {
        // Коллекция для хранения всех кристаллов на пластине
        private List<Crystal> crystals = new List<Crystal>();

        public List<Crystal> Crystals => crystals;   //вместо полного определения get { return crystals; } записывается => crystals;


        public static CrystalManager Instance = new CrystalManager();

        private CrystalManager()
        {            
        }
    }
}
