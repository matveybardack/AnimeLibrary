using ClassLibraryMySteam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.ViewModels
{
    public static class LogicService
    {
        /// <summary>
        /// Формирует расписание просмотра серий для выбранного произведения.
        /// </summary>
        /// <param name="work">Произведение</param>
        /// <param name="episodesPerDay">Количество серий в день</param>
        /// <param name="startDay">Начальный день недели (опционально). Если null, берется текущий день.</param>
        /// <returns>Словарь: ключ - день недели, значение - список серий для просмотра в этот день</returns>
        public static Dictionary<DayOfWeek, List<int>> GenerateSchedule(
            WorkItem work,
            int episodesPerDay,
            DayOfWeek? startDay = null)
        {
            if (work.Series <= 0)
                throw new ArgumentException("Количество серий должно быть больше 0", nameof(work));

            if (episodesPerDay <= 0)
                throw new ArgumentException("Количество серий в день должно быть больше 0", nameof(episodesPerDay));

            DayOfWeek currentDay = startDay ?? DateTime.Now.DayOfWeek;

            var schedule = new Dictionary<DayOfWeek, List<int>>();

            int totalEpisodes = work.Series;
            int episodeNumber = 1;

            while (episodeNumber <= totalEpisodes)
            {
                if (!schedule.ContainsKey(currentDay))
                    schedule[currentDay] = new List<int>();

                for (int i = 0; i < episodesPerDay && episodeNumber <= totalEpisodes; i++)
                {
                    schedule[currentDay].Add(episodeNumber);
                    episodeNumber++;
                }

                // Переходим к следующему дню недели
                currentDay = (DayOfWeek)(((int)currentDay + 1) % 7);
            }

            return schedule;
        }
    }
}
