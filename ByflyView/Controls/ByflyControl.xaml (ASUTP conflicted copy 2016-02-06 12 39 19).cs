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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeJelly.Byfly.ByflyView.Controls
{
    /// <summary>
    /// Логика взаимодействия для ByflyControl.xaml
    /// </summary>
    public partial class ByflyControl : UserControl
    {
        public State CurrentState
        {
            get { return ((State)GetValue(StateProperty)); }
            set
            {
                if(value!=CurrentState)
                {
                SetValue(StateProperty, value);

                switch(value)
                {
                    case State.Error:
                        RaiseStateEvent(ReadyToErrorStateEvent);
                        break;
                        case State.Logged
                }
                }
            }
        }

        public static readonly DependencyProperty StateProperty =
     DependencyProperty.Register("CurrentState", typeof(State), typeof(ByflyControl));

        public static readonly RoutedEvent ReadyToLoginStateEvent = EventManager.RegisterRoutedEvent("ReadyToLoginState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        public static readonly RoutedEvent ReadyToErrorStateEvent = EventManager.RegisterRoutedEvent("ReadyToErrorState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        public static readonly RoutedEvent LoggedStateEvent = EventManager.RegisterRoutedEvent("ReadyToErrorState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));

        public event RoutedEventHandler ReadyToLoginState
        {
            add { AddHandler(ReadyToLoginStateEvent, value); }
            remove { RemoveHandler(ReadyToLoginStateEvent, value); }
        }
        public event RoutedEventHandler ReadyToErrorState
        {
            add { AddHandler(ReadyToErrorStateEvent, value); }
            remove { RemoveHandler(ReadyToErrorStateEvent, value); }
        }


        public ByflyControl()
        {
            InitializeComponent();
            CurrentState = State.Login;
            RaiseStateEvent();
        }

        public void RaiseStateEvent(RoutedEvent rEvent)
        {
            var newEventArgs = new RoutedEventArgs(rEvent);
            RaiseEvent(newEventArgs);
        }

        void ByflyControl_StateChanged(object sender, RoutedEventArgs e)
        {
        }

        private void loginTb_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void loginTb_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void progressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as MahApps.Metro.Controls.ProgressRing).Visibility == Visibility.Hidden)
                if (!string.IsNullOrEmpty((string)balanceLabel.Content))
                    CurrentState = State.Logged;
                else if (!string.IsNullOrEmpty(errorTbl.Text))
                    CurrentState = State.Error;
        }

    }

    public enum State
    {
        Login,
        Error,
        Logged
    }

}
