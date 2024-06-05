using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace PostGenerator
{
    internal class Helper
    {
        public static SQLiteConnection sqliteConn =
            new SQLiteConnection(@"Data Source=.\database.db;");
    }
}
