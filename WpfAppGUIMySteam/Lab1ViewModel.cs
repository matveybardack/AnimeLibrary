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
            "драма", "комедия"
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
    }
}