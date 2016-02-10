using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace CoffeeJelly.Byfly.ByflyView.Controls
{
    class bfMenuCommands
    {
        public string WindowsStateHeader
        {
            get { return Application.Current.MainWindow.WindowState == WindowState.Minimized ? "Развернуть" : "Свернуть"; }
            set { }
        }
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                    }
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }

         /// <summary>
         /// Hides the main window. This command is only enabled if a window is open.
         /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.WindowState = WindowState.Minimized,
                    CanExecuteFunc = () => Application.Current.MainWindow != null
                };
            }
        }

        /// <summary>
        /// Defines witch command should use in dependence of window state
        /// </summary>
        public ICommand DefineWindowCommand
        {
            get
            {
               if(Application.Current.MainWindow == null)
                   return null;
               return Application.Current.MainWindow.WindowState == WindowState.Minimized ? ShowWindowCommand : HideWindowCommand;
            }
        }

        /// <summary>
        /// This command added an account (ByflyClient) to listview collection
        /// </summary>
        public ICommand AddAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => MainWindow.AddAccount(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null
                };
            }
        }

        /// <summary>
        /// This command added an account (ByflyClient) to listview collection
        /// </summary>
        public ICommand DeleteAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => MainWindow.DeleteAccount(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null 
                };
            }
        }
    }

    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
