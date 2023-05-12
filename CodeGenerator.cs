using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using System.Configuration;
using System.Globalization;
using System.Text;

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
                        classDefinition = classDefinition.Replace("public DATE", "DateTime");
                        classDefinition = classDefinition.Replace("BOOLEAN", "bool");
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
                            txtOutput.Clear(); // Clear previous output
                            foreach (var pair in classDefinitions)
                            {
                                string filePath = Path.Combine(folderDialog.SelectedPath, pair.Key + ".cs");
                                File.WriteAllText(filePath, pair.Value);
                                txtOutput.Text += $"{pair.Value}\n\n"; // Display class definitions
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

        private void GenerateCode_Click(object sender, EventArgs e)
        {
            string input = txtCodeSection.Text;
            string modelClass = GenerateClass(input);
            string neo4jInterface = GenerateSignatureForIneo4jInterface(txtCodeSection.Text);
            string neo4jService = GenerateFunctionForNeo4jService(input);
            string controllerMethod = GenerateControllerMethod(input);

            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string outputDir = folderDialog.SelectedPath;

                    // Delete the content of the selected folder if it exists
                    DirectoryInfo di = new DirectoryInfo(outputDir);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    // Write the content of myclass to MyClass.cs in the selected folder
                    File.WriteAllText(Path.Combine(outputDir, "ModelClasses.cs"), modelClass);

                    // Write the content of myfunction to MyFunction.cs in the selected folder
                    File.WriteAllText(Path.Combine(outputDir, "INeo4jService.cs"), neo4jInterface);

                    // Write the content of myfunction to MyFunction.cs in the selected folder
                    File.WriteAllText(Path.Combine(outputDir, "Neo4jService.cs"), neo4jService);

                    File.WriteAllText(Path.Combine(outputDir, "Controller.cs"), controllerMethod);

                    MessageBox.Show("Files have been written successfully.");
                }
            }
        }

        private string GenerateClass(string input)
        {
            string[] lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            string className = "public class " + lines[0] + Environment.NewLine + "{" + Environment.NewLine;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Contains("<"))
                {
                    continue;
                }

                string[] keyValue = line.Split(':');
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();

                string property = "";

                if (int.TryParse(value, out _))
                {
                    property = $"public int {key} {{ get; set; }}";
                }
                else if (bool.TryParse(value, out _))
                {
                    property = $"public bool {key} {{ get; set; }}";
                }
                else if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    property = $"public decimal {key} {{ get; set; }}";
                }
                else if (value.StartsWith("\""))
                {
                    property = $"public string {key} {{ get; set; }}";
                }

                else if (value.StartsWith("[") && value.EndsWith("]"))
                {
                    value = value.Trim('[', ']');
                    if (int.TryParse(value, out _))
                    {
                        property = $"public int[] {key} {{ get; set; }}";
                    }
                    else if (bool.TryParse(value, out _))
                    {
                        property = $"public bool[] {key} {{ get; set; }}";
                    }
                    else if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        property = $"public decimal[] {key} {{ get; set; }}";
                    }
                    else if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        property = $"public string[] {key} {{ get; set; }}";
                    }
                }

                if (property != "")
                {
                    className += $"{property}{Environment.NewLine}";
                }
            }

            className += "}";

            return className;
        }
        private string GenerateSignatureForIneo4jInterface(string input)
        {
            string[] lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string firstLine = lines[0].Trim();

            if (firstLine.StartsWith("<") && firstLine.EndsWith(">"))
            {
                // Ignore the line and get the next one
                firstLine = lines[1].Trim();
            }

            string className = firstLine.Split(':')[0].Trim();
            return $"Task<{className}> Create{className}Async({className} {className.ToLower()});";
        }
        public static string GenerateFunctionForNeo4jService(string input)
        {
            string[] lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string className = lines[0];

            string functionBody = $"public async Task<{className}> Create{className}Async({className} {className.ToLower()})" + Environment.NewLine + "{" + Environment.NewLine;
            functionBody += "var query = @\"\"\"" + Environment.NewLine;

            functionBody += $"CREATE (c:{className} {{" + Environment.NewLine;

            foreach (string line in lines.Skip(1))
            {
                if (line.Contains("<"))
                {
                    continue;
                }

                string[] keyValue = line.Split(':');
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();

                functionBody += $"{key}: ${key.ToLower()}," + Environment.NewLine;
            }

            functionBody = functionBody.TrimEnd(',', '\n') + Environment.NewLine + "})" + Environment.NewLine + "RETURN c\"\"\";" + Environment.NewLine;

            functionBody += "var parameters = new" + Environment.NewLine + "{" + Environment.NewLine;

            foreach (string line in lines.Skip(1))
            {
                if (line.Contains("<"))
                {
                    continue;
                }

                string[] keyValue = line.Split(':');
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();

                functionBody += $"{key.ToLower()} = {className.ToLower()}.{key}," + Environment.NewLine;
            }

            functionBody += "};" + Environment.NewLine;

            functionBody += @"IAsyncSession session = _driver.AsyncSession();

        try
        {
            var createdNode = await session.WriteTransactionAsync(async tx =>
            {
                var r = await tx.RunAsync(query, parameters);
                var record = await r.PeekAsync();
                return record[0].As<INode>();
            });

            return ConvertNodeTo" + className + @"(createdNode);
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    private " + className + @" ConvertNodeTo" + className + @"(INode node)
    {
        return new " + className + Environment.NewLine + "{" + Environment.NewLine;

            foreach (string line in lines.Skip(1))
            {
                if (line.Contains("<"))
                {
                    continue;
                }

                string[] keyValue = line.Split(':');
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();

                functionBody += $"{key} = node.Properties[\"{key}\"].As<string>(),";
                functionBody += Environment.NewLine;
            }

            functionBody = functionBody.TrimEnd(',', '\n') + Environment.NewLine + "};" + Environment.NewLine + "}" + Environment.NewLine;

            return functionBody;
        }
        private string GenerateControllerMethod(string input)
        {
            string[] lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string firstLine = lines[0].Trim();

            if (firstLine.StartsWith("<") && firstLine.EndsWith(">"))
            {
                // Ignore the line and get the next one
                firstLine = lines[1].Trim();
            }

            string className = firstLine.Split(':')[0].Trim();
            string lowerClassName = className.ToLower();
            StringBuilder output = new StringBuilder();

            output.AppendLine("[HttpPost]");
            output.AppendLine($"public async Task<IActionResult> Create{className}([FromBody] {className} {lowerClassName})");
            output.AppendLine("{");
            output.AppendLine("    if (!ModelState.IsValid)");
            output.AppendLine("    {");
            output.AppendLine("        return BadRequest(ModelState);");
            output.AppendLine("    }");
            output.AppendLine("");
            output.AppendLine($"    var created{className} = await _neo4jService.Create{className}Async({lowerClassName});");
            output.AppendLine($"    return Ok(created{className});");
            output.AppendLine("}");

            return output.ToString();
        }
    }
}