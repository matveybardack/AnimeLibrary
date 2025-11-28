using System.Windows;
using System.Windows.Input;

namespace WpfAppGUIMySteam
{
    public partial class Lab3Window : Window
    {
        public Lab3Window()
        {
            InitializeComponent();
            this.DataContext = new Lab3ViewModel();
        }
    }

    //public class Lab3ViewModel
    //{
    //    public ICommand BackCommand { get; }

    //    public Lab3ViewModel()
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
    //            if (window is Lab3Window)
    //            {
    //                window.Close();
    //                break;
    //            }
    //        }
    //    }
    //}
}