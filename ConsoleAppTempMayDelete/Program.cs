using ClassLibraryMySteam.Config;
using ClassLibraryMySteam.Models;
using ClassLibraryMySteam.ViewModels;
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

            Console.WriteLine("=== Пайплайн тестирования DBService ===");

            // ----------------------------
            // 1. Добавляем новое произведение
            // ----------------------------
            var newWork = new WorkItem(
                WorkId: 2,     // WorkId не важен, его присвоит база
                Title: "My Hero Academia 2",
                TypeName: "Anime",
                Year: 2016,
                Rating: 9.2,
                CoverPath: null,
                Series: 24
            );

            //bool yakudza = await dbService.AddWorkAsync(newWork);

            //if (yakudza)
            //    Console.WriteLine("Поймана пасхалка 'yakudza'!");
            //else
            //    Console.WriteLine($"Произведение '{newWork.Title}' успешно добавлено.");

            // ----------------------------
            // 2. Получаем все произведения
            // ----------------------------
            List<WorkItem> works = await dbService.GetAllWorksAsync();

            Console.WriteLine("\nСписок всех произведений:");
            foreach (var w in works)
                Console.WriteLine($"ID: {w.WorkId}, Название: {w.Title}, Тип: {w.TypeName}, Год: {w.Year}, Рейтинг: {w.Rating}");

            // ----------------------------
            // 3. Добавляем тег к произведению
            // ----------------------------
            //string tagName = "Action";

            //await dbService.AddTagAsync(newWork.Title, tagName);
            //Console.WriteLine($"\nТег '{tagName}' добавлен к произведению '{newWork.Title}'.");

            // ----------------------------
            // 4. Получаем теги произведения
            // ----------------------------
            int? workId = await new DBService().GetWorkByTitleAsync(newWork.Title);
            if (workId != null)
            {
                var tags = await dbService.GetTagsByWorkIdAsync(workId.Value);
                Console.WriteLine($"Теги для '{newWork.Title}': {string.Join(", ", tags.ConvertAll(t => t.Name))}");
            }

            // ============================================================
            // 5. ПРОВЕРКА СЛОЖНЫХ ФИЛЬТРОВ
            // ============================================================
            Console.WriteLine("\n=== Проверка сложных пользовательских фильтров ===");

            var filter = new WorkFilter
            {
                TypeName = "anime",

                RatingOperator = ">=",
                RatingValue = 8.5,

                SeriesOperator = "<=",
                SeriesValue = 50,

                Tags = new List<string> { "action", "fantasy" },
                TagMode = TagFilterMode.Or,          // Ищем работы, содержащие оба тега
                Limit = 20
            };

            Console.WriteLine("Фильтр установлен, выполняю запрос...");

            var filteredWorks = await dbService.GetFilteredWorksAsync(filter);

            Console.WriteLine("Результат фильтрации:");
            foreach (var w in filteredWorks)
                Console.WriteLine($"ID:{w.WorkId}  {w.Title}  {w.TypeName}  Rating:{w.Rating}  Series:{w.Series}");

            // ============================================================
            // 6. ПРОВЕРКА РАСПИСАНИЯ
            // ============================================================
            Console.WriteLine("\n=== Генерация расписания ===");

            // Возьмём любое произведение из списка, либо newWork
            var scheduleWork = filteredWorks.FirstOrDefault() ?? newWork;

            Console.WriteLine($"Создаем расписание для: {scheduleWork.Title}");
            Console.Write("Введите количество серий в день: ");
            int episodesPerDay = int.Parse(Console.ReadLine() ?? "3");

            Console.Write("Введите день недели начала (или Enter для текущего): ");
            string? dayInput = Console.ReadLine();

            DayOfWeek? startDay = null;
            if (!string.IsNullOrWhiteSpace(dayInput) &&
                Enum.TryParse<DayOfWeek>(dayInput, true, out var parsed))
            {
                startDay = parsed;
            }

            var schedule = LogicService.GenerateSchedule(scheduleWork, episodesPerDay, startDay);

            Console.WriteLine("\nВаше расписание:");
            foreach (var entry in schedule)
            {
                Console.WriteLine($"{entry.Key}: серии {string.Join(", ", entry.Value)}");
            }

            // ============================================================
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
