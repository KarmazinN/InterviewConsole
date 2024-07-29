using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EmployeeService.Models;
using Newtonsoft.Json;

namespace EmployeeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IEmployeeService
    {
        private string connectionString = "Data Source=DESKTOP-PB0N439\\SQLEXPRESS;Initial Catalog=TestDb;Integrated Security=True;";

        public string GetEmployeeById(int id)
        {
            var employees = new Dictionary<int, Employee>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Employee";
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employee = new Employee
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            ManagerID = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                            Enable = reader.GetBoolean(3)
                        };

                        employees[employee.ID] = employee;
                    }
                }
            }

            foreach (var employee in employees.Values)
            {
                if (employee.ManagerID.HasValue && employees.ContainsKey(employee.ManagerID.Value))
                {
                    employees[employee.ManagerID.Value].Subordinates.Add(employee);
                }
            }

            var result = employees.ContainsKey(id) ? employees[id] : null;
            return JsonConvert.SerializeObject(result);
        }

        public void EnableEmployee(int id, bool enable)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Employee SET Enable = @Enable WHERE ID = @ID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Enable", enable);
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}