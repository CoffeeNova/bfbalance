using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CoffeeJelly.Byfly.BFlib.Controls
{
    /// <summary>
    /// http://stackoverflow.com/questions/2278525/system-timers-timer-how-to-get-the-time-remaining-until-elapse
    /// </summary>
    public class TimerImproved : System.Timers.Timer
    {
        private DateTime m_dueTime;

        public TimerImproved() : base()
        {
            this.Elapsed += this.ElapsedAction;
        }

        protected new void Dispose()
        {
            this.Elapsed -= this.ElapsedAction;
            base.Dispose();
        }

        public double TimeLeft
        {
            get
            {
                return (this.m_dueTime - DateTime.Now).TotalMilliseconds;
            }
        }

        public new void Start()
        {
            this.m_dueTime = DateTime.Now.AddMilliseconds(this.Interval);
            base.Start();
        }

        private void ElapsedAction(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.AutoReset)
            {
                this.m_dueTime = DateTime.Now.AddMilliseconds(this.Interval);
            }
        }
    }
}
