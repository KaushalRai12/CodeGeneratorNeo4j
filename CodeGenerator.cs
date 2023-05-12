using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using System.Configuration;

namespace CodeGeneratorNeo4j
{
    public partial class CodeGenerator : Form
    {
        private readonly IConfiguration _configuration;
        public CodeGenerator()
        {
            InitializeComponent();
            var builder = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
        }

        private async void btnTestConnection_Click(object sender, EventArgs e)
        {
            var neo4jSettings = _configuration.GetSection("Neo4j");
            var driver = GraphDatabase.Driver(neo4jSettings["Url"], AuthTokens.Basic(neo4jSettings["Username"], neo4jSettings["Password"]));
            string cypherQuery = "CALL dbms.components() YIELD name, versions UNWIND versions as version RETURN name, version";

            try
            {
                using var session = driver.AsyncSession();
                var result = await session.RunAsync(cypherQuery);
                var record = await result.SingleAsync();
                string name = record["name"].As<string>();
                string version = record["version"].As<string>();
                MessageBox.Show($"Connected to {name} version {version}", "Connection Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to Neo4j: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

       
    }
}