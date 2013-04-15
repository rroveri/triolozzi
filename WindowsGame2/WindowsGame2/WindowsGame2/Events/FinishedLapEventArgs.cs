using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame2.Events
{
    class FinishedLapEventArgs : EventArgs
    {
        public FinishedLapEventArgs(int lapNumber)
        {
            LapNumber = lapNumber;
        }

        public int LapNumber { get; set; }
    }
}
