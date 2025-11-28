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
    public class Lab3ViewModel : INotifyPropertyChanged
    {
        // Класс для отображения данных в UI
        public class AnimeDisplay
        {
            public string Title { get; set; } = string.Empty;
            public string Genre { get; set; } = string.Empty;
            public int? Year { get; set; }
            public double? Rating { get; set; }
            public int Episodes { get; set; }
        }

        private readonly DBService _dbService;
        private List<WorkItem> _allWorks = new();

        // Коллекции для UI
        public ObservableCollection<AnimeDisplay> AllAnime { get; set; } = new();
        public ObservableCollection<AnimeDisplay> FilteredAnime { get; set; } = new();

        // Списки для ComboBox
        public List<string> ContentTypes { get; } = new List<string>
        {
            "аниме", "манга"
        };

        public List<string> Genres { get; } = new List<string>
        {
            "драма", "комедия" 
        };

        public List<string> ComparisonOperators { get; } = new List<string>
        {
            "=", ">", ">=", "<", "<=", "!="
        };

        // Свойства для фильтров
        private string _selectedContentType;
        public string SelectedContentType
        {
            get => _selectedContentType;
            set { _selectedContentType = value; OnPropertyChanged(); }
        }

        private string _selectedGenre;
        public string SelectedGenre
        {
            get => _selectedGenre;
            set { _selectedGenre = value; OnPropertyChanged(); }
        }

        private string _yearOperator;
        public string YearOperator
        {
            get => _yearOperator;
            set { _yearOperator = value; OnPropertyChanged(); }
        }

        private string _filterYear;
        public string FilterYear
        {
            get => _filterYear;
            set { _filterYear = value; OnPropertyChanged(); }
        }

        private string _ratingOperator;
        public string RatingOperator
        {
            get => _ratingOperator;
            set { _ratingOperator = value; OnPropertyChanged(); }
        }

        private string _filterRating;
        public string FilterRating
        {
            get => _filterRating;
            set { _filterRating = value; OnPropertyChanged(); }
        }

        private string _episodesOperator;
        public string EpisodesOperator
        {
            get => _episodesOperator;
            set { _episodesOperator = value; OnPropertyChanged(); }
        }

        private string _filterEpisodes;
        public string FilterEpisodes
        {
            get => _filterEpisodes;
            set { _filterEpisodes = value; OnPropertyChanged(); }
        }

        // Свойства для статистики
        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set { _totalCount = value; OnPropertyChanged(); }
        }

        private int _filteredCount;
        public int FilteredCount
        {
            get => _filteredCount;
            set { _filteredCount = value; OnPropertyChanged(); }
        }

        private string _averageRating;
        public string AverageRating
        {
            get => _averageRating;
            set { _averageRating = value; OnPropertyChanged(); }
        }

        private string _averageEpisodes;
        public string AverageEpisodes
        {
            get => _averageEpisodes;
            set { _averageEpisodes = value; OnPropertyChanged(); }
        }

        private string _collectionStatus;
        public string CollectionStatus
        {
            get => _collectionStatus;
            set { _collectionStatus = value; OnPropertyChanged(); }
        }

        private string _activeFiltersText;
        public string ActiveFiltersText
        {
            get => _activeFiltersText;
            set { _activeFiltersText = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand ApplyFilterCommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand BackCommand { get; }

        public Lab3ViewModel()
        {
            _dbService = new DBService();

            // Инициализация команд
            ApplyFilterCommand = new RelayCommand(async () => await ApplyFilter());
            ResetFilterCommand = new RelayCommand(ResetFilter);
            BackCommand = new RelayCommand(BackToMain);

            // Начальная загрузка данных
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                CollectionStatus = "Загрузка данных...";
                _allWorks = await _dbService.GetAllWorksAsync();

                await LoadAnimeWithTags();
                UpdateStatistics();
                ResetFilter(); // Применяем начальный фильтр (показывает все)

                CollectionStatus = $"Загружено записей: {TotalCount}";
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nПодробности SQL-ошибки:\n{ex.InnerException.Message}";
                }
                MessageBox.Show($"Ошибка загрузки БД: {errorMessage}");
                CollectionStatus = "Ошибка подключения";
            }
        }

        private async Task LoadAnimeWithTags()
        {
            AllAnime.Clear();

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
                    Episodes = work.Series
                };

                AllAnime.Add(displayItem);
            }

            TotalCount = AllAnime.Count;
        }

        private async Task ApplyFilter()
        {
            try
            {
                var filter = new WorkFilter();

                // Тип контента
                if (!string.IsNullOrWhiteSpace(SelectedContentType))
                {
                    filter.TypeName = SelectedContentType;
                }

                // Жанр (теги)
                if (!string.IsNullOrWhiteSpace(SelectedGenre))
                {
                    filter.Tags = new List<string> { SelectedGenre };
                    filter.TagMode = TagFilterMode.And;
                }

                // Год
                if (!string.IsNullOrWhiteSpace(FilterYear) && int.TryParse(FilterYear, out int yearValue))
                {
                    filter.SeriesOperator = YearOperator ?? "=";
                    filter.SeriesValue = yearValue;
                }

                // Рейтинг
                if (!string.IsNullOrWhiteSpace(FilterRating) && double.TryParse(FilterRating, out double ratingValue))
                {
                    filter.RatingOperator = RatingOperator ?? ">=";
                    filter.RatingValue = ratingValue;
                }

                // Серии
                if (!string.IsNullOrWhiteSpace(FilterEpisodes) && int.TryParse(FilterEpisodes, out int episodesValue))
                {
                    filter.SeriesOperator = EpisodesOperator ?? "=";
                    filter.SeriesValue = episodesValue;
                }

                // Применяем фильтр
                var filteredWorks = await _dbService.GetFilteredWorksAsync(filter);

                // Обновляем отфильтрованную коллекцию
                FilteredAnime.Clear();
                foreach (var work in filteredWorks)
                {
                    var tags = await _dbService.GetTagsByWorkIdAsync(work.WorkId);
                    var tagNames = string.Join(", ", tags.Select(t => t.Name));

                    FilteredAnime.Add(new AnimeDisplay
                    {
                        Title = work.Title,
                        Genre = tagNames,
                        Year = work.Year,
                        Rating = work.Rating,
                        Episodes = work.Series
                    });
                }

                // Обновляем статистику
                UpdateFilteredStatistics();
                UpdateActiveFiltersText();

                FilteredCount = FilteredAnime.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}");
            }
        }

        private void ResetFilter()
        {
            // Сбрасываем значения фильтров
            SelectedContentType = null;
            SelectedGenre = null;
            YearOperator = null;
            FilterYear = null;
            RatingOperator = null;
            FilterRating = null;
            EpisodesOperator = null;
            FilterEpisodes = null;

            // Показываем все аниме
            FilteredAnime.Clear();
            foreach (var item in AllAnime)
            {
                FilteredAnime.Add(item);
            }

            FilteredCount = FilteredAnime.Count;
            UpdateFilteredStatistics();
            UpdateActiveFiltersText();
        }

        private void UpdateStatistics()
        {
            if (AllAnime.Count == 0)
            {
                AverageRating = "0.0";
                AverageEpisodes = "0";
                return;
            }

            var validRatings = AllAnime.Where(a => a.Rating.HasValue).Select(a => a.Rating.Value);
            var averageRating = validRatings.Any() ? validRatings.Average() : 0;

            var averageEpisodes = AllAnime.Average(a => a.Episodes);

            AverageRating = averageRating.ToString("0.0");
            AverageEpisodes = averageEpisodes.ToString("0");
        }

        private void UpdateFilteredStatistics()
        {
            if (FilteredAnime.Count == 0)
            {
                AverageRating = "0.0";
                AverageEpisodes = "0";
                return;
            }

            var validRatings = FilteredAnime.Where(a => a.Rating.HasValue).Select(a => a.Rating.Value);
            var averageRating = validRatings.Any() ? validRatings.Average() : 0;

            var averageEpisodes = FilteredAnime.Average(a => a.Episodes);

            AverageRating = averageRating.ToString("0.0");
            AverageEpisodes = averageEpisodes.ToString("0");
        }

        private void UpdateActiveFiltersText()
        {
            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(SelectedContentType))
                filters.Add($"Тип: {SelectedContentType}");

            if (!string.IsNullOrWhiteSpace(SelectedGenre))
                filters.Add($"Жанр: {SelectedGenre}");

            if (!string.IsNullOrWhiteSpace(FilterYear))
                filters.Add($"Год {YearOperator} {FilterYear}");

            if (!string.IsNullOrWhiteSpace(FilterRating))
                filters.Add($"Рейтинг {RatingOperator} {FilterRating}");

            if (!string.IsNullOrWhiteSpace(FilterEpisodes))
                filters.Add($"Серии {EpisodesOperator} {FilterEpisodes}");

            ActiveFiltersText = filters.Count > 0
                ? string.Join("\n", filters)
                : "Фильтры не применены";
        }

        private void BackToMain()
        {
            // Открываем главное окно
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Закрываем текущее окно
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Lab3Window)
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