using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace InterviewConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            DataTable dtEmployees = GetQueryResult("SELECT * FROM Employees");
        }
        
        private static DataTable GetQueryResult(string query)
        {
            var dt = new DataTable();

			using (var connection = new SqlConnection("Data Source=DESKTOP-PB0N439\\SQLEXPRESS;Initial Catalog=TestDb;Integrated Security=True;"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
					command.CommandText = query;

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

			return dt;
        }
    }
}
