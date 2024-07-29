using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EmployeeService.Models;
using System;

namespace EmployeeService
{
    public class Service1 : IEmployeeService
    {
        private readonly string connectionString = "Data Source=DESKTOP-PB0N439\\SQLEXPRESS;Initial Catalog=TestDb;Integrated Security=True;";
        private readonly IMemoryCache _cache;

        public Service1()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<string> GetEmployeeById(int id)
        {
            if (_cache.TryGetValue(id.ToString(), out string cachedResult))
            {
                return cachedResult;
            }

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

            var result = JsonConvert.SerializeObject(employee);

            if (employee != null)
            {
                _cache.Set(id.ToString(), result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
            }

            return result;
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

            if (_cache.TryGetValue(id.ToString(), out _))
            {
                _cache.Remove(id.ToString());
            }
        }
    }
}