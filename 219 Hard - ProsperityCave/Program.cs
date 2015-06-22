using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace _219_Hard___ProsperityCave
{
    class ProsperityCave
    {
        static void Main(string[] args)
        {
            const string PATH = "../../../input/input_10.txt";

            int CAPACITY;
            int N;
            int[] NUGGETS;
            using (StreamReader sr = new StreamReader(PATH))
            {
                CAPACITY = double.Parse(sr.ReadLine()).Normalize();
                N = int.Parse(sr.ReadLine());
                NUGGETS = new int[N + 1];

                for (int i = 1; i <= N; i++)
                    NUGGETS[i] = double.Parse(sr.ReadLine()).Normalize();
            }
            int MIN = NUGGETS.Skip(1).Min();

            int[,] dp = new int[CAPACITY + 1,N + 1];
            Dictionary<int, int> update = new Dictionary<int, int>();
            for (int i = 1; i <= N; i++)
            {
                update.Clear();
                for (int j = MIN; j <= CAPACITY; j++)
                {
                    if (NUGGETS[i] <= j && dp.SumRow(j - NUGGETS[i], i-1) + NUGGETS[i] > dp.SumRow(j, i-1))
                        update.Add(j - NUGGETS[i], j);
                }
                foreach (var kv in update)
                {
                    dp.CopyRow(kv.Key, kv.Value, i-1);
                    dp[kv.Value, i] = NUGGETS[i];
                }
            }

            Console.WriteLine("{0:0.0000000}", dp.SumRow(CAPACITY, N).Denormalize());
            for (int i = 0; i <= N; i++)
                if (dp[CAPACITY, i] != 0)
                    Console.WriteLine("{0:0.0000000}", dp[CAPACITY, i].Denormalize());

            Console.Read();
        }
    }

    public static class Helper
    {
        const double NORMALIZE = 10000000;
        public static int Normalize(this double val) { return (int)(val * NORMALIZE + 0.5); }
        public static double Denormalize(this int val) { return Math.Round((double)val / NORMALIZE, 7); }
        public static int SumRow(this int[,] grid, int row, int n) 
        {
            int count = 0;
            for (int i = 0; i <= n; i++)
                count += grid[row, i];

            return count;
        }
        public static void CopyRow(this int[,] grid, int srcRow, int destRow, int n)
        {
            for (int i = 0; i <= n; i++)
                grid[destRow, i] = grid[srcRow, i];
        }
    }
}