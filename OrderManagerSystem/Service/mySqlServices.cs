//建立 Database Service
using MySqlConnector;

namespace OrderManagerSystem.Services
{
    public class MySqlService
    {  
        private readonly IConfiguration _configuration;

        public MySqlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MySqlConnection GetConnection1()
        {
            string connStr =
                _configuration.GetConnectionString("DefaultConnection1");

            return new MySqlConnection(connStr);
        }

        public MySqlConnection GetConnection2()
        {
            string connStr =
                _configuration.GetConnectionString("DefaultConnection2");

            return new MySqlConnection(connStr);
        }
    }
}