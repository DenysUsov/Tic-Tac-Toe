using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    public class GameEventArgs : EventArgs
    {
        public string event_parameter { get; set; }
        public object? Control { get; set; }
    }
}
