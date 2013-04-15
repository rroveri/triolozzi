using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGame2.GameElements;

namespace WindowsGame2.Events
{
    public class EliminatedCarEventArgs : EventArgs
    {
        public EliminatedCarEventArgs(Car car)
        {
            EliminatedCar = car;
        }

        public Car EliminatedCar { get; set; }
    }
}
