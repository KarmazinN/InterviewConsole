using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeService.Models
{
    public class Employee
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int? ManagerID { get; set; }
        public bool Enable { get; set; }
        public List<Employee> Subordinates { get; set; }

        public Employee()
        {
            Subordinates = new List<Employee>();
        }
    }
}
