using CrystalTable.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace CrystalTable.Logic
{
    /// <summary>
    /// Данные для кеша (перенесено отдельно и сделано public).
    /// </summary>
    [Serializable]
    public class CacheData
    {
        public WaferInfo WaferInfo { get; set; }
        public List<Crystal> Crystals { get; set; }
    }

    /// <summary>
    /// Класс для сохранения и загрузки кристаллов на диск.
    /// </summary>
    public static class CrystalCache
    {
        /// <summary>
        /// Возвращает путь к файлу-кешу для заданных параметров.
        /// </summary>
        public static string GetCacheFilePath(float sizeX, float sizeY, float diameter)
        {
            var storedDataDir = Path.Combine(Directory.GetCurrentDirectory(), "Stored data");
            if (!Directory.Exists(storedDataDir))
                Directory.CreateDirectory(storedDataDir);
            string fileName = $"Crystals_{sizeX}_{sizeY}_{diameter}.xml";
            return Path.Combine(storedDataDir, fileName);
        }

        /// <summary>
        /// Сохраняет коллекцию кристаллов и информацию о пластине.
        /// </summary>
        public static void Save(string path, WaferInfo info, List<Crystal> crystals)
        {
            var serializer = new XmlSerializer(typeof(CacheData));
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, new CacheData { WaferInfo = info, Crystals = crystals });
            }
        }

        /// <summary>
        /// Пытается загрузить кристаллы из кеша.
        /// </summary>
        public static bool TryLoad(string path, out List<Crystal> crystals)
        {
            crystals = null;
            if (!File.Exists(path))
                return false;

            var serializer = new XmlSerializer(typeof(CacheData));
            try
            {
                using (var reader = new StreamReader(path))
                {
                    var data = (CacheData)serializer.Deserialize(reader);
                    crystals = data.Crystals;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
