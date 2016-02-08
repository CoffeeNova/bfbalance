using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoffeeJelly.Byfly.ByflyView.Controls
{
    public class ByflyStateControl : FrameworkElement
    {
         public State CurrentState
        {
            get { return ((State)GetValue(DependencyState)); }
            set
            {
                SetValue(DependencyState, value);
            }
        }

        public static readonly DependencyProperty DependencyState =
     DependencyProperty.Register("CurrentState", typeof(State), typeof(ByflyStateControl), new PropertyMetadata(false));

        public enum State
        {
            Login,
            Error,
            Logged
        }

    }
}
