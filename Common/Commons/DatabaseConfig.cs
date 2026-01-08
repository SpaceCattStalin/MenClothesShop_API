using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Commons
{
    public class DatabaseConfig
    {
        public DatabaseType DatabaseType { get; set; }
        public string Name { get; set; } = "";
        public string ConnectionString { get; set; } = "";
    }

    public class DatabaseSettings
    {
        public DatabaseConfig[] Databases { get; set; } = Array.Empty<DatabaseConfig>();
    }
}
