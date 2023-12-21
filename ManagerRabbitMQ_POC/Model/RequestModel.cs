using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerRabbitMQ_POC.Model
{
    public class RequestModel
    {
        public int ID { get; set; }

        public string URL { get; set; }

        public string MachineName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime RequestTime { get; set; }


    }
}
