using ClassLibraryMySteam.Models;
using ClassLibraryMySteam.Config;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.Services
{
    public class Connection
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        /// <summary>
        /// Получение всех произведений с типами
        /// </summary>
        /// <returns>Список из WorkItem</returns>
        public async Task<List<WorkItem>> GetAllWorksAsync()
        {
            string query = AppConfig.SqlGetAllWorks;

            var list = new List<WorkItem>();

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new WorkItem(
                    WorkId: reader.GetInt32(0),
                    Title: reader.GetString(1),
                    TypeName: reader.GetString(2),
                    Year: reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Rating: reader.IsDBNull(4) ? null : reader.GetDouble(4),
                    CoverPath: reader.IsDBNull(5) ? null : reader.GetString(5)
                ));
            }

            return list;
        }

        /// <summary>
        /// Получение всех тегов для указанного произведения
        /// </summary>
        /// <param name="workId">id произведения в бд</param>
        /// <returns>Список из TagItem</returns>
        public async Task<List<TagItem>> GetTagsByWorkIdAsync(int workId)
        {
            string query = AppConfig.SqlGetTagsByWorkId;

            var list = new List<TagItem>();

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@WorkId", workId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new TagItem(
                    TagId: reader.GetInt32(0),
                    Name: reader.GetString(1)
                ));
            }

            return list;
        }

        /// <summary>
        /// Получение id тега по имени
        /// </summary>
        /// <param name="tagName">имя тега</param>
        /// <returns>Int если имя найдено, иначе null</returns>
        public async Task<int?> GetTagByNameAsync(string tagName)
        {
            string query = AppConfig.SqlGetTagByName;

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", tagName);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }

            return null;
        }

        /// <summary>
        /// Получение id произведения по названию
        /// </summary>
        /// <param name="title">название произведения</param>
        /// <returns></returns>
        public async Task<int?> GetWorkByTitleAsync(string title)
        {
            string query = AppConfig.SqlGetWorkByTitle;

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@Title", title);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }

            return null;
        }

        /// <summary>
        /// Получение id типа по имени
        /// </summary>
        /// <param name="typeName">имя типа</param>
        /// <returns>Int, если тип найден, иначе false</returns>
        public async Task<int?> GetTypeByNameAsync(string typeName)
        {
            string query = AppConfig.SqlGetTypeByName;

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", typeName);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }

            return null;
        }

        /// <summary>
        /// Добавление нового тега
        /// </summary>
        /// <param name="tagName">имя тега</param>
        /// <returns></returns>
        public async Task AddNewTagAsync(string tagName)
        {
            string query = AppConfig.AddNewTag;

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", tagName);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление нового произведения
        /// </summary>
        /// <param name="title">название произведения</param>
        /// <param name="typeId">id типа произведения</param>
        /// <param name="year">год выпуска</param>
        /// <param name="rating">рейтинг в 10 бальной шкале</param>
        /// <param name="cover">защищенный двойными кавычками путь к иконке</param>
        /// <returns></returns>
        public async Task AddWorkAsync(string title, int typeId, int? year, double? rating, string? cover)
        {
            string query = AppConfig.AddWork;

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);

            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@TypeId", typeId);
            cmd.Parameters.AddWithValue("@Year", year ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Rating", rating ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CoverPath", cover ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление нового типа произведения
        /// </summary>
        /// <param name="typeName">имя типа</param>
        /// <returns></returns>
        public async Task AddTypeAsync(string typeName)
        {
            string query = AppConfig.AddType;

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", typeName);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Добавление нового тега к произведению
        /// </summary>
        /// <param name="WorkId">id произведения</param>
        /// <param name="TagId">id тэга</param>
        /// <returns></returns>
        public async Task AddTagAsync(int WorkId, int TagId)
        {
            string query = AppConfig.AddTag;

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);

            cmd.Parameters.AddWithValue("@WorkId", WorkId);
            cmd.Parameters.AddWithValue("@TagId", TagId);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Поиск произведений по условию фильтрации
        /// </summary>
        /// <param name="filterCondition">пользовательский фильтр</param>
        /// <returns>Список произведений (WorkItem)</returns>
        public async Task<List<WorkItem>> GetWorksByFilterAsync(string filterCondition)
        {
            string query = AppConfig.SqlGetWorksByFilter + " " + filterCondition;

            var list = new List<WorkItem>();

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new WorkItem(
                    WorkId: reader.GetInt32(0),
                    Title: reader.GetString(1),
                    TypeName: reader.GetString(2),
                    Year: reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Rating: reader.IsDBNull(4) ? null : reader.GetDouble(4),
                    CoverPath: reader.IsDBNull(5) ? null : reader.GetString(5)
                ));
            }

            return list;
        }

        /// <summary>
        /// Получение ограниченного количества произведений по рейтингу
        /// </summary>
        /// <param name="rating">порог рейтинга</param>
        /// <param name="limit">число произведений</param>
        /// <returns></returns>
        public async Task<List<WorkItem>> GetWorksByRatingAndLimitAsync(double rating, int limit)
        {
            string query = AppConfig.SqlGetWorksByRatingAndLimit;

            var list = new List<WorkItem>();

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@Rating", rating);
            cmd.Parameters.AddWithValue("@Limit", limit);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new WorkItem(
                    WorkId: reader.GetInt32(0),
                    Title: reader.GetString(1),
                    TypeName: reader.GetString(2),
                    Year: reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Rating: reader.IsDBNull(4) ? null : reader.GetDouble(4),
                    CoverPath: reader.IsDBNull(5) ? null : reader.GetString(5)
                ));
            }

            return list;
        }
    }
}
