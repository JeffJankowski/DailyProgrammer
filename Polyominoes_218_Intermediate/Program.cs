using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyominoes_218_Intermediate
{
    class PolyominoGenerator
    {
        static void Main(string[] args)
        {
            int _order;
            do
                Console.Write("Enter polyomino order: ");
            while (!Int32.TryParse(Console.ReadLine(), out _order));
            Console.WriteLine();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Dictionary<int, Polyomino[]> _dict = new Dictionary<int, Polyomino[]>(_order);
            _dict.Add(0, new Polyomino[0]);
            _dict.Add(1, new Polyomino[] { new Polyomino(new Point[] { Point.Empty }) });
            for (int n = 2; n <= _order; n++)
            {
                ConcurrentDictionary<Polyomino, byte> possibles = new ConcurrentDictionary<Polyomino, byte>();
                Parallel.ForEach(_dict[n - 1], pl =>
                {
                    foreach (Point p in pl.GetAvailablePoints())
                        possibles.GetOrAdd(new Polyomino(pl, p), 0);
                });

                _dict.Add(n, possibles.Keys.ToArray());
            }

            foreach (Polyomino poly in _dict[_order])
                Console.WriteLine(poly.ToString());

            stopwatch.Stop();
            Console.WriteLine("--------------------------------------\nCount: {0} in {1:0.0000} seconds", _dict[_order].Length, stopwatch.ElapsedMilliseconds / 1000.0);

            Console.ReadKey();
        }
    }

    static class PointHelper
    {
        public static Point[] GetCardinals(this Point p)
        {
            return new Point[] { new Point(p.X, p.Y + 1), new Point(p.X + 1, p.Y), 
                new Point(p.X, p.Y - 1), new Point(p.X - 1, p.Y) };
        }

        public static Point[] SortPoints(this Point[] points) {
            return points.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray(); 
        }
    }

    class Polyomino
    {
        public Point[] Points { get; private set; }
        public Point[] SortedPoints { get { return Points.SortPoints(); } }
        public int Order { get { return Points.Length; } }
        private Point[][] AllConfigs
        {
            get
            {
                Polyomino rot90 = Polyomino.Rotate90Deg(this), rot180 = Polyomino.Rotate90Deg(rot90),
                        rot270 = Polyomino.Rotate90Deg(rot180), flip = Polyomino.Flip(this),
                        flip90 = Polyomino.Rotate90Deg(flip), flip180 = Polyomino.Rotate90Deg(flip90),
                        flip270 = Polyomino.Rotate90Deg(flip180);

                return new Point[][]{ this.SortedPoints, rot90.SortedPoints, rot180.SortedPoints, 
                    rot270.SortedPoints, flip.SortedPoints, flip90.SortedPoints, 
                    flip180.SortedPoints, flip270.SortedPoints };
            }
        }

        public Polyomino(Point[] points)
        {
            this.Points = points;
            translateToOrigin();
        }

        public Polyomino(Polyomino poly, Point p)
        {
            Point[] points = new Point[poly.Order + 1];
            poly.Points.CopyTo(points, 0);
            points[poly.Order] = p;

            this.Points = points;
            translateToOrigin();
        }

        public Point[] GetAvailablePoints()
        {
            return Points.SelectMany(p => p.GetCardinals()).Distinct().Where(p => !Points.Contains(p)).ToArray();
        }

        public override int GetHashCode()
        {
            Point[] allsorted = AllConfigs.SelectMany(pa => pa.Select(p => p)).ToArray().SortPoints();

            int hc = allsorted.Length;
            foreach (Point p in allsorted)
                hc = unchecked(hc * 31 + p.GetHashCode());
            return hc;
        }

        public override bool Equals(object obj)
        {
            foreach (Point[] config in this.AllConfigs)
            {
                if (config.SequenceEqual(((Polyomino)obj).SortedPoints))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            bool[,] grid = new bool[Order, Order];
            foreach (Point p in Points)
                grid[p.X, p.Y] = true;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Order; i++)
            {
                bool addLine = false;
                for (int j = 0; j < Order; j++)
                {
                    if (grid[i, j])
                    {
                        sb.Append("#");
                        addLine  = true;
                    }
                    else
                        sb.Append(" ");
                }
                if (addLine)
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        private void translateToOrigin()
        {
            int offX = -Points.Select(p => p.X).Min();
            int offY = -Points.Select(p => p.Y).Min();

            for (int i = 0; i < Points.Length; i++)
                Points[i].Offset(offX, offY);
        }

        public static Polyomino Rotate90Deg(Polyomino poly)
        {
            return new Polyomino(poly.Points.Select(p => new Point(-p.Y, p.X)).ToArray());
        }

        public static Polyomino Flip(Polyomino poly)
        {
            return new Polyomino(poly.Points.Select(p => new Point(-p.X, p.Y)).ToArray());
        }
    }
}
