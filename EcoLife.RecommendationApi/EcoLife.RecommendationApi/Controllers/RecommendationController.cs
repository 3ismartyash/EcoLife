using Microsoft.AspNetCore.Mvc;

using EcoLife.RecommendationApi.Models; // Your RecommendationEntity model

using System.Net.Http;

using System.Net.Http.Json;

using System;

using System.Collections.Generic;

namespace EcoLife.RecommendationApi.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class RecommendationController : ControllerBase

    {

        private readonly HttpClient _httpClient;

        public RecommendationController(IHttpClientFactory httpClientFactory)   

        {

            _httpClient = httpClientFactory.CreateClient();

        }

        [HttpGet("{userId}")]

        public async Task<IActionResult> GetRecommendations(int userId)

        {

            double totalEmissions = 0;

            try

            {

                // **IMPORTANT:** Replace these with your actual API URLs and ports!

                string transportationUrl = $"http://localhost:<transport-aggregator-port>/api/TransportationAggregator/{userId}";

                string householdUrl = $"http://localhost:<household-aggregator-port>/api/HouseholdAggregator/{userId}";

                string wasteManagementUrl = $"http://localhost:<waste-aggregator-port>/api/WasteManagementAggregator/{userId}";

                async Task<double> GetEmissionsFromApi(string apiUrl)

                {

                    var response = await _httpClient.GetAsync(apiUrl);

                    if (!response.IsSuccessStatusCode)

                    {

                        Console.WriteLine($"Error calling API: {apiUrl} - Status Code: {response.StatusCode}"); // Log the error

                        return 0; // Or handle the error differently (e.g., throw an exception, return a specific status code)

                    }

                    var result = await response.Content.ReadFromJsonAsync<dynamic>();

                    if (result == null || !result.ContainsKey("TotalEmissions"))

                    {

                        Console.WriteLine($"API response does not contain TotalEmissions: {apiUrl}");

                        return 0; // Or handle as needed

                    }

                    return (double)result.TotalEmissions;

                }

                totalEmissions += await GetEmissionsFromApi(transportationUrl);

                totalEmissions += await GetEmissionsFromApi(householdUrl);

                totalEmissions += await GetEmissionsFromApi(wasteManagementUrl);

                string category;

                List<string> dialogBoxes;

                if (totalEmissions < 10) // Example thresholds - ADJUST THESE

                {

                    category = "Good";

                    dialogBoxes = new List<string>() {

            "Excellent! You have a low carbon footprint.",

            "Well done! Your efforts are making a difference.",

            "Keep up the great work! You're setting a positive example."

          };

                }

                else if (totalEmissions < 20) // Example thresholds - ADJUST THESE

                {

                    category = "Average";

                    dialogBoxes = new List<string>() {

            "Your carbon footprint is average. There's room for improvement.",

            "You're doing okay, but consider exploring ways to reduce your impact.",

            "Not bad, but let's aim for a lower footprint together."

          };

                }

                else

                {

                    category = "Bad";

                    dialogBoxes = new List<string>() {

            "Your carbon footprint is high. Consider making significant changes.",

            "You have a substantial impact. Explore options to reduce your emissions.",

            "Your current lifestyle is contributing heavily to emissions. Let's work on reducing that."

          };

                }

                Random random = new Random();

                string selectedDialog = dialogBoxes[random.Next(dialogBoxes.Count)];

                var recommendation = new RecommendationEntity

                {

                    Category = category,

                    Message = selectedDialog,

                    TotalEmissions = totalEmissions

                };

                return Ok(recommendation);

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Exception in GetRecommendations: {ex}"); // Log the exception

                return StatusCode(500, $"An error occurred: {ex.Message}");

            }

        }

    }

}

