using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Data.SqlClient;

namespace Cms.Buildeploy.Tasks
{
    public class SqlServerDropDatabase : Task
    {
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string DatabaseName { get; set; }
        public override bool Execute()
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"Drop database [{DatabaseName}]";
                    command.ExecuteNonQuery();
                }
            }

            return true;
        }
    }
}