using System.Windows;
using System.Windows.Input;

namespace WpfAppGUIMySteam
{
    public partial class Lab1Window : Window
    {
        public Lab1Window()
        {
            InitializeComponent();
            this.DataContext = new Lab1ViewModel();
        }
    }

    //public class Lab1ViewModel
    //{
    //    public ICommand BackCommand { get; }

    //    public Lab1ViewModel()
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
    //            if (window is Lab1Window)
    //            {
    //                window.Close();
    //                break;
    //            }
    //        }
    //    }
    //}
}