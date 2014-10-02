using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace LTISShip
{
    class Program
    {
        static void Main(string[] args)
        {
            var shipWoks = new ShipWorks();
            shipWoks.Process();
        }
    }
}
