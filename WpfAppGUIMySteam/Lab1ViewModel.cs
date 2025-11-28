using ClassLibraryMySteam.Models;
using ClassLibraryMySteam.ViewModels; // Здесь лежит DBService
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfAppGUIMySteam
{
    public class Lab1ViewModel : INotifyPropertyChanged
    {
        // Класс для отображения данных в UI
        public class WorkItemDisplay
        {
            public WorkItem Work { get; set; }
            public string TagNames { get; set; } = string.Empty;

            // Можно добавить любые другие свойства для отображения
            public string DisplayYear => Work.Year?.ToString() ?? "Не указан";
            public string DisplayRating => Work.Rating?.ToString("0.0") ?? "Нет оценки";
        }

        private readonly DBService _dbService;

        // Меняем коллекции на отображаемые объекты
        public ObservableCollection<WorkItemDisplay> AllAnime { get; set; } = new();
        public ObservableCollection<WorkItemDisplay> SearchResults { get; set; } = new();

        // Список жанров для ComboBox
        public List<string> Tags { get; } = new List<string>
        {
            "драма", "комедия", "повседневность", "приключения", "романтика", "сверхъестественное",
            "спорт", "тайна", "триллер", "фантастика", "фэнтези", "экшен"
        };

        public List<string> Types { get; } = new List<string>
        {
            "аниме", "манга"
        };

        // Поля для добавления
        private string _newAnimeTitle;
        public string NewAnimeTitle
        {
            get => _newAnimeTitle;
            set { _newAnimeTitle = value; OnPropertyChanged(); }
        }

        // Для типа (аниме/манга)
        private string _selectedType;
        public string SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        private string _selectedGenre;
        public string SelectedGenre
        {
            get => _selectedGenre;
            set { _selectedGenre = value; OnPropertyChanged(); }
        }

        private int? _newAnimeYear;
        public int? NewAnimeYear
        {
            get => _newAnimeYear;
            set { _newAnimeYear = value; OnPropertyChanged(); }
        }

        private double? _newAnimeRating;
        public double? NewAnimeRating
        {
            get => _newAnimeRating;
            set { _newAnimeRating = value; OnPropertyChanged(); }
        }

        private int _numberOfEpisodes;
        public int NumberOfEpisodes
        {
            get => _numberOfEpisodes;
            set { _numberOfEpisodes = value; OnPropertyChanged(); }
        }

        // Поля для поиска и статуса
        private string _searchTitle;
        public string Title // Привязано к TextBox поиска
        {
            get => _searchTitle;
            set { _searchTitle = value; OnPropertyChanged(); }
        }

        private int _totalAnimeCount;
        public int TotalAnimeCount
        {
            get => _totalAnimeCount;
            set { _totalAnimeCount = value; OnPropertyChanged(); }
        }

        private string _collectionStatus;
        public string CollectionStatus
        {
            get => _collectionStatus;
            set { _collectionStatus = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand AddAnimeCommand { get; }
        public ICommand SearchAnimeCommand { get; }
        public ICommand BackCommand { get; }

        public Lab1ViewModel()
        {
            _dbService = new DBService();

            // Инициализация команд
            AddAnimeCommand = new RelayCommand(async () => await AddAnime());
            SearchAnimeCommand = new RelayCommand(async () => await SearchAnime());
            BackCommand = new RelayCommand(BackToMain);

            // Инициализация контракта
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
                var works = await _dbService.GetAllWorksAsync();

                AllAnime.Clear();
                foreach (var work in works)
                {
                    // получения тегов
                    var tags = await _dbService.GetTagsByWorkIdAsync(work.WorkId);
                    var tagNames = string.Join(", ", tags.Select(t => t.Name));

                    // Создаем объект для отображения
                    var displayItem = new WorkItemDisplay
                    {
                        Work = work,
                        TagNames = tagNames
                    };

                    AllAnime.Add(displayItem);
                }

                TotalAnimeCount = AllAnime.Count;
                CollectionStatus = $"Всего записей: {TotalAnimeCount}";
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

        private async Task AddAnime()
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(NewAnimeTitle) ||
                string.IsNullOrWhiteSpace(SelectedType) ||
                string.IsNullOrWhiteSpace(SelectedGenre))
            {
                MessageBox.Show("Заполните название, выберите тип и жанр!");
                return;
            }

            try
            {
                var newWork = new WorkItem(
                    WorkId: 0,
                    Title: NewAnimeTitle,
                    TypeName: SelectedType, // Тип: аниме или манга
                    Year: NewAnimeYear,
                    Rating: NewAnimeRating,
                    CoverPath: null,
                    Series: NumberOfEpisodes
                );

                // Добавляем произведение
                await _dbService.AddWorkAsync(newWork);

                // Получаем ID добавленного произведения для привязки тега
                var workId = await _dbService.GetWorkByTitleAsync(NewAnimeTitle.Trim().ToLower());

                if (workId.HasValue)
                {
                    // Добавляем жанр как тег к произведению
                    await _dbService.AddTagAsync(NewAnimeTitle, SelectedGenre);
                }

                MessageBox.Show("Произведение успешно добавлено!");

                // Очистка полей
                NewAnimeTitle = string.Empty;
                SelectedType = null;
                SelectedGenre = null;
                NewAnimeYear = null;
                NewAnimeRating = null;
                NumberOfEpisodes = 0;

                // Обновление таблицы
                LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}");
            }
        }

        private async Task SearchAnime()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                LoadDataAsync();
                return;
            }

            try
            {
                string filter = $"WHERE w.Title LIKE '%{Title}%'";
                var results = await _dbService.GetWorksByFilterAsync(filter);

                SearchResults.Clear();
                foreach (var item in results)
                {
                    var tags = await _dbService.GetTagsByWorkIdAsync(item.WorkId);
                    var tagNames = string.Join(", ", tags.Select(t => t.Name));

                    SearchResults.Add(new WorkItemDisplay
                    {
                        Work = item,
                        TagNames = tagNames
                    });
                }

                if (SearchResults.Count == 0)
                    MessageBox.Show("Ничего не найдено.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        private void BackToMain()
        {
            // Открываем главное окно
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Закрываем текущее
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Lab1Window)
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
        // Контрактные свойства
        public List<string> ContractOperations { get; } = new List<string>
{
    "Добавление аниме",
    "Поиск по названию",
    "Подсчет статистики",
    "Валидация данных"
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

        private string _effects;
        public string Effects
        {
            get => _effects;
            set { _effects = value; OnPropertyChanged(); }
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

        // Метод для обновления деталей контракта
        private void UpdateContractDetails()
        {
            switch (SelectedContractOperation)
            {
                case "Добавление аниме":
                    ContractTitle = "Операция: Добавление произведения в библиотеку";
                    PreCondition = "Название ≠ null ∧ Тип ∈ {аниме, манга} ∧ Жанр ∈ допустимых ∧ Рейтинг ∈ [0,10] ∧ Год ∈ [1900,2024]";
                    PostCondition = "Произведение ∈ коллекции ∧ Коллекция.Count = Коллекция.Count' + 1 ∧ Все данные сохранены";
                    Effects = "Увеличение размера коллекции; Обновление статистики; Сохранение в БД";
                    Invariant = "∀ work ∈ Коллекция: work.Year ≤ 2024 ∧ work.Rating ∈ [0,10]";
                    Variant = "Размер коллекции (монотонно возрастает)";
                    ValidExample = "Название='Attack on Titan', Тип='аниме', Жанр='экшен', Год=2013, Рейтинг=8.5";
                    InvalidExample = "Название='', Тип='фильм', Год=3000, Рейтинг=15";
                    break;

                case "Поиск по названию":
                    ContractTitle = "Операция: Поиск произведений по названию";
                    PreCondition = "Запрос ≠ null ∧ Запрос.Length > 0";
                    PostCondition = "Результаты = {work ∈ Коллекция | work.Title.Contains(Запрос)} ∧ Результаты ⊆ Коллекция";
                    Effects = "Фильтрация коллекции; Отображение результатов";
                    Invariant = "Результаты ⊆ Коллекция ∧ ∀ work ∈ Результаты: work.Title.Contains(Запрос)";
                    Variant = "Количество необработанных элементов коллекции (монотонно убывает)";
                    ValidExample = "Запрос='Naruto' → Находит 'Naruto', 'Naruto Shippuden'";
                    InvalidExample = "Запрос='' → Пустой результат";
                    break;

                case "Подсчет статистики":
                    ContractTitle = "Операция: Подсчет общей статистики";
                    PreCondition = "Коллекция ≠ null";
                    PostCondition = "TotalCount = |Коллекция| ∧ Статистика вычислена корректно";
                    Effects = "Агрегация данных; Обновление UI статистики";
                    Invariant = "TotalCount ≥ 0 ∧ TotalCount = |Коллекция|";
                    Variant = "Количество обработанных элементов (монотонно возрастает)";
                    ValidExample = "Коллекция из 50 произведений → TotalCount=50";
                    InvalidExample = "Коллекция=null → Ошибка вычисления";
                    break;

                case "Валидация данных":
                    ContractTitle = "Операция: Валидация введенных данных";
                    PreCondition = "Данные для проверки ≠ null";
                    PostCondition = "isValid = (Год ∈ [1900,2024] ∧ Рейтинг ∈ [0,10] ∧ Название ≠ '')";
                    Effects = "Проверка корректности; Установка флагов валидности";
                    Invariant = "Валидные данные сохраняют целостность коллекции";
                    Variant = "Количество проверенных полей (монотонно возрастает)";
                    ValidExample = "Год=2020, Рейтинг=7.5, Название='Demon Slayer' → isValid=true";
                    InvalidExample = "Год=1800, Рейтинг=-5, Название='' → isValid=false";
                    break;
            }
        }
    }
}