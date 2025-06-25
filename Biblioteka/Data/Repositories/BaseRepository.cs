namespace Biblioteka.Data.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;

        protected BaseRepository()
        {
            // Determine project root directory
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                .Parent.Parent.Parent.FullName;
            // Load appsettings.json
            var configPath = Path.Combine(projectDir, "appsettings.json");
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
            // Read connection string and map to database file
            var connBuilder = new SQLiteConnectionStringBuilder(config.GetConnectionString("DefaultConnection"));
            var dbPath = Path.Combine(projectDir, "database", Path.GetFileName(connBuilder.DataSource));
            _connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version = 3
            }.ToString();
        }
    }
}