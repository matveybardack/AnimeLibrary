using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.Config
{
    internal static class AppConfig
    {
        #region Получение пути к базе данных
        private static readonly string ExePath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string RootPath = Directory.GetParent(ExePath).Parent.Parent.Parent.Parent.FullName;
        private static readonly string DbPath = Path.Combine(RootPath, "Storage");
        #endregion

        /// <summary>
        /// Строка подключения к базе данных SQLite
        /// </summary>
        internal static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        /// <summary>
        /// Получение всех работ с информацией о типе
        /// </summary>
        internal static readonly string SqlGetAllWorks = @"
            SELECT 
                w.WorksId,
                w.Title,
                t.Name AS TypeName,
                w.Year,
                w.Rating,
                w.CoverPath
            FROM Works w
            JOIN Types t ON w.TypeId = t.TypesId
            ORDER BY w.Title;
        ";

        /// <summary>
        /// Получение всех тегов для указанной работы
        /// </summary>
        internal static readonly string SqlGetTagsByWorkId = @"
            SELECT 
                tg.TagId,
                tg.Name
            FROM Tags tg
            JOIN WorkTags wt ON tg.TagId = wt.TagId
            WHERE wt.WorkId = @WorkId
            ORDER BY tg.Name;
        ";

        /// <summary>
        /// Добавление нового произведения
        /// </summary>
        internal static readonly string AddWork = @"
                INSERT INTO Works (Title, TypeId, Year, Rating, CoverPath)
                VALUES (@Title, @TypeId, @Year, @Rating, @CoverPath);
        ";

        /// <summary>
        /// Добавление нового типа произведения
        /// </summary>
        internal static readonly string AddType = @"
            INSERT INTO Types (Name)
            VALUES (@Name);
        ";

        /// <summary>
        /// Добавление нового тега
        /// </summary>
        internal static readonly string AddTag = @"
            INSERT INTO WorkTags (WorkId, TagId)
            VALUES (@WorkId, @TagId);
        ";

        /// <summary>
        /// Поиск произведения по названию
        /// </summary>
        internal static readonly string SqlGetWorkByTitle = @"
            SELECT 
                w.WorksId,
                w.Title,
                t.Name AS TypeName,
                w.Year,
                w.Rating,
                w.CoverPath
            FROM Works w
            JOIN Types t ON w.TypeId = t.TypesId
            WHERE w.Title = @Title;
        ";

        /// <summary>
        /// Поиск тега по имени
        /// </summary>
        internal static readonly string SqlGetTagByName = @"
            SELECT 
                tg.TagId,
                tg.Name
            FROM Tags tg
            WHERE tg.Name = @Name;
        ";

        /// <summary>
        /// Поиск типа по имени
        /// </summary>
        internal static readonly string SqlGetTypeByName = @"
            SELECT 
                tp.TypesId,
                tp.Name
            FROM Types tp
            WHERE tp.Name = @Name;
        ";

        /// <summary>
        /// Добавление нового тега
        /// </summary>
        internal static readonly string AddNewTag = @"
            INSERT INTO Tags (Name)
            VALUES (@Name); 
        ";
    }
}
