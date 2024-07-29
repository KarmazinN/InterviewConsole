using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EmployeeService.Models
{
    [DataContract]
    public class Employee
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int? ManagerID { get; set; }

        [DataMember]
        public bool Enable { get; set; }

        [DataMember]
        public List<Employee> Subordinates { get; set; } = new List<Employee>();
    }
}
