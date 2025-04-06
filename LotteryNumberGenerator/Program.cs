using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;

class MegaMillionsGenerator
{
    static Random rng = new Random();

    static async Task Main()
    {
        // 1) Fetch current winning numbers from the API
        var winningNumbers = await FetchCurrentWinningNumbers();

        // 2) If we couldn't fetch them, stop here
        if (winningNumbers == null)
        {
            Console.WriteLine("Failed to fetch the latest winning numbers.");
            return;
        }

        // 3) Display the winning numbers
        Console.WriteLine("Latest Winning Numbers:");
        Console.WriteLine("Main Numbers: {0}", string.Join(", ", winningNumbers.MainNumbers));
        Console.WriteLine("Mega Ball: {0}", winningNumbers.MegaBall);

        // 4) Generate new numbers excluding the winning numbers
        var (mainNumbers, megaBall) = GenerateNumbersExcludingWinning(winningNumbers);

        // 5) Print your newly generated numbers
        Console.WriteLine("\nYour Mega Millions numbers are: {0} and Mega Ball: {1}",
            string.Join(", ", mainNumbers), megaBall);
    }

    /// <summary>
    /// Fetches the current winning Mega Millions numbers from CollectAPI
    /// and returns them as a LotteryNumbers object (main numbers + mega ball).
    /// </summary>
    static async Task<LotteryNumbers> FetchCurrentWinningNumbers()
    {
        try
        {
            // Base URL only
            var client = new RestClient("https://api.collectapi.com");

            // Endpoint path
            var request = new RestRequest("/chancegame/usaMegaMillions", Method.Get);

            // Retrieve the API key from an environment variable named "COLLECTAPI_KEY"
            string apiKey = Environment.GetEnvironmentVariable("COLLECTAPI_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API key is not set. Please set the COLLECTAPI_KEY environment variable.");
                return null;
            }

            // Proper header format: "apikey YOUR_KEY"
            request.AddHeader("authorization", "apikey " + apiKey);
            request.AddHeader("content-type", "application/json");

            // Make the HTTP call
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine("Raw JSON:");
            Console.WriteLine(response.Content);

            if (!response.IsSuccessful)
            {
                Console.WriteLine("Error fetching winning numbers: " + response.StatusDescription);
                return null;
            }

            // Deserialize the JSON into ApiResponse
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);

            // Make sure we got a valid response
            if (apiResponse == null || !apiResponse.Success || apiResponse.Result == null || apiResponse.Result.Numbers == null)
            {
                Console.WriteLine("No winning numbers found in the API response.");
                return null;
            }

            // Extract the mainNumbers and megaBall strings from the object
            var mainNumbersStr = apiResponse.Result.Numbers.MainNumbers; // e.g. "5-28-62-65-70"
            var megaBallStr = apiResponse.Result.Numbers.MegaBall;    // e.g. "5"

            // Convert them to numeric form
            var mainNumbers = mainNumbersStr.Split('-').Select(int.Parse).ToList();
            int megaBall = int.Parse(megaBallStr);

            // Return a LotteryNumbers object
            return new LotteryNumbers
            {
                MainNumbers = mainNumbers,
                MegaBall = megaBall
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching winning numbers: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Generates new numbers excluding the latest winning numbers.
    /// </summary>
    static (List<int> mainNumbers, int megaBall) GenerateNumbersExcludingWinning(LotteryNumbers winningNumbers)
    {
        // Exclude the winning main numbers from the pool 1..70
        List<int> mainNumberPool = Enumerable.Range(1, 70).Except(winningNumbers.MainNumbers).ToList();
        List<int> mainNumbers = GetUniqueRandomNumbersFromPool(5, mainNumberPool);

        // Exclude the winning mega ball from the pool 1..25
        List<int> megaBallPool = Enumerable.Range(1, 25).ToList();
        megaBallPool.Remove(winningNumbers.MegaBall);
        int megaBall = GetRandomNumberFromList(megaBallPool);

        mainNumbers.Sort();
        return (mainNumbers, megaBall);
    }

    /// <summary>
    /// From a given pool, get 'count' unique random numbers as a list.
    /// </summary>
    static List<int> GetUniqueRandomNumbersFromPool(int count, List<int> numberPool)
    {
        HashSet<int> numbers = new HashSet<int>();
        while (numbers.Count < count && numberPool.Count > 0)
        {
            int index = rng.Next(0, numberPool.Count);
            numbers.Add(numberPool[index]);
            numberPool.RemoveAt(index);
        }
        return numbers.ToList();
    }

    /// <summary>
    /// Returns one random number from the given list, then no removal is needed for a single pick.
    /// </summary>
    static int GetRandomNumberFromList(List<int> numberPool)
    {
        if (numberPool.Count == 0)
            throw new Exception("Number pool is empty.");

        int index = rng.Next(0, numberPool.Count);
        return numberPool[index];
    }

    // Represents the final "winning numbers" data we care about.
    class LotteryNumbers
    {
        public List<int> MainNumbers { get; set; }
        public int MegaBall { get; set; }
    }

    // The top-level JSON structure from CollectAPI
    public class ApiResponse
    {
        public bool Success { get; set; }
        public SingleResult Result { get; set; }
    }

    // "result" in the JSON is a single object, not a list.
    public class SingleResult
    {
        public NumbersObj Numbers { get; set; }
        public string Date { get; set; }
        // Add other fields if needed
    }

    // "numbers" is an object with "MainNumbers" and "MegaBall".
    public class NumbersObj
    {
        public string MainNumbers { get; set; }
        public string MegaBall { get; set; }
    }
}
