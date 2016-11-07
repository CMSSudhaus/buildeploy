using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;

namespace Cms.Buildeploy.Tasks
{

    public class SqlServerRestoreDatabase : Task
    {

        [Required]
        public string ConnectionString { get; set; }


        [Required]
        public string DatabaseBackupPath { get; set; }

        [Required]
        public string RestoreDatabaseName { get; set; }

        public override bool Execute()
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                connection.InfoMessage += (s, e) => Console.WriteLine(e.Message);
                string dataDirectory;
                string logDirectory;

                StringBuilder sb = new StringBuilder($"Restore database [{RestoreDatabaseName}] from disk='{DatabaseBackupPath}' with stats=10,replace");
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select     serverproperty('InstanceDefaultDataPath'),serverproperty('InstanceDefaultLogPath');" +
                        $"Restore filelistonly from disk='{DatabaseBackupPath}'";


                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dataDirectory = reader.GetString(0);
                            logDirectory = reader.GetString(1);
                        }
                        else
                            throw new InvalidOperationException("Cannot obtain default directories.");

                        reader.NextResult();
                        int fileIndex = 1;
                        while (reader.Read())
                        {
                            string logicalFileName = reader.GetString(0);
                            string physicalFileName = Path.Combine(dataDirectory,
                                RestoreDatabaseName + "_" + fileIndex.ToString(CultureInfo.InvariantCulture) + Path.GetExtension(reader.GetString(1)));
                            sb.AppendLine(",");
                            sb.Append($"move '{logicalFileName}' to '{physicalFileName}'");
                        }
                    }

                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();
                }
            }

            return true;
        }
    }
}
