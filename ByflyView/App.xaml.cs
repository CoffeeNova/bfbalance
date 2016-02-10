using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace CoffeeJelly.Byfly.ByflyView
{


    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] startupArguments;
        public TaskbarIcon notifyIcon;

        private void Application_startup(object sender, StartupEventArgs e)
        {
            startupArguments = e.Args;
        }
    }
}
