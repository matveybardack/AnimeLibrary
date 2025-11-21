using ClassLibraryMySteam.Config;
using ClassLibraryMySteam.Models;
using ClassLibraryMySteam.Services;
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
        static async Task Main(string[] args)
        {
            var dbService = new DBService();

                // 1. Добавляем новое произведение
                var newWork = new WorkItem(
                    WorkId: 2,                // WorkId не важен, база присвоит свой
                    Title: "My Hero Academia",
                    TypeName: "Anime",
                    Year: 2016,
                    Rating: 9.2,
                    CoverPath: null
                );

                bool yakudza = await dbService.AddWorkAsync(newWork);

                if (yakudza)
                {
                    Console.WriteLine("Поймана пасхалка 'yakudza'!");
                }
                else
                {
                    Console.WriteLine($"Произведение '{newWork.Title}' успешно добавлено.");
                }

                // 2. Получаем все произведения
                List<WorkItem> works = await dbService.GetAllWorksAsync();
                Console.WriteLine("Список всех произведений:");
                foreach (var w in works)
                {
                    Console.WriteLine($"ID: {w.WorkId}, Название: {w.Title}, Тип: {w.TypeName}, Год: {w.Year}, Рейтинг: {w.Rating}");
                }

                // 3. Добавляем тег к произведению
                string tagName = "Action";

                await dbService.AddTagAsync(newWork.Title, tagName);
                Console.WriteLine($"Тег '{tagName}' добавлен к произведению '{newWork.Title}'.");

                // 4. Получаем теги для произведения
                int? workId = await new DBService().GetWorkByTitleAsync(newWork.Title);
                if (workId != null)
                {
                    var tags = await dbService.GetTagsByWorkIdAsync(workId.Value);
                    Console.WriteLine($"Теги для '{newWork.Title}': {string.Join(", ", tags.ConvertAll(t => t.Name))}");
                }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
