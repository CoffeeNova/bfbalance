using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeJelly.Byfly.ByflyView.Controls
{
    /// <summary>
    /// Логика взаимодействия для ByflyStateController.xaml
    /// </summary>
    public partial class ByflyStateController : UserControl
    {
        public State CurrentState
        {
            get { return ((State)GetValue(StateProperty)); }
            set
            {
                SetValue(StateProperty, value);
            }
        }

        public static readonly DependencyProperty StateProperty =
     DependencyProperty.Register("CurrentState", typeof(State), typeof(ByflyControl));

        public ByflyStateController()
        {
            InitializeComponent();
        }
    }
    //public enum State
    //{
    //    Login,
    //    Error,
    //    Logged
    //}
}
