using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NonTransitiveDiceGame
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!ValidateArgs(args, out List<List<int>> diceSets))
            {
                Console.WriteLine("Invalid input. Example: 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3");
                return;
            }

            if (diceSets.Count < 3)
            {
                Console.WriteLine("At least 3 dice sets are required for a non-transitive game.");
                return;
            }

            FairRandomGenerator fairGenerator = new FairRandomGenerator();
            Console.WriteLine("Let's determine who makes the first move.");

            int computerChoice = fairGenerator.Generate(0, 1);
            string hmac = fairGenerator.GenerateHMAC(computerChoice);
            Console.WriteLine($"I selected a random value in the range 0..1 (HMAC={hmac}).");

            Console.WriteLine("Try to guess my selection.");
            Console.WriteLine("0 - 0");
            Console.WriteLine("1 - 1");
            Console.WriteLine("X - exit");
            Console.WriteLine("? - help");

            Console.Write("Your selection: ");
            string input = Console.ReadLine();

            if (input.ToLower() == "exit" || input.ToLower() == "x") return;
            if (input == "?")
            {
                ShowHelp(diceSets);
                return;
            }
            if (!int.TryParse(input, out int userGuess) || userGuess < 0 || userGuess > 1)
            {
                Console.WriteLine("Invalid input");
                return;
            }

            Console.WriteLine($"My selection: {computerChoice} (KEY={fairGenerator.Key}).");

            int computerDiceChoice = fairGenerator.Generate(0, diceSets.Count - 1);
            hmac = fairGenerator.GenerateHMAC(computerDiceChoice);
            Console.WriteLine($"I have chosen my dice (HMAC={hmac}).");

            Console.WriteLine("Choose your dice:");
            for (int i = 0; i < diceSets.Count; i++)
            {
                Console.WriteLine($"{i} - {string.Join(",", diceSets[i])}");
            }
            Console.WriteLine("X - exit");
            Console.WriteLine("? - help");

            Console.Write("Your selection: ");
            input = Console.ReadLine();

            if (input.ToLower() == "exit" || input.ToLower() == "x") return;
            if (input == "?")
            {
                ShowHelp(diceSets);
                return;
            }

            if (!int.TryParse(input, out int userDiceIndex) || userDiceIndex < 0 || userDiceIndex >= diceSets.Count)
            {
                Console.WriteLine("Invalid input");
                return;
            }

            Console.WriteLine($"I chose dice set {computerDiceChoice}: [{string.Join(",", diceSets[computerDiceChoice])}] (KEY={fairGenerator.Key})");

            Console.WriteLine("It's time for my throw.");
            int computerModChoice = fairGenerator.Generate(0, diceSets[0].Count - 1);
            hmac = fairGenerator.GenerateHMAC(computerModChoice);
            Console.WriteLine($"I selected a random value in the range 0..{diceSets[0].Count - 1} (HMAC={hmac}).");

            Console.WriteLine($"Add your number modulo {diceSets[0].Count}.");
            for (int i = 0; i < diceSets[0].Count; i++)
                Console.WriteLine($"{i} - {i}");
            Console.WriteLine("X - exit");
            Console.WriteLine("? - help");

            Console.Write("Your selection: ");
            input = Console.ReadLine();
            if (!int.TryParse(input, out int userMod) || userMod < 0 || userMod >= diceSets[0].Count) return;

            Console.WriteLine($"My number is {computerModChoice} (KEY={fairGenerator.Key}).");
            int resultMod = (computerModChoice + userMod) % diceSets[0].Count;
            Console.WriteLine($"The result is {computerModChoice} + {userMod} = {resultMod} (mod {diceSets[0].Count}).");

            int computerRoll = diceSets[computerDiceChoice][resultMod];
            Console.WriteLine($"My throw is {computerRoll}.");

            Console.WriteLine("It's time for your throw.");
            computerModChoice = fairGenerator.Generate(0, diceSets[0].Count - 1);
            hmac = fairGenerator.GenerateHMAC(computerModChoice);
            Console.WriteLine($"I selected a random value in the range 0..{diceSets[0].Count - 1} (HMAC={hmac}).");

            Console.WriteLine($"Add your number modulo {diceSets[0].Count}.");
            for (int i = 0; i < diceSets[0].Count; i++)
                Console.WriteLine($"{i} - {i}");
            Console.WriteLine("X - exit");
            Console.WriteLine("? - help");

            Console.Write("Your selection: ");
            input = Console.ReadLine();
            if (!int.TryParse(input, out userMod) || userMod < 0 || userMod >= diceSets[0].Count) return;

            Console.WriteLine($"My number is {computerModChoice} (KEY={fairGenerator.Key}).");
            resultMod = (computerModChoice + userMod) % diceSets[0].Count;
            Console.WriteLine($"The result is {computerModChoice} + {userMod} = {resultMod} (mod {diceSets[0].Count}).");

            int userRoll = diceSets[userDiceIndex][resultMod];
            Console.WriteLine($"Your throw is {userRoll}.");

            if (userRoll > computerRoll)
                Console.WriteLine($"You win ({userRoll} > {computerRoll})!");
            else if (userRoll < computerRoll)
                Console.WriteLine($"You lose ({userRoll} < {computerRoll})!");
            else
                Console.WriteLine("It's a draw!");
        }

        static void ShowHelp(List<List<int>> diceSets)
        {
            Console.WriteLine("\nGame Rules:");
            Console.WriteLine("1. Each player selects a dice from the available sets");
            Console.WriteLine("2. The dice selection and throws use HMAC commitment to prevent cheating");
            Console.WriteLine("3. Each throw is determined by combining random numbers from both players");
            Console.WriteLine("4. The higher number wins\n");

            Console.WriteLine("Win Probability Matrix:");
            Console.WriteLine("(Shows probability of row beating column)");

            Console.Write("     ");
            for (int i = 0; i < diceSets.Count; i++)
                Console.Write($"Dice{i}  ");
            Console.WriteLine();

            for (int i = 0; i < diceSets.Count; i++)
            {
                Console.Write($"Dice{i} ");
                for (int j = 0; j < diceSets.Count; j++)
                {
                    if (i == j)
                        Console.Write("  -    ");
                    else
                    {
                        double prob = CalculateWinProbability(diceSets[i], diceSets[j]);
                        Console.Write($" {prob:F3}  ");
                    }
                }
                Console.WriteLine();
            }
        }

        static double CalculateWinProbability(List<int> dice1, List<int> dice2)
        {
            int wins = 0;
            int total = dice1.Count * dice2.Count;

            for (int i = 0; i < dice1.Count; i++)
                for (int j = 0; j < dice2.Count; j++)
                    if (dice1[i] > dice2[j])
                        wins++;

            return (double)wins / total;
        }

        static bool ValidateArgs(string[] args, out List<List<int>> diceSets)
        {
            diceSets = new List<List<int>>();
            if (args.Length < 3) return false;

            int firstLength = -1;
            foreach (string arg in args)
            {
                string[] parts = arg.Split(',');
                if (!parts.All(p => int.TryParse(p, out _)))
                    return false;

                var diceSet = parts.Select(int.Parse).ToList();

                if (firstLength == -1)
                    firstLength = diceSet.Count;
                else if (diceSet.Count != firstLength)
                    return false;

                diceSets.Add(diceSet);
            }

            return true;
        }
    }

    class FairRandomGenerator
    {
        private HMACSHA256 hmac;
        public string Key { get; private set; }

        public FairRandomGenerator()
        {
            GenerateNewKey();
        }

        private void GenerateNewKey()
        {
            byte[] keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            Key = Convert.ToHexString(keyBytes);
            hmac = new HMACSHA256(keyBytes);
        }

        public int Generate(int min, int max)
        {
            if (min > max) throw new ArgumentException("min must be less than or equal to max");

            int range = max - min + 1;
            byte[] randomBytes = new byte[4];
            int randomValue;
            do
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }
                randomValue = BitConverter.ToInt32(randomBytes, 0) & int.MaxValue;
            } while (randomValue >= range * (int.MaxValue / range));

            GenerateNewKey();
            return min + (randomValue % range);
        }

        public string GenerateHMAC(int value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value.ToString());
            byte[] hash = hmac.ComputeHash(valueBytes);
            return Convert.ToHexString(hash);
        }
    }
}
