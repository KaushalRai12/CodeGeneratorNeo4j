using Microsoft.AspNetCore.Mvc;

namespace CodeGeneratorNeo4j.Services
{
    public interface INeo4jConnection
    {
        Task<IActionResult> TestConnection();
    }
}
