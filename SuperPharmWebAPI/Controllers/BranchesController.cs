using GoogleApi.Entities.Places.Search.Text.Response;
using GoogleApi.Entities.Places.Search.Text.Request;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using SuperPharmWebAPI.Models;
using System.Collections.Generic;
using GoogleApi.Entities.Places.Common;
using System.Globalization;
using CsvHelper;
using GoogleApi.Entities.Search.Common;
using GoogleApi.Entities.Search.Video.Common;


namespace SuperPharmWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly ILogger<BranchesController> _logger;

        private const string API_KEY = "AIzaSyB3Dmy3t9ZBW5sj7zFIHqB6tfN6Z-TVoxA";
        public BranchesController(ILogger<BranchesController> logger)
        {
            _logger = logger;

        }

        [HttpGet]
        public async Task<IActionResult> FetchBranches([FromQuery] string keyword)
        {
            var allResults = new List<PlaceResult>();
            string nextPageToken = string.Empty;

            try
            {
                do
                {
                    var request = new PlacesTextSearchRequest
                    {
                        Key = API_KEY,
                        Query = keyword,
                        Location = new GoogleApi.Entities.Common.Coordinate(32.0853, 34.7818), //meanwhile only central district
                        Radius = 50000
                    };

                    if (!string.IsNullOrEmpty(nextPageToken)) request.PageToken = nextPageToken;

                    var response = await GoogleApi.GooglePlaces.Search.TextSearch.QueryAsync(request);

                    if (response.Status != GoogleApi.Entities.Common.Enums.Status.Ok)
                        return BadRequest("Failed to fetch data from Google Maps.");

                    allResults.AddRange(response.Results);
                    nextPageToken = response.NextPageToken;

                    if (!string.IsNullOrEmpty(nextPageToken))
                        await Task.Delay(2000);

                } while (!string.IsNullOrEmpty(nextPageToken));

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching branches: {ex.Message}");
                return StatusCode(500, "Internal server error while fetching branches.");
            }

            var branchResponses = allResults.Select(branch => new Branch
            {
                BranchName = branch.Name,
                Address = branch.FormattedAddress,
                City = ExtractCity(branch.FormattedAddress),
                IsOpen24Hours = branch.OpeningHours?.OpenNow ?? false,
                LatitudeY = branch.Geometry.Location.Latitude,
                LongitudeX = branch.Geometry.Location.Longitude
            }).ToList();

            string time = DateTime.Now.ToString("HH_mm");
            ExportToCSV(branchResponses, $"CSVs\\Branches_{time}.csv");

            branchResponses.ForEach(branch =>
            {
                (branch.yITM, branch.xITM) = CoordinateTransformHelper.TransformToItm(branch.LatitudeY, branch.LongitudeX);
            });

            ExportToCSV(branchResponses, $"CSVs\\BranchedITM_{time}.csv");

            return Ok(branchResponses);
        }

        private string ExtractCity(string formattedAddress)
        {
            var parts = formattedAddress.Split(',').ToList();
            return parts.Count > 1 ? parts[1].Trim() : "Unknown";
        }

        private void ExportToCSV(IEnumerable<Branch> branches, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(branches);
        }       
    }
}
