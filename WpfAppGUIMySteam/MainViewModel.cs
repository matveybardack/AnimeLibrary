using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfAppGUIMySteam
{
    public class MainViewModel
    {
        public ICommand OpenLab1Command { get; }
        public ICommand OpenLab2Command { get; }
        public ICommand OpenLab3Command { get; }
        public ICommand OpenLab4Command { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            OpenLab1Command = new RelayCommand(OpenLab1);
            OpenLab2Command = new RelayCommand(OpenLab2);
            OpenLab3Command = new RelayCommand(OpenLab3);
            OpenLab4Command = new RelayCommand(OpenLab4);
            ExitCommand = new RelayCommand(ExitApp);
        }

        private void OpenLab1()
        {
            // Простой вариант без Application
            var lab1Window = new Lab1Window();
            lab1Window.Show();

            // Закрываем текущее окно
            foreach (Window window in App.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void OpenLab2()
        {
            var lab2Window = new Lab2Window();
            lab2Window.Show();

            foreach (Window window in App.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void OpenLab3()
        {
            var lab3Window = new Lab3Window();
            lab3Window.Show();

            foreach (Window window in App.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void OpenLab4()
        {
            var lab4Window = new Lab4Window();
            lab4Window.Show();

            foreach (Window window in App.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
        private void ExitApp()
        {
            Application.Current.Shutdown();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        // Конструктор с одним параметром
        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        // Конструктор с двумя параметрами
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute?.Invoke();
    }
}
