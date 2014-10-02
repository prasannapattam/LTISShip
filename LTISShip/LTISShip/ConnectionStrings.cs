using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTISShip
{
    public class ConnectionStrings
    {
        public static string LTIS
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["LTConnectionString"].ConnectionString;
            }
        }
        public static string ShipWorks
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ShipWorksConnectionString"].ConnectionString;
            }
        }
    }
}
