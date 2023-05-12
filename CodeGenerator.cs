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

        private async void btnGenerateClasses_Click(object sender, EventArgs e)
        {
            var neo4jSettings = _configuration.GetSection("Neo4j");
            var driver = GraphDatabase.Driver(neo4jSettings["Url"], AuthTokens.Basic(neo4jSettings["Username"], neo4jSettings["Password"]));

            string cypherQueryLabels = @"
                                CALL db.labels()
                                YIELD label
                                RETURN label";

            try
            {
                using var session = driver.AsyncSession();
                txtQuery.Text += cypherQueryLabels + Environment.NewLine;  // Display the query
                var labelsResult = await session.RunAsync(cypherQueryLabels);
                var labels = await labelsResult.ToListAsync(record => record[0].As<string>());

                var classDefinitions = new Dictionary<string, string>();

                foreach (string label in labels)
                {
                    var queryParams = new Dictionary<string, object>
            {
                { "nodeLabel", label }
            };
                    string cypherQuery = @"
                        MATCH (n) WHERE $nodeLabel IN labels(n)
                        WITH n, size(keys(n)) as numProperties
                        ORDER BY numProperties DESC
                        LIMIT 1
                        WITH keys(n) as properties, [k IN keys(n) | [k, apoc.meta.cypher.type(n[k])]] as propertiesWithTypes
                        WITH ""public class "" + $nodeLabel + "" {"" +
                            reduce(acc = """", propWithType in propertiesWithTypes |
                            acc + ""    public "" + propWithType[1] + "" "" + propWithType[0] + "" { get; set; }"") + ""}"" AS class_definition
                        RETURN class_definition";

                    txtQuery.Text += cypherQuery + Environment.NewLine;  // Display the query
                    var result = await session.RunAsync(cypherQuery, queryParams);
                    var records = await result.ToListAsync(record => record[0].As<string>());
                    var classDefinition = records.SingleOrDefault();

                    if (classDefinition != null)
                    {
                        classDefinition = classDefinition.Replace("INTEGER", "int");
                        classDefinition = classDefinition.Replace("STRING", "string");
                        classDefinition = classDefinition.Replace("list of STRING", "List<string>");
                        classDefinition = classDefinition.Replace("public date", "DateTime");
                        classDefinition = classDefinition.Replace("boolean", "bool");
                        classDefinitions[label] = classDefinition;
                    }
                }

                if (classDefinitions.Count == 0)
                {
                    MessageBox.Show("No class definitions found", "Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            foreach (var pair in classDefinitions)
                            {
                                string filePath = Path.Combine(folderDialog.SelectedPath, pair.Key + ".cs");
                                File.WriteAllText(filePath, pair.Value);
                            }

                            MessageBox.Show("Class definitions have been written to files.", "Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating classes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}