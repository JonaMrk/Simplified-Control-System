using System;
using System.IO;

namespace GitLite
{
    public class DiffEngine
    {
        public string[] Compare(string fileA, string fileB)
        {
            if (!File.Exists(fileA) || !File.Exists(fileB))
            {
                return new string[] { "One or both files do not exist." };
            }

            string[] linesA = File.ReadAllLines(fileA);
            string[] linesB = File.ReadAllLines(fileB);

            int[,] lcs = BuildLcsTable(linesA, linesB);

            var result = new System.Collections.Generic.List<string>();

            BuildDiff(linesA, linesB, lcs, linesA.Length, linesB.Length, result);

            return result.ToArray();
        }

        private int[,] BuildLcsTable(string[] linesA, string[] linesB)
        {
            int m = linesA.Length;
            int n = linesB.Length;

            int[,] lcs = new int[m + 1, n + 1];

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (linesA[i - 1] == linesB[j - 1])
                    {
                        lcs[i, j] = lcs[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        lcs[i, j] = Math.Max(lcs[i - 1, j], lcs[i, j - 1]);
                    }
                }
            }

            return lcs;
        }

        private void BuildDiff(string[] linesA, string[] linesB, int[,] lcs, int i, int j, System.Collections.Generic.List<string> result)
        {
            if (i > 0 && j > 0 && linesA[i - 1] == linesB[j - 1])
            {
                BuildDiff(linesA, linesB, lcs, i - 1, j - 1, result);
                result.Add("  " + linesA[i - 1]);
            }
            else if (j > 0 && (i == 0 || lcs[i, j - 1] >= lcs[i - 1, j]))
            {
                BuildDiff(linesA, linesB, lcs, i, j - 1, result);
                result.Add("+ " + linesB[j - 1]);
            }
            else if (i > 0 && (j == 0 || lcs[i, j - 1] < lcs[i - 1, j]))
            {
                BuildDiff(linesA, linesB, lcs, i - 1, j, result);
                result.Add("- " + linesA[i - 1]);
            }
        }
    }
}