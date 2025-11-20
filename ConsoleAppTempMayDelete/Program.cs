using ClassLibraryMySteam.Services;
using ClassLibraryMySteam.Config;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTempMayDelete
{
    internal class Program
    {
        static async Task Main()
        {
            var db = new Connection();

            // Получаем все произведения
            var works = await db.GetAllWorksAsync();
            Console.WriteLine("\nСписок всех произведений:");
            foreach (var w in works)
                Console.WriteLine($"{w.WorkId}: {w.Title} ({w.TypeName}) [{w.Year}] Рейтинг: {w.Rating}");

            // Получаем теги для первого произведения
            if (works is not null)
            {
                var firstWork = works.GetEnumerator();
                if (firstWork.MoveNext())
                {
                    int workId = firstWork.Current.WorkId;
                    var tags = await db.GetTagsByWorkIdAsync(workId);
                    Console.WriteLine($"\nТеги для произведения '{firstWork.Current.Title}':");
                    foreach (var tag in tags)
                        Console.WriteLine(tag.Name);
                }
            }

            Console.WriteLine("\nПроверка завершена. Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}
