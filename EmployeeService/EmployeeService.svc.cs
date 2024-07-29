using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using EmployeeService.Models;
using Newtonsoft.Json;

namespace EmployeeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IEmployeeService
    {
        private string connectionString = "Data Source=DESKTOP-PB0N439\\SQLEXPRESS;Initial Catalog=TestDb;Integrated Security=True;";

        public async Task<string> GetEmployeeById(int id)
        {
            Employee employee = null;
            List<Employee> subordinates = new List<Employee>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Запит для вибірки основного працівника
                string employeeQuery = "SELECT ID, Name, ManagerID, Enable FROM Employee WHERE ID = @Id";
                using (var employeeCommand = new SqlCommand(employeeQuery, connection))
                {
                    employeeCommand.Parameters.AddWithValue("@Id", id);
                    using (var reader = await employeeCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            employee = new Employee
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ManagerID = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                Enable = reader.GetBoolean(3)
                            };
                        }
                    }
                }

                if (employee != null)
                {
                    // Запит для вибірки підлеглих
                    string subordinatesQuery = "SELECT ID, Name, ManagerID, Enable FROM Employee WHERE ManagerID = @ManagerId";
                    using (var subordinatesCommand = new SqlCommand(subordinatesQuery, connection))
                    {
                        subordinatesCommand.Parameters.AddWithValue("@ManagerId", id);
                        using (var reader = await subordinatesCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var subordinate = new Employee
                                {
                                    ID = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    ManagerID = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                    Enable = reader.GetBoolean(3)
                                };

                                subordinates.Add(subordinate);
                            }
                        }
                    }
                    employee.Subordinates = subordinates;
                }
            }
            return JsonConvert.SerializeObject(employee);
        }

        public async Task EnableEmployee(int id, bool enable)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE Employee SET Enable = @Enable WHERE ID = @ID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Enable", enable);
                    command.Parameters.AddWithValue("@ID", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}