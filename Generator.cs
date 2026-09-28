using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BlitzkriegWPF
{
    static class Generator
    {
        private static int[] SortAndSplitStartPoses(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<int>();

            var uniqueNumbers = new SortedSet<int>();

            foreach (Match match in Regex.Matches(input, @"-?\d+"))
                if (int.TryParse(match.Value, out int num) && num >= 1 && num <= 99)
                    uniqueNumbers.Add(num);

            return uniqueNumbers.ToArray();
        }

        // Генератор блицкрига
        public static void GenerateBlitzkrieg(string sourceStartposes)
        {
            int[] startPoses = SortAndSplitStartPoses(sourceStartposes);

            int[] numbers = new int[startPoses.Length + 2];
            numbers[0] = 0;
            Array.Copy(startPoses, 0, numbers, 1, startPoses.Length);
            numbers[numbers.Length - 1] = 100;

            StringBuilder runsList = new StringBuilder();
            int stage = 1;
            int len = numbers.Length;
            int leftBorder = len - 2;

            while (leftBorder > 0)
            {
                runsList.Append($"Stage {stage}\n");
                int k = leftBorder;
                int j = len - 1;

                while (k >= 0)
                {
                    runsList.Append($"{numbers[k]} - {numbers[j]}\n");
                    k--;
                    j--;
                }

                leftBorder--;
                stage++;
            }

            runsList.Append($"Stage {stage}\n0 - 100");
            string[] runs = runsList.ToString().Split("\n");
            DBManager.AddBlitzkeig(new Blitzkrieg(runs, new bool[runs.Length], new int[runs.Length], new string[runs.Length]));
        }
    }
}