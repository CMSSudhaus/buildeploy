using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Cms.Buildeploy.Tasks
{
    public class SqlServerRestoreDatabase : Task
    {

        [Required]
        public string ConnectionString { get; set; }


        public override bool Execute()
        {
            using (var connection = new SqlConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    return true;
                }
            }

        }
    }
}
