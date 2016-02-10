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
                        case State.BlockError:
                            RaiseStateEvent(ReadyToErrorStateEvent);
                            break;
                        case State.Logged:
                            RaiseStateEvent(ReadyToLoggedStateEvent);
                            break;
                        case State.Login:
                            RaiseStateEvent(ReadyToLoginStateEvent);
                            break;
                        //case State.BlockError:
                        //    RaiseStateEvent(ReadyToBlockErrorStateEvent);
                        //    break;
                    }
                    SetValue(StateProperty, value);
                }
            }
        }


        public string ToolTipContent
        {
            get
            {
                if (_boundedClient == null)
                    return "";
                if (_boundedClient.IsGettingData)
                    return "Ожидайте, идет связь с сервером Белтелекома.";
                if (ControlState == State.Logged)
                    return _boundedClient.Abonent + ".\r\nТарифный план: " + _boundedClient.TariffPlan +".\r\nТекущий статус: " + _boundedClient.Status + ".";
                if (ControlState == State.Error)
                    return "Кликните один раз для возврата к меню логина.";
                if (ControlState == State.BlockError)
                    return string.Format("Кликните один раз для возврата к меню логина. \r\nДо окончания блокировки: {0} минут {1} секунд.", _boundedClient.BlockTime.Minutes, _boundedClient.BlockTime.Seconds);
                else
                    if (_boundedClient.IsBlocked)
                        return string.Format("До окончания блокировки: {0} минут {1} секунд.", _boundedClient.BlockTime.Minutes, _boundedClient.BlockTime.Seconds);
                    else
                        return "Введите имя пользователя (номер договора) и пароль. Нажмите Enter для получения состояния текущего баланса.";
            }

        }

       // private string _toolTipContent = "";

        public static readonly DependencyProperty StateProperty =
     DependencyProperty.Register("ControlState", typeof(State), typeof(ByflyControl));

        public static readonly RoutedEvent ReadyToLoginStateEvent = EventManager.RegisterRoutedEvent("ReadyToLoginState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        public static readonly RoutedEvent ReadyToErrorStateEvent = EventManager.RegisterRoutedEvent("ReadyToErrorState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        public static readonly RoutedEvent ReadyToLoggedStateEvent = EventManager.RegisterRoutedEvent("ReadyToLoggedState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));
        //public static readonly RoutedEvent ReadyToBlockErrorStateEvent = EventManager.RegisterRoutedEvent("ReadyToBlockErrorState", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ByflyControl));

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
        //public event RoutedEventHandler ReadyToBlockErrorState
        //{
        //    add { AddHandler(ReadyToBlockErrorStateEvent, value); }
        //    remove { RemoveHandler(ReadyToBlockErrorStateEvent, value); }
        //}

        DependencyPropertyDescriptor dpdBalanceLabel = DependencyPropertyDescriptor.FromProperty(Label.ContentProperty, typeof(Label));
        DependencyPropertyDescriptor dpdErrorTextBlock = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
        //DependencyPropertyDescriptor dpdBlockErrorLabel = DependencyPropertyDescriptor.FromProperty(Label.ContentProperty, typeof(Label));

        private ByflyClient _boundedClient;

        public ByflyControl()
        {
            InitializeComponent();
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
            //if(ControlState != State.BlockError)
            //    errorBorder.Tag = true;
        }

        private void passwordBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Get our ByflyClient
                CoffeeJelly.Byfly.ByflyView.ByflyTools.ParserCallbackDelegate pcd = new ByflyTools.ParserCallbackDelegate((b) => b.GetAccountData());
                pcd.BeginInvoke(_boundedClient, null, null);
            }

        }

        private ByflyClient GetBoundedByflyClient()
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

        private void bfc_Initialized(object sender, EventArgs e)
        {
           
        }

        private void bfc_Loaded(object sender, RoutedEventArgs e)
        {
            _boundedClient = GetBoundedByflyClient();

            dpdBalanceLabel.AddValueChanged(balanceLabel, (object a, EventArgs b) =>
            {
                if (!string.IsNullOrEmpty((string)(a as Label).Content))
                    ControlState = State.Logged;
            });
            dpdErrorTextBlock.AddValueChanged(errorTbl, (object a, EventArgs b) =>
            {
                if (!string.IsNullOrEmpty((a as TextBlock).Text))
                {
                    if (_boundedClient.IsBlocked)
                        ControlState = State.BlockError;
                    else
                        ControlState = State.Error;
                }
                else if (ControlState == State.Error || ControlState == State.BlockError)
                    ControlState = State.Login;
            });

            ControlState = State.Login;
        }

        private void bfc_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            bfc.ToolTip = ToolTipContent;
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
