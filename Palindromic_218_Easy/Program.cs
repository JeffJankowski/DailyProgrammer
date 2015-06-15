using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Collections;

namespace DailyProgrammer
{
    static class Palindrome_218_Easy
    {
        struct Palindome
        {
            public int Num;
            public int Steps;
            public BigInteger Palindrome;
            public Palindome(int num, int steps, BigInteger palindrome)
            {
                this.Num = num;
                this.Steps = steps;
                this.Palindrome = palindrome;
            }
            public bool IsPossibleLychrel() { return this.Steps >= 10000; }
        }

        static string Reverse(this string s) { return new string(s.AsEnumerable().Reverse().ToArray()); }
        static BigInteger Reverse(this BigInteger bi) { return BigInteger.Parse(bi.ToString().Reverse()); }
        static int Reverse(this int i) { return Int32.Parse(i.ToString().Reverse()); }
        static bool IsPalindrome(this BigInteger bi)
        {
            string s = bi.ToString();
            return s.Substring(0, s.Length / 2) == s.Substring((int)(s.Length / 2.0 + 0.5)).Reverse();
        }

        static void Main(string[] args)
        {
            List<Task> jobs = new List<Task>();
            ConcurrentDictionary<int, Palindome> dict = new ConcurrentDictionary<int, Palindome>();
            for (int i = 1; i <= 1000; i++)
            {
                jobs.Add(Task.Factory.StartNew((obj) =>
                    {
                        int x = (int)obj;
                        BigInteger val = new BigInteger(x);
                        int steps = 0;
                        while (!val.IsPalindrome() && steps < 10000)
                        {
                            steps++;
                            val += val.Reverse();

                            if (dict.ContainsKey(x))
                                return;
                        }

                        Palindome p = new Palindome(x, steps, val);
                        dict.GetOrAdd(x, p);
                        int rev_x = x.Reverse();
                        if ((int)Math.Log10(rev_x) == (int)Math.Log10(x))
                        {
                            p.Num = x;
                            dict.GetOrAdd(x, p);
                        }
                    }, i));
            }

            Console.Write("Working...");
            foreach (Task t in jobs)
                t.Wait();

            using (StreamWriter sw = new StreamWriter("output.txt"))
            {
                sw.WriteLine("  NUM      STEPS            PALINDROME        ");
                sw.WriteLine("-------+-----------+--------------------------");
                sw.Write(String.Join("\n", dict.OrderBy(kv => kv.Key).Select(kv =>
                    String.Format(" {0,-6}| {1,-10}| {2}",
                        kv.Value.Num,
                        !kv.Value.IsPossibleLychrel() ?  kv.Value.Steps.ToString() : "--",
                        !kv.Value.IsPossibleLychrel() ? kv.Value.Palindrome.ToString() : "Possible Lychrel")).ToArray()));
            }
        }
    }
}