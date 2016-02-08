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
using System.Windows.Threading;

namespace CoffeeJelly.Byfly.ByflyView.Controls
{
    /// <summary>
    /// Логика взаимодействия для ByflyControl.xaml
    /// </summary>
    public partial class ByflyControl : UserControl
    {
        public State ControlState
        {
            get { return ((State)GetValue(StateProperty)); }
            set
            {
                if (value != ControlState)
                {
                    switch (value)
                    {
                        case State.Error:
                            RaiseStateEvent(ReadyToErrorStateEvent);
                            break;
                        case State.Logged:
                            RaiseStateEvent(ReadyToLoggedStateEvent);
                            break;
                        case State.Login:
                            RaiseStateEvent(ReadyToLoginStateEvent);
                            break;
                        case State.BlockError:
                            RaiseStateEvent(ReadyToBlockErrorStateEvent);
                            break;
                    }
                    SetValue(StateProperty, value);
                }
            }
        }

        public static readonly DependencyProperty StateProperty =
     DependencyProperty.Register("ControlState", typeof(State), typeof(ByflyControl));

        public static readonly RoutedEvent ReadyToLoginStateEvent = EventManager.RegisterRoutedEvent("ReadyToLoginState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        public static readonly RoutedEvent ReadyToErrorStateEvent = EventManager.RegisterRoutedEvent("ReadyToErrorState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        public static readonly RoutedEvent ReadyToLoggedStateEvent = EventManager.RegisterRoutedEvent("ReadyToLoggedState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        public static readonly RoutedEvent ReadyToBlockErrorStateEvent = EventManager.RegisterRoutedEvent("ReadyToBlockErrorState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));

        public event RoutedEventHandler ReadyToLoginState
        {
            add { AddHandler(ReadyToLoginStateEvent, value); }
            remove { RemoveHandler(ReadyToLoginStateEvent, value); }
        }
        public event RoutedEventHandler ReadyToLoggedState
        {
            add { AddHandler(ReadyToLoggedStateEvent, value); }
            remove { RemoveHandler(ReadyToLoggedStateEvent, value); }
        }
        public event RoutedEventHandler ReadyToErrorState
        {
            add { AddHandler(ReadyToErrorStateEvent, value); }
            remove { RemoveHandler(ReadyToErrorStateEvent, value); }
        }
        public event RoutedEventHandler ReadyToBlockErrorState
        {
            add { AddHandler(ReadyToBlockErrorStateEvent, value); }
            remove { RemoveHandler(ReadyToBlockErrorStateEvent, value); }
        }

        DependencyPropertyDescriptor dpdBalanceLabel = DependencyPropertyDescriptor.FromProperty(Label.ContentProperty, typeof(Label));
        DependencyPropertyDescriptor dpdErrorTextBlock = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
        DependencyPropertyDescriptor dpdBlockErrorLabel = DependencyPropertyDescriptor.FromProperty(Label.ContentProperty, typeof(Label));

        public ByflyControl()
        {
            InitializeComponent();

            dpdBalanceLabel.AddValueChanged(balanceLabel, (object a, EventArgs b) =>
            {
                if (!string.IsNullOrEmpty((string)(a as Label).Content))
                    ControlState = State.Logged;
            });
            dpdErrorTextBlock.AddValueChanged(errorTbl, (object a, EventArgs b) =>
            {
                if (!string.IsNullOrEmpty((a as TextBlock).Text))
                    ControlState = State.Error;
                else if(ControlState == State.Error)
                    ControlState = State.Login;
            });
            dpdBlockErrorLabel.AddValueChanged(_blockTimeLabel, (object a, EventArgs b) =>
            {
                if ((TimeSpan)(a as Label).Content > TimeSpan.Zero)
                    ControlState = State.BlockError;
            });
            ControlState = State.Login;
            //test
            //new System.Threading.Thread(() => { System.Threading.Thread.Sleep(4000); Dispatcher.Invoke(new Action(() => { errorTbl.Text = "123"; })); }).Start();
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

        private void progressRing_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if ((sender as MahApps.Metro.Controls.ProgressRing).Visibility == Visibility.Hidden)
            //    if (!string.IsNullOrEmpty((string)balanceLabel.Content))
            //        CurrentState = State.Logged;

        }

        private void errorBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (ControlState == State.Error)
            //    ControlState = State.Login;
            errorBorder.Tag = true;
            errorBorder.Tag = false;
        }

        private void passwordBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                //Get our ByflyClient
                var bfc = GetByflyClient();
                bfc.GetAccountData();
            }

        }

        private ByflyClient GetByflyClient()
        {
            var bfControlsDaddy = VisualTreeHelper.GetParent(this);
            var bfControlsGrandDaddy = VisualTreeHelper.GetParent(bfControlsDaddy) as ListViewItem;
            var bfClient = bfControlsGrandDaddy.Content as ByflyClient;
            return bfClient;
        }
        private void loginTb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                passwordBox.Focus();
        }


    }

    public enum State
    {
        Login,
        Error,
        Logged,
        BlockError
    }

}
