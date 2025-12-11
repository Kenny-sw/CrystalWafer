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
        private readonly List<Crystal> crystals = new List<Crystal>();

        public List<Crystal> Crystals => crystals;

        // ✅ ИСПРАВЛЕНО: readonly поле для thread-safe singleton
        private static readonly CrystalManager _instance = new CrystalManager();
        public static CrystalManager Instance => _instance;

        private CrystalManager()
        {            
        }
    }
}
