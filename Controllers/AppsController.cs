using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using OfficeOpenXml;
using Newtonsoft.Json.Linq;

namespace TravelFactory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppsController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AppsController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult GetApps()
        {
            // return a list of all applications
            var translationsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "translations");
            var files = Directory.GetFiles(translationsDirectory, "*.json");
            var appNames = files.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
            return Ok(appNames);
        }
        
        [HttpPost]
        public IActionResult CreateApp([FromBody] string appName)
        {
            var translationsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "translations");
            var filePath = Path.Combine(translationsDirectory, $"{appName}.json");

            // Check if file already exists. If yes, return a conflict response.
            if (System.IO.File.Exists(filePath))
            {
                return Conflict("An app with this name already exists.");
            }

            // Create a new JSON file with the given app name.
            using (var file = System.IO.File.Create(filePath))
            {
                // Create a new TranslationData object with dummy data
                var translationData = new TranslationData
                {
                    Name = appName,
                    Last_updated = DateTime.Now,
                    Translations = new Dictionary<string, Dictionary<string, string>>
                    {
                        {
                            "hello", new Dictionary<string, string>
                            {
                                {"en", "Welcome"},
                                {"fr", "Bonjour"},
                                {"es", "Bienvenido"}
                            }
                        }
                    }
                };

                // Serialize the TranslationData object to JSON
                var json = JsonConvert.SerializeObject(translationData);

                var bytes = Encoding.UTF8.GetBytes(json);
                file.Write(bytes, 0, bytes.Length);
            }

            // Read the file content
            var fileContent = System.IO.File.ReadAllText(filePath);

            // Return the JSON response
            return Ok(fileContent);
        }

        [HttpGet("{appName}/translations")]
        public IActionResult GetTranslations(string appName)
        {
            var translationsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "translations");
            var filePath = Path.Combine(translationsDirectory, $"{appName}.json");

            // Check if file exists. If not, return a not found response.
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("The specified app does not exist.");
            }

            // Read the JSON file and return its content.
            var json = System.IO.File.ReadAllText(filePath);
            return Ok(json);
        }

        [HttpPost("{appName}/deploy")]
        public IActionResult AddTranslation(string appName, [FromBody] JObject translationData)
        {
            var translationsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "translations");
            var filePath = Path.Combine(translationsDirectory, $"{appName}.json");

            // Convert the JObject to a string
            var jsonData = JsonConvert.SerializeObject(translationData, Formatting.Indented);

            // Write the JSON data to the file, overwriting any existing content
            System.IO.File.WriteAllText(filePath, jsonData);

            return Ok($"Translation for {appName} has been saved.");
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadTranslations(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID cannot be null or empty.");
            }

            var jsonFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, $"translations/{id}.json");

            if (!System.IO.File.Exists(jsonFilePath))
            {
                return NotFound($"File {jsonFilePath} does not exist.");
            }

            var jsonData = await System.IO.File.ReadAllTextAsync(jsonFilePath);

            if (jsonData == null)
            {
                return BadRequest("File is empty.");
            }

            var data = JsonConvert.DeserializeObject<TranslationData>(jsonData);

            if (data == null || data.Translations == null)
            {
                return BadRequest("Invalid JSON data.");
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Translations");

                worksheet.Cells[1, 1].Value = "Keyword";
                worksheet.Cells[1, 2].Value = "Language";
                worksheet.Cells[1, 3].Value = "Translation";

                int row = 2;
                foreach (var translation in data.Translations)
                {
                    foreach (var language in translation.Value)
                    {
                        worksheet.Cells[row, 1].Value = translation.Key;
                        worksheet.Cells[row, 2].Value = language.Key;
                        worksheet.Cells[row, 3].Value = language.Value;
                        row++;
                    }
                }

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{id}_Translations.xlsx");
            }
        }
    }
}