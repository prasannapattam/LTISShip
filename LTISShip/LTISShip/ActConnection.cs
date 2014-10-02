using Act.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTISShip
{
    public class ActConnection : IDisposable
    {
        private ActFramework fm;
        public ActConnection()
        {
            string actPad = ConfigurationManager.AppSettings["ActPad"];
            string actUserName = ConfigurationManager.AppSettings["ActUserName"];
            string actPassword = ConfigurationManager.AppSettings["ActPassword"];
            fm = new ActFramework();
            fm.LogOn(actPad, actUserName, actPassword);

        }

        public ActFramework Framework
        {
            get { return fm; }
        }

        public void Dispose()
        {
            fm.Dispose();
        }
    }
}
