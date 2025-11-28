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
            "драма", "комедия", "повседневность", "приключения", "романтика", "сверхъестественное",
            "спорт", "тайна", "триллер", "фантастика", "фэнтези", "экшен" 
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

            // Инициализация контракта для ЛР3
            SelectedContractOperation = ContractOperations.First();
            UpdateContractDetails();

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
                // Проверка предусловия
                if (AllAnime.Count == 0)
                {
                    MessageBox.Show("Ошибка: коллекция пуста");
                    return;
                }

                InvariantBeforeStep = "✓ Выполнен";

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

                InvariantAfterStep = FilteredAnime.Count <= AllAnime.Count ? "✓ Выполнен" : "✗ Нарушен";
                UpdateInvariantCheck();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}");
                InvariantAfterStep = "✗ Нарушен (ошибка выполнения)";
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

            UpdateInvariantCheck();
            InvariantBeforeStep = "✓ Выполнен";
            InvariantAfterStep = "✓ Выполнен";
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
        // Контрактные свойства для ЛР3
        public List<string> ContractOperations { get; } = new List<string>
        {
            "Фильтрация коллекции",
            "Статистический анализ",
            "Поиск по критериям",
            "Алгоритм подбора"
        };

        private string _selectedContractOperation;
        public string SelectedContractOperation
        {
            get => _selectedContractOperation;
            set
            {
                _selectedContractOperation = value;
                OnPropertyChanged();
                UpdateContractDetails();
            }
        }

        private string _contractTitle;
        public string ContractTitle
        {
            get => _contractTitle;
            set { _contractTitle = value; OnPropertyChanged(); }
        }

        private string _preCondition;
        public string PreCondition
        {
            get => _preCondition;
            set { _preCondition = value; OnPropertyChanged(); }
        }

        private string _postCondition;
        public string PostCondition
        {
            get => _postCondition;
            set { _postCondition = value; OnPropertyChanged(); }
        }

        private string _invariant;
        public string Invariant
        {
            get => _invariant;
            set { _invariant = value; OnPropertyChanged(); }
        }

        private string _variant;
        public string Variant
        {
            get => _variant;
            set { _variant = value; OnPropertyChanged(); }
        }

        private string _exitCondition;
        public string ExitCondition
        {
            get => _exitCondition;
            set { _exitCondition = value; OnPropertyChanged(); }
        }

        private string _validExample;
        public string ValidExample
        {
            get => _validExample;
            set { _validExample = value; OnPropertyChanged(); }
        }

        private string _invalidExample;
        public string InvalidExample
        {
            get => _invalidExample;
            set { _invalidExample = value; OnPropertyChanged(); }
        }

        // Свойства для проверки инварианта
        private string _invariantBeforeStep = "✓ Выполнен";
        public string InvariantBeforeStep
        {
            get => _invariantBeforeStep;
            set { _invariantBeforeStep = value; OnPropertyChanged(); }
        }

        private string _invariantAfterStep = "✓ Выполнен";
        public string InvariantAfterStep
        {
            get => _invariantAfterStep;
            set { _invariantAfterStep = value; OnPropertyChanged(); }
        }

        private string _variantValue = "n - j = 50";
        public string VariantValue
        {
            get => _variantValue;
            set { _variantValue = value; OnPropertyChanged(); }
        }

        // Метод для обновления деталей контракта
        private void UpdateContractDetails()
        {
            switch (SelectedContractOperation)
            {
                case "Фильтрация коллекции":
                    ContractTitle = "Операция: Фильтрация коллекции по критериям";
                    PreCondition = "Коллекция ≠ null ∧ Критерии валидны ∧ Операторы ∈ {=,>,>=,<,<=,!=}";
                    PostCondition = "Filtered ⊆ Коллекция ∧ ∀item ∈ Filtered: item удовлетворяет критериям";
                    Invariant = "Filtered ⊆ Коллекция ∧ |Filtered| ≤ |Коллекция| ∧ Фильтр монотонен";
                    Variant = "t = |Коллекция| - |Filtered| (монотонно убывает при добавлении фильтров)";
                    ExitCondition = "Все элементы коллекции проверены ∧ Filtered = {x ∈ Коллекция | критерии(x)}";
                    ValidExample = "Рейтинг >= 8.0, Жанр='драма' → фильтрует драмы с высоким рейтингом";
                    InvalidExample = "Рейтинг > 10, Год < 1900 → пустой результат (невалидные критерии)";
                    break;

                case "Статистический анализ":
                    ContractTitle = "Операция: Вычисление статистики коллекции";
                    PreCondition = "Коллекция ≠ null ∧ |Коллекция| > 0";
                    PostCondition = "AverageRating = Σrating/|Коллекция| ∧ AverageEpisodes = Σepisodes/|Коллекция|";
                    Invariant = "Σrating ≥ 0 ∧ Σepisodes ≥ 0 ∧ 0 ≤ AverageRating ≤ 10";
                    Variant = "t = количество необработанных элементов (монотонно убывает)";
                    ExitCondition = "Все элементы обработаны ∧ статистика вычислена корректно";
                    ValidExample = "Коллекция из 100 аниме → вычисляет средние значения";
                    InvalidExample = "Коллекция=null → ошибка вычисления";
                    break;

                case "Поиск по критериям":
                    ContractTitle = "Операция: Поиск произведений по комбинированным критериям";
                    PreCondition = "Коллекция ≠ null ∧ Критерии ≠ ∅ ∧ Операторы корректны";
                    PostCondition = "Результаты = {x ∈ Коллекция | тип(x)=T ∧ жанр(x)=G ∧ год(x) op Y ∧ ...}";
                    Invariant = "Результаты ⊆ Коллекция ∧ критерии применяются последовательно";
                    Variant = "t = количество оставшихся критериев для проверки";
                    ExitCondition = "Все критерии применены ∧ результаты отфильтрованы";
                    ValidExample = "Тип='аниме', Рейтинг>7, Год>=2010 → релевантные аниме";
                    InvalidExample = "Несуществующий жанр, нечисловой рейтинг → пустой результат";
                    break;

                case "Алгоритм подбора":
                    ContractTitle = "Алгоритм: Подбор произведения (цикл обработки)";
                    PreCondition = "Коллекция ≠ null ∧ j=0 ∧ res=∅";
                    PostCondition = "res = {x ∈ Коллекция[0..j-1] | критерии(x)} ∧ j=|Коллекция|";
                    Invariant = "res = {x ∈ Коллекция[0..k] | критерии(x)} ∧ 0≤k≤j";
                    Variant = "t = |Коллекция| - j (монотонно убывает)";
                    ExitCondition = "j ≥ |Коллекция| ∧ обработаны все элементы";
                    ValidExample = "j=0 → j=50, обработана вся коллекция, res содержит подходящие";
                    InvalidExample = "j=-1 (невалидная инициализация) → нарушение инварианта";
                    break;
            }

            // Обновляем значения проверки инварианта
            UpdateInvariantCheck();
        }

        // Метод для обновления проверки инварианта
        private void UpdateInvariantCheck()
        {
            // Симуляция проверки инварианта для демонстрации
            InvariantBeforeStep = "✓ Выполнен";
            InvariantAfterStep = "✓ Выполнен";
            VariantValue = $"n - j = {Math.Max(0, TotalCount - FilteredCount)}";
        }
    }
}