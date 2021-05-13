using Snowflake.Data.Client;
using Snowflake.FileStream.Model;
using System.Threading.Tasks;

namespace OracleTest
{
    class Program
    {
        static Task Main(string[] args)
        {
            return App.Execute(
                @"account=xxxxxx;host=xxxxx.eu-west-1.snowflakecomputing.com;user=xxxxxx@xxxxxxx;db=demo_db;schema=public;role=sysadmin;authenticator=externalbrowser",
                "User Id=xxxxx;Password=xxxxx;Data Source=xxxxx/xxxxxx"
                );
        }
    }
}
