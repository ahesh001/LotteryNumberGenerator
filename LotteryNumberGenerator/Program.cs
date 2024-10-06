using System;
using System.Collections.Generic;
using System.Linq;

class MegaMillionsGenerator
{
    static void Main()
    {
        var (mainNumbers, megaBall) = GenerateUniqueMegaMillionsNumbers();
        Console.WriteLine("Your Mega Millions numbers are: {0} and Mega Ball: {1}",
            string.Join(", ", mainNumbers), megaBall);
    }

    static (List<int> mainNumbers, int megaBall) GenerateUniqueMegaMillionsNumbers()
    {
        // Avoid numbers 1-31 to reduce the chance of sharing the jackpot
        List<int> mainNumberPool = Enumerable.Range(32, 39).ToList(); // Numbers from 32 to 70
        List<int> mainNumbers = GetUniqueRandomNumbersFromPool(5, mainNumberPool);

        // Mega Ball remains the same
        int megaBall = GetRandomNumber(1, 25);

        mainNumbers.Sort(); // Sort the main numbers for readability
        return (mainNumbers, megaBall);
    }

    static List<int> GetUniqueRandomNumbersFromPool(int count, List<int> numberPool)
    {
        Random rng = new Random();
        HashSet<int> numbers = new HashSet<int>();
        while (numbers.Count < count)
        {
            int index = rng.Next(0, numberPool.Count);
            numbers.Add(numberPool[index]);
        }
        return numbers.ToList();
    }

    static int GetRandomNumber(int min, int max)
    {
        Random rng = new Random();
        return rng.Next(min, max + 1);
    }
}
