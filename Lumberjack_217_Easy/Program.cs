using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace DailyProgrammer
{
    class Lumberjack_217_Easy
    {
        static void Main(string[] args)
        {
            string filename = "../../inputs/input_big.txt";
            string[] input = File.ReadAllLines(filename);

            int n = Int32.Parse(input[0]);
            int logs = Int32.Parse(input[1]);

            int min = Int32.MaxValue;
            IEnumerable<int> piles = null;
            for (int i = 0; i < n; i++)
            {
                var row = input[i + 2].Split(' ').Where(s => s.Trim() != String.Empty).Select(s => Int32.Parse(s));
                piles = piles == null ? row : piles.Concat(row);
                min = Math.Min(min, row.Min());
            }

            int idx = 0;
            int[] flat = piles.ToArray();
            while (logs > 0)
            {
                if (flat[idx] == min)
                {
                    logs--;
                    flat[idx]++;
                }

                if (idx+1 < n * n)
                    idx++;
                else
                {
                    idx = 0;
                    min++;
                }
            }

            StreamWriter sw = new StreamWriter("C:\\test_out.txt");
            for (int i = 0; i < n; i++)
                //Console.WriteLine(String.Join(" ", flat.Skip(i * n).Take(n).Select(j => j.ToString())));
                sw.WriteLine(String.Join(" ", flat.Skip(i * n).Take(n).Select(j => j.ToString())));
            Console.ReadKey();
        }
    }
}
