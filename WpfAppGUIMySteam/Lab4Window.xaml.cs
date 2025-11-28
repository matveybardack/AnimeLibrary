using System.Windows;
using System.Windows.Input;

namespace WpfAppGUIMySteam
{
    public partial class Lab4Window : Window
    {
        public Lab4Window()
        {
            InitializeComponent();
            this.DataContext = new Lab4ViewModel();
        }
    }

    //public class Lab4ViewModel
    //{
    //    public ICommand BackCommand { get; }

    //    public Lab4ViewModel()
    //    {
    //        BackCommand = new RelayCommand(BackToMain);
    //    }

    //    private void BackToMain()
    //    {
    //        var mainWindow = new MainWindow();
    //        mainWindow.Show();

    //        // Закрываем текущее окно
    //        foreach (Window window in Application.Current.Windows)
    //        {
    //            if (window is Lab4Window)
    //            {
    //                window.Close();
    //                break;
    //            }
    //        }
    //    }
    //}
}