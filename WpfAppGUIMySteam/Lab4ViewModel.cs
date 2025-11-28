using ClassLibraryMySteam.Models;
using ClassLibraryMySteam.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfAppGUIMySteam
{
    public class Lab4ViewModel : INotifyPropertyChanged
    {
        // Классы для отображения данных в UI
        public class AnimeDisplay
        {
            public string Title { get; set; } = string.Empty;
            public string Genre { get; set; } = string.Empty;
            public int? Year { get; set; }
            public double? Rating { get; set; }
            public int Episodes { get; set; }
            public int WorkId { get; set; }
        }

        public class ScheduleDay
        {
            public int WeekNumber { get; set; }
            public string DayOfWeek { get; set; } = string.Empty;
            public string Episodes { get; set; } = string.Empty;
            public int EpisodeCount { get; set; }
            public string ViewingTime { get; set; } = string.Empty;
        }

        private readonly DBService _dbService;
        private List<WorkItem> _allWorks = new();

        // Коллекции для UI
        public ObservableCollection<AnimeDisplay> AvailableAnime { get; set; } = new();
        public ObservableCollection<ScheduleDay> Schedule { get; set; } = new();

        // Списки для ComboBox
        public List<string> DaysOfWeek { get; } = new List<string>
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье"
        };

        // Свойства для выбора аниме
        private AnimeDisplay _selectedAnime;
        public AnimeDisplay SelectedAnime
        {
            get => _selectedAnime;
            set { _selectedAnime = value; OnPropertyChanged();}
        }

        // Свойства для настроек расписания
        private string _episodesPerDay = "2";
        public string EpisodesPerDay
        {
            get => _episodesPerDay;
            set { _episodesPerDay = value; OnPropertyChanged(); }
        }

        private string _startDay = "Понедельник";
        public string StartDay
        {
            get => _startDay;
            set { _startDay = value; OnPropertyChanged(); }
        }

        // Свойства для статистики
        private string _totalViewingTime;
        public string TotalViewingTime
        {
            get => _totalViewingTime;
            set { _totalViewingTime = value; OnPropertyChanged(); }
        }

        private string _totalWeeks;
        public string TotalWeeks
        {
            get => _totalWeeks;
            set { _totalWeeks = value; OnPropertyChanged(); }
        }

        private string _totalViewingDays;
        public string TotalViewingDays
        {
            get => _totalViewingDays;
            set { _totalViewingDays = value; OnPropertyChanged(); }
        }

        private string _averagePerDay;
        public string AveragePerDay
        {
            get => _averagePerDay;
            set { _averagePerDay = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand CalculateScheduleCommand { get; }
        public ICommand BackCommand { get; }

        public Lab4ViewModel()
        {
            _dbService = new DBService();

            // Инициализация команд
            CalculateScheduleCommand = new RelayCommand(async () => await CalculateSchedule());
            BackCommand = new RelayCommand(BackToMain);

            // Начальная загрузка данных
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                _allWorks = await _dbService.GetAllWorksAsync();
                await LoadAvailableAnime();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nПодробности SQL-ошибки:\n{ex.InnerException.Message}";
                }
                MessageBox.Show($"Ошибка загрузки БД: {errorMessage}");
            }
        }

        private async Task LoadAvailableAnime()
        {
            AvailableAnime.Clear();

            foreach (var work in _allWorks)
            {
                // Получаем теги для произведения
                var tags = await _dbService.GetTagsByWorkIdAsync(work.WorkId);
                var tagNames = string.Join(", ", tags.Select(t => t.Name));

                // Создаем объект для отображения
                var displayItem = new AnimeDisplay
                {
                    Title = work.Title,
                    Genre = tagNames,
                    Year = work.Year,
                    Rating = work.Rating,
                    Episodes = work.Series,
                    WorkId = work.WorkId
                };

                AvailableAnime.Add(displayItem);
            }
        }

        private async Task CalculateSchedule()
        {
            if (SelectedAnime == null)
            {
                MessageBox.Show("Выберите аниме для планирования!");
                return;
            }

            if (!int.TryParse(EpisodesPerDay, out int episodesPerDay) || episodesPerDay <= 0)
            {
                MessageBox.Show("Введите корректное количество серий в день!");
                return;
            }

            try
            {
                // Получаем полную информацию о произведении
                var work = _allWorks.FirstOrDefault(w => w.WorkId == SelectedAnime.WorkId);
                if (work == null)
                {
                    MessageBox.Show("Ошибка: произведение не найдено!");
                    return;
                }

                // Преобразуем день недели
                DayOfWeek startDay = ConvertToDayOfWeek(StartDay);

                // Генерируем расписание
                var scheduleDict = LogicService.GenerateSchedule(work, episodesPerDay, startDay);

                // Обновляем UI
                UpdateScheduleDisplay(scheduleDict);
                UpdateStatistics(scheduleDict, work.Series, episodesPerDay);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете расписания: {ex.Message}");
            }
        }

        private DayOfWeek ConvertToDayOfWeek(string dayName)
        {
            return dayName switch
            {
                "Понедельник" => DayOfWeek.Monday,
                "Вторник" => DayOfWeek.Tuesday,
                "Среда" => DayOfWeek.Wednesday,
                "Четверг" => DayOfWeek.Thursday,
                "Пятница" => DayOfWeek.Friday,
                "Суббота" => DayOfWeek.Saturday,
                "Воскресенье" => DayOfWeek.Sunday,
                _ => DayOfWeek.Monday
            };
        }

        private string ConvertDayOfWeekToRussian(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                DayOfWeek.Sunday => "Воскресенье",
                _ => "Неизвестно"
            };
        }

        private void UpdateScheduleDisplay(Dictionary<DayOfWeek, List<int>> scheduleDict)
        {
            Schedule.Clear();

            // Получаем выбранный начальный день
            DayOfWeek startDay = ConvertToDayOfWeek(StartDay);

            // Создаем порядок дней недели, начиная с выбранного дня
            var daysInOrder = CreateDaysOrder(startDay);

            int currentWeek = 1;
            int currentEpisode = 1;
            int totalEpisodes = SelectedAnime.Episodes;
            int episodesPerDay = int.Parse(EpisodesPerDay);

            // Продолжаем пока не распределим все эпизоды
            while (currentEpisode <= totalEpisodes)
            {
                // Для каждой недели проходим по всем дням в правильном порядке
                foreach (var day in daysInOrder)
                {
                    if (currentEpisode > totalEpisodes) break;

                    // Получаем эпизоды для этого дня
                    var dayEpisodes = new List<int>();

                    for (int i = 0; i < episodesPerDay && currentEpisode <= totalEpisodes; i++)
                    {
                        dayEpisodes.Add(currentEpisode);
                        currentEpisode++;
                    }

                    // Добавляем день в расписание (даже если эпизодов нет, чтобы показать пустой день)
                    var scheduleDay = new ScheduleDay
                    {
                        WeekNumber = currentWeek,
                        DayOfWeek = ConvertDayOfWeekToRussian(day),
                        Episodes = dayEpisodes.Count > 0 ? string.Join(", ", dayEpisodes) : "-",
                        EpisodeCount = dayEpisodes.Count,
                        ViewingTime = dayEpisodes.Count > 0 ? CalculateViewingTime(dayEpisodes.Count) : "-"
                    };

                    Schedule.Add(scheduleDay);
                }

                currentWeek++;
            }
        }

        private List<DayOfWeek> CreateDaysOrder(DayOfWeek startDay)
        {
            var days = new List<DayOfWeek>();
            int startIndex = (int)startDay;

            // Добавляем дни от startDay до конца недели
            for (int i = startIndex; i < 7; i++)
            {
                days.Add((DayOfWeek)i);
            }

            // Добавляем дни от начала недели до startDay-1
            for (int i = 0; i < startIndex; i++)
            {
                days.Add((DayOfWeek)i);
            }

            return days;
        }

        private string CalculateViewingTime(int episodeCount)
        {
            // Предполагаем, что одна серия длится 24 минуты (стандарт для аниме)
            int totalMinutes = episodeCount * 24;
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return hours > 0 ? $"{hours}ч {minutes}м" : $"{minutes}м";
        }

        private void UpdateStatistics(Dictionary<DayOfWeek, List<int>> scheduleDict, int totalEpisodes, int episodesPerDay)
        {
            // Общее время просмотра
            int totalMinutes = totalEpisodes * 24;
            int totalHours = totalMinutes / 60;
            int totalMinutesRemainder = totalMinutes % 60;
            TotalViewingTime = totalHours > 0 ? $"{totalHours}ч {totalMinutesRemainder}м" : $"{totalMinutes}м";

            // Количество дней просмотра
            int viewingDays = (int)Math.Ceiling(totalEpisodes / (double)episodesPerDay);
            TotalViewingDays = viewingDays.ToString();

            // Количество недель
            int weeks = (int)Math.Ceiling(viewingDays / 7.0);
            TotalWeeks = weeks.ToString();

            // Среднее в день
            double average = totalEpisodes / (double)viewingDays;
            AveragePerDay = average.ToString("0.0") + " серий";
        }

        private void BackToMain()
        {
            // Открываем главное окно
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Закрываем текущее окно
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Lab4Window)
                {
                    window.Close();
                    break;
                }
            }
        }

        // Реализация INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}