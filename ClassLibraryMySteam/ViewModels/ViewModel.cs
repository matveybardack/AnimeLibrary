using ClassLibraryMySteam.Models;
using ClassLibraryMySteam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.ViewModels
{
    public class ViewModel
    {
        private readonly DBService _dbService;

        public ViewModel()
        {
            _dbService = new DBService();
        }

        public List<ProcessingMode> GetProcessingModes()
        {
            return new List<ProcessingMode>
            {
                new ProcessingMode { Name = "Количество аниме с рейтингом > N", Description = "Подсчет аниме с высоким рейтингом" },
                new ProcessingMode { Name = "Поиск аниме с максимальным рейтингом", Description = "Нахождение самого популярного аниме" }
            };
        }

        /// <summary>
        /// Собирает условие фильтрации для SQL запроса
        /// </summary>
        /// <param name="filterType">имя типа</param>
        /// <param name="filterRating">порог рейтинга</param>
        /// <param name="selectedOperator">оператор сравнения с рейтингом</param>
        /// <returns>строку условия WHERE ... AND ...</returns>
        public string BuildFilterCondition(string filterType, string filterRating, string selectedOperator)
        {
            return $"WHERE genre = '{filterType}' AND rating {selectedOperator} {filterRating}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <returns></returns>
        public async Task<List<WorkItem>> ApplyFilterToDatabase(string filterCondition)
        {
            return await _dbService.GetWorksByFilterAsync(filterCondition);
        }

        /// <summary>
        /// Получает из базы заданное количество аниме с рейтингом выше порога
        /// </summary>
        /// <param name="animeCount"></param>
        /// <param name="ratingThreshold"></param>
        /// <returns></returns>
        public async Task<List<WorkItem>> GetAnimeFromDatabase(int animeCount, double ratingThreshold)
        {
            return await _dbService.GetWorksByRatingAndLimitAsync(ratingThreshold, animeCount);
        }

        /// <summary>
        /// Циклическая обработка одного шага
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="collection"></param>
        /// <param name="currentIndex"></param>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public (int newIndex, object newState, string result, bool completed) ExecuteLoopStep(ProcessingMode mode, List<WorkItem> collection, int currentIndex, object currentState)
        {
            if (currentIndex >= collection.Count)
                return (currentIndex, currentState, "Completed", true);

            var item = collection[currentIndex];
            object newState = null;
            string result = "";

            if (mode.Name == "Количество аниме с рейтингом > N")
            {
                int count = (int)(currentState ?? 0);
                if (item.Rating > 7) // Assuming N is 7 for now
                {
                    count++;
                }
                newState = count;
                result = $"Current count: {count}";
            }
            else if (mode.Name == "Поиск аниме с максимальным рейтингом")
            {
                WorkItem maxRated = (WorkItem)currentState;
                if (maxRated == null || item.Rating > maxRated.Rating)
                {
                    maxRated = item;
                }
                newState = maxRated;
                result = $"Max rated anime so far: {maxRated.Title}";
            }

            return (currentIndex + 1, newState, result, false);
        }

        /// <summary>
        /// Циклическая автоматическая обработка всей коллекции
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public Task StartAutoProcessing(ProcessingMode mode, List<WorkItem> collection)
        {
            return Task.Run(() =>
            {
                int index = 0;
                object state = null;
                bool completed = false;
                while (!completed)
                {
                    var stepResult = ExecuteLoopStep(mode, collection, index, state);
                    index = stepResult.newIndex;
                    state = stepResult.newState;
                    completed = stepResult.completed;
                    // Add a delay to visualize the process
                    Task.Delay(100).Wait();
                }
            });
        }

        /// <summary>
        /// Сбрасывает состояние обработки
        /// </summary>
        /// <returns></returns>
        public (int index, object state, string result) ResetProcessing()
        {
            return (0, null, "");
        }

        /// <summary>
        /// Информация об инварианте
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public (string description, string formula) GetInvariantInfo(ProcessingMode mode)
        {
            if (mode.Name == "Количество аниме с рейтингом > N")
            {
                return ("Количество найденных аниме с рейтингом > N среди просмотренных элементов.", "count = |{i | 0 <= i < currentIndex and collection[i].Rating > N}|");
            }
            else if (mode.Name == "Поиск аниме с максимальным рейтингом")
            {
                return ("Найденное аниме имеет максимальный рейтинг среди просмотренных.", "maxRated = argmax(collection[0..currentIndex-1])");
            }
            return ("", "");
        }

        /// <summary>
        /// Вариант функции
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string GetVariantFunction(ProcessingMode mode)
        {
            return "V = N - k, где N - общее количество элементов, k - количество обработанных элементов.";
        }

        /// <summary>
        /// Вычисляет значение варианта и прогресс
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="currentIndex"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public (int value, int progress) CalculateVariantValue(ProcessingMode mode, int currentIndex, int totalCount)
        {
            int value = totalCount - currentIndex;
            int progress = (int)((double)currentIndex / totalCount * 100);
            return (value, progress);
        }

        /// <summary>
        /// Проверяет монотонность варианта
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="previousVariant"></param>
        /// <param name="currentVariant"></param>
        /// <returns></returns>
        public bool CheckMonotonicity(ProcessingMode mode, int previousVariant, int currentVariant)
        {
            return currentVariant < previousVariant;
        }

        /// <summary>
        /// Получает формулу слабейшего предусловия
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public (string formula, string description) GetWpFormula(ProcessingMode mode)
        {
            if (mode.Name == "Количество аниме с рейтингом > N")
            {
                return ("WP(S, Post) = (currentIndex < collection.Count) => Invariant(currentIndex + 1, ...)", "Weakest precondition for count");
            }
            else if (mode.Name == "Поиск аниме с максимальным рейтингом")
            {
                return ("WP(S, Post) = (currentIndex < collection.Count) => Invariant(currentIndex + 1, ...)", "Weakest precondition for max rating");
            }
            return ("", "");
        }

        /// <summary>
        /// Получает постусловие
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string GetPostCondition(ProcessingMode mode)
        {
            if (mode.Name == "Количество аниме с рейтингом > N")
            {
                return "finalState = |{i | 0 <= i < collection.Count and collection[i].Rating > N}|";
            }
            else if (mode.Name == "Поиск аниме с максимальным рейтингом")
            {
                return "finalState = argmax(collection)";
            }
            return "";
        }

        /// <summary>
        /// Проверяет постусловие
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="collection"></param>
        /// <param name="finalState"></param>
        /// <returns></returns>
        public bool CheckPostCondition(ProcessingMode mode, List<WorkItem> collection, object finalState)
        {
            if (mode.Name == "Количество аниме с рейтингом > N")
            {
                int expectedCount = collection.Count(a => a.Rating > 7); // Assuming N is 7
                return (int)finalState == expectedCount;
            }
            else if (mode.Name == "Поиск аниме с максимальным рейтингом")
            {
                var maxRated = collection.OrderByDescending(a => a.Rating).FirstOrDefault();
                return finalState == maxRated;
            }
            return false;
        }

        /// <summary>
        /// Получает условия верификации
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public List<string> GetVerificationConditions(ProcessingMode mode)
        {
            return new List<string>
            {
                "1. Инвариант выполняется перед началом цикла.",
                "2. Инвариант сохраняется на каждой итерации.",
                "3. Цикл завершается.",
                "4. Постусловие выполняется после завершения цикла."
            };
        }
    }
}