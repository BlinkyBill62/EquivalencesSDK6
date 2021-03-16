using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class ServiceConfiguration
    {
        public ServiceConfiguration()
        {
            this.UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
            this.Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
        }

        public string UserName { get; private set; }
        public string Password { get; private set; }
    }
}