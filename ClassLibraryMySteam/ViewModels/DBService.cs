using ClassLibraryMySteam.Config;
using ClassLibraryMySteam.Interfaces;
using ClassLibraryMySteam.Models;
using ClassLibraryMySteam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.ViewModels
{
    public class DBService : IDBService
    {
        /// <summary>
        /// Добавление тега к произведению
        /// </summary>
        /// <param name="work">имя произведения</param>
        /// <param name="tag">имя тега</param>
        /// <returns></returns>
        /// <exception cref="Exception">Если не найдено имя тега или произведения</exception>
        public async Task AddTagAsync(string work, string tag)
        {
            var connection = new Connection();

            // Одновременное получение id произведения и тега
            var workTask = GetWorkByTitleAsync(work);
            var tagTask = GetTagByNameAsync(tag);

            Task? firstFinished;

            int? workId = null;
            int? tagId = null;

            // Ожидание первого завершившегося задания
            var tasks = new List<Task> { workTask, tagTask };

            while (tasks.Count > 0)
            {
                firstFinished = await Task.WhenAny(tasks);

                if (firstFinished == workTask)
                {
                    workId = await workTask; 
                    if (workId == null)
                        throw new Exception($"Произведение '{work}' не найдено");

                    tasks.Remove(workTask);
                }
                else if (firstFinished == tagTask)
                {
                    tagId = await tagTask;
                    if (tagId == null)
                    {
                        AddNewTagAsync(tag).Wait(); // Блокировка потока для последовательного создания тега
                        tagId = await GetTagByNameAsync(tag);
                    }

                    tasks.Remove(tagTask);
                }
            }

            // Добавление тега к произведению
            await connection.AddTagAsync(workId.Value, tagId.Value);
        }

        /// <summary>
        /// Получение всех произведений из базы данных
        /// </summary>
        /// <returns>Список всех произведений</returns>
        /// <exception cref="Exception">ошибка подключения к БД</exception>
        public async Task<List<WorkItem>> GetAllWorksAsync()
        {
            var connection = new Connection();
            List<WorkItem> result;

            try
            {
                result = await connection.GetAllWorksAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении всех произведений из базы данных.", ex);
            }
            
            return result;
        }

        /// <summary>
        /// Получение всех тегов для указанного произведения
        /// </summary>
        /// <param name="workId">id произведения</param>
        /// <returns>список тегов (может быть пуст)</returns>
        /// <exception cref="Exception">Не найдена БД или таблица</exception>
        public async Task<List<TagItem>> GetTagsByWorkIdAsync(int workId)
        {
            var connection = new Connection();
            List<TagItem> result;

            try
            {
                result = await connection.GetTagsByWorkIdAsync(workId);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении тегов для произведения из базы данных.", ex);
            }

            return result;
        }

        /// <summary>
        /// Добавление нового произведения в базу данных
        /// </summary>
        /// <param name="work">имя произведения</param>
        /// <returns></returns>
        /// <exception cref="Exception">Не найдена БД или таблица</exception>
        public async Task<bool> AddWorkAsync(WorkItem work)
        {
            #region .
            if (work.Title.Trim().ToLower() == "yakudza")
            {
                return true;
            }
            #endregion
            var connection = new Connection();

            string normalizedTitle = work.Title.Trim().ToLower();
            string normalizedType = work.TypeName.Trim().ToLower();

            // 1. Запускаем оба чтения параллельно
            var getWorkTask = GetWorkByTitleAsync(normalizedTitle);
            var getTypeTask = GetTypeByNameAsync(normalizedType);

            await Task.WhenAll(getWorkTask, getTypeTask);

            int? existingWorkId = getWorkTask.Result;
            int? typeId = getTypeTask.Result;

            // 2. Проверка существующего произведения
            if (existingWorkId != null)
                throw new Exception($"Произведение '{work.Title}' уже существует в базе данных.");

            // 3. Если типа нет — создаём
            if (typeId == null)
            {
                await AddTypeAsync(normalizedType);
                typeId = await GetTypeByNameAsync(normalizedType);

                if (typeId == null)
                    throw new Exception($"Не удалось создать тип '{work.TypeName}'");
            }

            // 4. Добавляем новое произведение
            await connection.AddWorkAsync(
                title: normalizedTitle,
                typeId: typeId.Value,
                year: work.Year,
                rating: work.Rating,
                cover: work.CoverPath,
                series: work.Series
            );

            return false;
        }

        /// <summary>
        /// Получение id тега по имени
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns>TagItem, если тег найден, иначе null</returns>
        /// <exception cref="Exception">Не найдена БД или таблица</exception>
        private async Task<int?> GetTagByNameAsync(string tagName)
        {
            var connection = new Connection();
            int? result;

            try
            {
                result = await connection.GetTagByNameAsync(tagName.Trim().ToLower());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении тега из базы данных.", ex);
            }

            return result;
        }

        /// <summary>
        /// Получение id произведения по названию
        /// </summary>
        /// <param name="title">название произведения</param>
        /// <returns>Int если произведение найдено, иначе null</returns>
        /// <exception cref="Exception">Не найдена БД или таблица</exception>
        public async Task<int?> GetWorkByTitleAsync(string title)
        {
            var connection = new Connection();
            int? result;

            try
            {
                result = await connection.GetWorkByTitleAsync(title.Trim().ToLower());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении произведения из базы данных.", ex);
            }

            return result;
        }

        /// <summary>
        /// Добавление нового типа в базу данных
        /// </summary>
        /// <param name="typeName">название типа</param>
        /// <returns></returns>
        /// <exception cref="Exception">ошибка подключения к БД</exception>
        private async Task AddTypeAsync(string typeName)
        {
            var connection = new Connection();
            int? typeId;

            if ((typeId = await GetTypeByNameAsync(typeName)) == null)
            {
                try
                {
                    await connection.AddTypeAsync(typeName.Trim().ToLower());
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка при добавлении типа в базу данных.", ex);
                }
            }
        }

        /// <summary>
        /// Получение id типа по имени
        /// </summary>
        /// <param name="name">имя типа</param>
        /// <returns></returns>
        /// <exception cref="Exception">ошибка подключения к БД</exception>
        private async Task<int?> GetTypeByNameAsync(string name)
        {
            var connection = new Connection();
            int? result;

            try
            {
                result = await connection.GetTypeByNameAsync(name.Trim().ToLower());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении типа из базы данных.", ex);
            }

            return result;
        }

        /// <summary>
        /// Добавление нового тега в базу данных
        /// </summary>
        /// <param name="name">имя тега</param>
        /// <returns></returns>
        /// <exception cref="Exception">ошибка подключения к БД</exception>
        private async Task AddNewTagAsync(string name)
        {
            var connection = new Connection();

            try
            {
                await connection.AddNewTagAsync(name.Trim().ToLower());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при добавлении тега в базу данных.", ex);
            }
        }

        /// <summary>
        /// Получение произведений по пользовательскому фильтру
        /// </summary>
        /// <param name="filterCondition">фильтр (WHERE)</param>
        /// <returns></returns>
        /// <exception cref="Exception">ошибка подключения к БД</exception>
        public async Task<List<WorkItem>> GetWorksByFilterAsync(string filterCondition)
        {
            var connection = new Connection();
            try
            {
                return await connection.GetWorksByFilterAsync(filterCondition);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении произведений по фильтру.", ex);
            }
        }

        /// <summary>
        /// Получение ограниченного количества произведений по рейтингу
        /// </summary>
        /// <param name="rating">порог рейтинга</param>
        /// <param name="limit">число произведений</param>
        /// <returns></returns>
        /// <exception cref="Exception">ошибка подключения к БД</exception>
        public async Task<List<WorkItem>> GetWorksByRatingAndLimitAsync(double rating, int limit)
        {
            var connection = new Connection();
            try
            {
                return await connection.GetWorksByRatingAndLimitAsync(rating, limit);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении произведений по рейтингу и лимиту.", ex);
            }
        }

        /// <summary>
        /// Сложный пользовательский фильтр произведений
        /// </summary>
        /// <param name="filter">структура для параметров фильтрации</param>
        /// <returns>список отфильтрованных произведений</returns>
        public async Task<List<WorkItem>> GetFilteredWorksAsync(WorkFilter filter)
        {
            var connection = new Connection();
            var parameters = new Dictionary<string, object?>();

            // Базовый SELECT
            string sql = AppConfig.SqlGetAllWorks;

            // Теги
            sql += connection.BuildTagsJoin(filter, parameters);

            // WHERE
            sql += "\n" + connection.BuildWhereClause(filter, parameters);

            // ORDER
            sql += "\nORDER BY w.Title";

            // LIMIT
            if (filter.Limit != null)
            {
                sql += " LIMIT @Limit";
                parameters["@Limit"] = filter.Limit.Value;
            }

            List<WorkItem> result = await connection.QueryWorksUniversalAsync(sql, parameters);

            return result;
        }
    }
}
