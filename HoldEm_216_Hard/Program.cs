using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DailyProgrammer
{
    class HoldEm_216_Hard
    {
        public static void Main(string[] args)
        {
            int numPlayers, numGames;
            do
            {
                Console.Write("\nHow many players (2-8)? ");
            } while (!int.TryParse(Console.ReadKey().KeyChar.ToString(), out numPlayers) || numPlayers < 2 || numPlayers > 8);

            do
            {
                Console.Write("\nHow many games (1-100,000)? ");
            } while (!int.TryParse(Console.ReadLine(), out numGames) || numGames < 1 || numPlayers > 100000);

            Console.WriteLine('\n');

            Player[] players = new Player[numPlayers];
            players[0] = new Player("You");
            for (int i = 1; i < numPlayers; i++)
                players[i] = new Player(String.Format("CPU {0}", i));


            Stats stats = new Stats(players);
            string numFormat = "{0:" + String.Join("", Enumerable.Repeat(0, (int)Math.Log10((double)numGames) + 1).ToArray()) + "}";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 1; i <= numGames; i++)
            {
                new HoldEm(players, ref stats).Play();
                Console.Write("\rPlaying: " + numFormat + "/{1}  @ {2} games/sec", i, numGames, (int)((double)i / stopwatch.Elapsed.TotalSeconds));
            }
            Console.Write("\r                                                                  \r"); //clear line out
            Console.WriteLine(stats.ToString());
            Console.ReadKey();
        }
    }

    public class Stats
    {
        public int TotalGames { get; private set; }
        public int BadFolds { get; private set; }
        public Dictionary<Player, int> WinCount { get; private set; }
        public List<HandType> WinHands { get; private set; }

        public Stats(Player[] players)
        {
            BadFolds = 0;
            TotalGames = 0;
            WinCount = new Dictionary<Player, int>();
            WinHands = new List<HandType>();

            foreach (Player p in players)
                WinCount.Add(p, 0);
        }

        public void AddResult(Player[] winners, HandType handType, bool badFold)
        {
            foreach (Player p in winners)
                WinCount[p]++;

            WinHands.Add(handType);

            if (badFold)
                BadFolds++;
            TotalGames++;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Total games:\t{0}\n", TotalGames);
            sb.AppendFormat("Bad folds:\t{0} ({1:0.00%})\n\n", BadFolds, (double)BadFolds / (double)TotalGames);
            sb.AppendLine("     PLAYER          WIN-LOSS          WIN %");
            sb.AppendLine("---------------+------------------+--------------");
            foreach (var kv in WinCount)
                sb.AppendFormat(" {0,-14}| {1,-17}| {2:00.00%}\n", kv.Key.Name, String.Format("{0}-{1}", kv.Value, TotalGames - kv.Value), (double)kv.Value / (double)TotalGames);
            sb.AppendLine("\n");
            sb.AppendLine("       HAND             WIN %");
            sb.AppendLine("-------------------+--------------");
            var winTypes = WinHands.GroupBy(t => t).OrderBy(grp => (int)grp.Key).Select(grp => new { type = grp.Key, count = grp.Count() });
            foreach (var obj in winTypes)
                sb.AppendFormat(" {0, -18}| {1:00.00%}\n", Hand.StringFromType(obj.type), (double)obj.count / (double)TotalGames);
            return sb.ToString();
        }
    }

    public class HoldEm
    {
        Random _rand = new Random();

        private Player[] _players;
        private Deck _deck;
        private Card[] _commonCards;
        private Stats _stats;

        public HoldEm(Player[] players, ref Stats stats)
        {
            _commonCards = new Card[5];
            _deck = new Deck();
            _players = players;
            _stats = stats;
        }

        public void Play()
        {
            deal();

            _deck.BurnCard();
            flop();
            checkFolds();

            _deck.BurnCard();
            turn();

            _deck.BurnCard();
            river();
            checkWinner();
        }

        private void deal()
        {
            foreach (Player p in _players)
            {
                p.Cards[0] = _deck.DrawCard();
                p.Cards[1] = _deck.DrawCard();
                //Console.WriteLine(p);
            }
            //Console.WriteLine();
        }

        private void flop()
        {
            _commonCards[0] = _deck.DrawCard();
            _commonCards[1] = _deck.DrawCard();
            _commonCards[2] = _deck.DrawCard();
            //Console.WriteLine("Flop:\t{0}  {1}  {2}", _commonCards[0], _commonCards[1], _commonCards[2]);
        }

        private void turn()
        {
            _commonCards[3] = _deck.DrawCard();
            //Console.WriteLine("Turn:\t{0}", _commonCards[3]);
        }

        private void river()
        {
            _commonCards[4] = _deck.DrawCard();
            //Console.WriteLine("River:\t{0}", _commonCards[4]);
        }

        private void checkFolds()
        {
            foreach (Player player in _players)
            {
                var visibleCards = _commonCards.Take(3).Concat(player.Cards);
                Hand hand = HandCalculator.GetBestHand(visibleCards.ToArray());
                if (HandCalculator.IsGoodHand(hand))
                    continue;

                int outs = 0;
                foreach (Card card in Deck.FullDeckOfCards().Except(visibleCards))
                {
                    Hand newHand = HandCalculator.GetBestHand(visibleCards.Concat(new Card[] { card }).ToArray());
                    if (HandCalculator.IsGoodHand(newHand))
                        outs++;
                }

                double prob = (93 * outs - outs * outs) / 2162.0;
                //fold if probability of making a hand by river is >=50%
                player.Folded = prob < 0.5;
            }

            //pick random winner if everyone folded..
            if (_players.All(p => p.Folded))
                _players[_rand.Next(_players.Length)].Folded = false;
        }

        private void checkWinner()
        {
            Dictionary<Player, Hand> best = new Dictionary<Player, Hand>();
            foreach (Player player in _players)
            {
                Hand hand = HandCalculator.GetBestHand(_commonCards.Concat(player.Cards).ToArray());
                best.Add(player, hand);
            }

            Hand[] winHands = HandCalculator.GetWinners(best.Where(kv => !kv.Key.Folded).Select(kv => kv.Value).ToArray());
            Hand[] wouldWinHands = HandCalculator.GetWinners(best.Values.ToArray());
            Dictionary<Player, Hand> winners = best.Where(kv => winHands.Contains(kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value);
            Dictionary<Player, Hand> wouldBes = best.Where(kv => wouldWinHands.Contains(kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value);

            _stats.AddResult(winners.Keys.ToArray(), winHands[0].Type, !winners.SequenceEqual(wouldBes));
        }
    }

    public class Hand
    {
        public Card[] UsedCards { get; private set; }
        public Card[] Kickers { get; private set; }
        public HandType Type { get; private set; }
        public string TypeString { get { return Hand.StringFromType(Type); } }

        public Hand(HandType type, Card[] cards, Card[] kickers)
        {
            UsedCards = cards;
            Kickers = kickers;
            Type = type;
        }

        public static string StringFromType(HandType type)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(
                  Enum.GetName(typeof(HandType), type).Replace('_', ' ').ToLower());
        }
    }

    public enum HandType
    {
        HIGH_CARD = 0,
        ONE_PAIR = 1,
        TWO_PAIR = 2,
        THREE_OF_A_KIND = 3,
        STRAIGHT = 4,
        FLUSH = 5,
        FULL_HOUSE = 6,
        FOUR_OF_A_KIND = 7,
        STRAIGHT_FLUSH = 8
    }

    public static class HandCalculator
    {
        private const int CHAR_OFFSET = 95; //'a'-2 ====> 2+offset='a' , 3+offset='b', ... , K+offset='l', A+offset='m'

        public static Hand[] GetWinners(Hand[] hands)
        {
            Hand[] contenders = hands.GroupBy(h => h.Type).OrderByDescending(grp => (int)grp.Key).First().ToArray();
            Dictionary<Hand, string> encodings = new Dictionary<Hand, string>();
            foreach (Hand hand in contenders)
            {
                string encode = String.Join("", hand.UsedCards.Concat(hand.Kickers ?? new Card[0]).Select(h => (char)(h.Value + CHAR_OFFSET)));
                encodings.Add(hand, encode);
            }

            return encodings.GroupBy(kv => kv.Value).OrderByDescending(grp => grp.Key).First().Select(kv => kv.Key).ToArray();
        }

        public static bool IsGoodHand(Hand hand)
        {
            //two pair or single face pair
            return (int)hand.Type >= (int)HandType.TWO_PAIR || (hand.Type == HandType.ONE_PAIR && hand.UsedCards[0].Value > 10);
        }

        public static Hand GetBestHand(Card[] cards)
        {
            Card[] used;
            var valGroups = cards.GroupBy(c => c.Value).OrderByDescending(grp => grp.Count()).ThenByDescending(grp => grp.Key);
            var suitGroups = cards.GroupBy(c => c.Suit).OrderByDescending(grp => grp.Count());

            int nOfAKind = valGroups.First().Count();
            bool flush = suitGroups.First().Count() >= 5;

            //straight flush
            if (flush)
            {
                var straightFlush = suitGroups.First().OrderByDescending(c => c.Value).ToConsecutiveGroups((c1, c2) => c2.Value == c1.Value - 1)
                    .OrderByDescending(list => list.Count).FirstOrDefault(list => list.Count >= 5);
                if (straightFlush != null)
                    return new Hand(HandType.STRAIGHT_FLUSH, straightFlush.OrderByDescending(c => c.Value).Take(5).ToArray(), null);
            }

            //4-of-a-kind
            if (nOfAKind == 4)
            {
                used = valGroups.First().ToArray();
                return new Hand(HandType.FOUR_OF_A_KIND, used, getSortedKickers(cards, used));
            }

            //full house
            var pairGroup = valGroups.FirstOrDefault(grp => grp.Count() == 2);
            if (nOfAKind == 3 && pairGroup != null)
                return new Hand(HandType.FULL_HOUSE, valGroups.First().Concat(pairGroup).ToArray(), null);

            //flush
            if (flush)
                return new Hand(HandType.FLUSH, suitGroups.First().OrderByDescending(c => c.Value).Take(5).ToArray(), null);

            //straight
            var straight = valGroups.Select(grp => grp.First()).OrderByDescending(c => c.Value).ToConsecutiveGroups((c1, c2) => c2.Value == c1.Value - 1)
                .OrderByDescending(list => list.Count).FirstOrDefault(list => list.Count >= 5);
            if (straight != null)
                return new Hand(HandType.STRAIGHT, straight.OrderByDescending(c => c.Value).Take(5).ToArray(), null);

            //3-of-a-kind
            if (nOfAKind == 3)
            {
                used = valGroups.First().ToArray();
                return new Hand(HandType.THREE_OF_A_KIND, used, getSortedKickers(cards, used));
            }

            if (nOfAKind == 2)
            {
                var extraPair = valGroups.Skip(1).FirstOrDefault(grp => grp.Count() == 2);
                if (extraPair != null)
                {
                    //two pair
                    used = valGroups.First().Concat(extraPair).ToArray();
                    return new Hand(HandType.TWO_PAIR, used, getSortedKickers(cards, used));
                }
                else
                {
                    //one pair
                    used = valGroups.First().ToArray();
                    return new Hand(HandType.ONE_PAIR, used, getSortedKickers(cards, used));
                }
            }

            //high card
            used = valGroups.First().ToArray();
            return new Hand(HandType.HIGH_CARD, used, getSortedKickers(cards, used));
        }

        private static Card[] getSortedKickers(Card[] allCards, Card[] usedCards)
        {
            return allCards.Except(usedCards).OrderByDescending(c => c.Value).Take(5 - usedCards.Length).ToArray();
        }

        //extension method taken from: http://sagarkhyaju.blogspot.com/2014/01/split-array-with-linq-to-group.html
        public static IEnumerable<List<T>> ToConsecutiveGroups<T>(this IEnumerable<T> source, Func<T, T, bool> isConsequtive)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    yield break;
                else
                {
                    T current = iterator.Current;
                    List<T> group = new List<T> { current };

                    while (iterator.MoveNext())
                    {
                        T next = iterator.Current;
                        if (!isConsequtive(current, next))
                        {
                            yield return group;
                            group = new List<T>();
                        }

                        current = next;
                        group.Add(current);
                    }

                    if (group.Any())
                        yield return group;
                }
            }
        }
    }


    public class Player
    {
        public string Name { get; private set; }
        public Card[] Cards { get; private set; }
        public bool Folded { get; set; }

        public Player(string name)
        {
            this.Name = name;
            Cards = new Card[2];
            Folded = false;
        }

        public override string ToString()
        {
            return string.Format("{0} Hand:\t{1}  {2}", Name == "You" ? "Your" : Name, Cards[0], Cards[1]);
        }
    }

    public class Deck
    {
        private Card[] _deck;
        private int _cardsLeft;

        public Deck()
        {
            _deck = Deck.FullDeckOfCards();
            _cardsLeft = _deck.Length;
            Shuffle();
        }

        public void Shuffle()
        {
            Deck.shuffle(_deck);
        }

        public Card DrawCard()
        {
            Card c = _deck[_cardsLeft - 1];
            _deck[--_cardsLeft] = null;
            return c;
        }

        public void BurnCard()
        {
            _deck[--_cardsLeft] = null;
        }

        public static Card[] FullDeckOfCards()
        {
            return Enumerable.Range(0, 52).Select(n => new Card((n % 13) + 2, (CardSuit)(n / 13))).ToArray();
        }

        //dotnetpearls extension shuffle method
        private static Random _random = new Random();
        private static void shuffle<T>(T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int r = i + (int)(_random.NextDouble() * (n - i));
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }
    }

    public enum CardSuit
    {
        HEARTS = 3,
        DIAMONDS = 2,
        SPADES = 1,
        CLUBS = 0
    }

    public class Card
    {
        public int Value { get; private set; }
        public CardSuit Suit { get; private set; }

        public Card(int value, CardSuit suit)
        {
            this.Value = value;
            this.Suit = suit;
        }

        private static Dictionary<CardSuit, string> _suitToChar = new Dictionary<CardSuit, string>()
        {
            {CardSuit.HEARTS, "\u2665"},
            {CardSuit.DIAMONDS, "\u2666"},
            {CardSuit.CLUBS, "\u2663"},
            {CardSuit.SPADES, "\u2660"}
        };
        private static Dictionary<int, string> _valToChar = new Dictionary<int, string>()
        {
            {11, "J"},
            {12, "Q"},
            {13, "K"},
            {14, "A"}
        };

        public override string ToString()
        {
            return string.Format(
                "{0}{1}",
                _valToChar.ContainsKey(Value) ? _valToChar[Value] : Value.ToString(),
                _suitToChar[Suit]);
        }
    }
}