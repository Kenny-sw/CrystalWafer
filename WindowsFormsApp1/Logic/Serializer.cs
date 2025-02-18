using CrystalTable.Data;              
using System.IO;                      
using System.Xml.Serialization;       

namespace CrystalTable.Logic
{
    // Внутренний класс для сериализации объектов в XML
    internal class Serializer
    {
        // Метод для сериализации объекта типа WaferInfo в XML-файл
        public void Serialize(WaferInfo waferInfo)
        {
            // Создаём XML-сериализатор для типа WaferInfo
            var xmlSerializer = new XmlSerializer(typeof(WaferInfo));

            // Сериализуем объект в XML и записываем его в файл
            using (var writer = new StreamWriter(GetFilePath()))
            {
                // Процесс сериализации: объект waferInfo преобразуется в XML и записывается в writer
                xmlSerializer.Serialize(writer, waferInfo);
            } // После завершения using, writer автоматически закрывается и освобождает ресурсы
        }

        // Метод для получения пути к файлу, в который будет сохраняться XML
        private static string GetFilePath()
        {
            // Формируем путь к папке "Stored data" в текущей директории приложения
            var storedDataDir = Path.Combine(Directory.GetCurrentDirectory(), "Stored data");

            // Если такой папки не существует, создаём её
            if (!Directory.Exists(storedDataDir))
            {
                Directory.CreateDirectory(storedDataDir);
            }

            // Возвращаем полный путь к файлу "WaferInfo.xml" в папке "Stored data"
            return Path.Combine(storedDataDir, "WaferInfo.xml");
        }
    }
}
