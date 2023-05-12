using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGeneratorNeo4j.Services
{
    public class Neo4jConnection : INeo4jConnection
    {
        public Task<IActionResult> TestConnection()
        {
            throw new NotImplementedException();
        }
    }
}
