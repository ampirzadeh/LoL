using System;
using System.Collections.Generic;
using System.Linq;

namespace Last_One_Loses
{
    public class Player
    {
        public string Name { get; set; }
        public virtual int GetMatchSticks(int remainingMatchSticks)
        {
            // Standard code
            return 0;
        }

        public Player(string name)
        {
            Name = name;
        }
    }
    public class HumanPlayer : Player
    {
        public HumanPlayer(string name) : base(name)
        { }

        public override int GetMatchSticks(int remainingMatchSticks)
        {
            int playerMatchSticks;
            do
            {
                playerMatchSticks = Helpers.IntPrompt(() => Helpers.Print(string.Format("{0}'s turn (1, 2, or 3): ", base.Name)));
            } while (playerMatchSticks <= 0 || playerMatchSticks > 3 || playerMatchSticks > remainingMatchSticks);

            return playerMatchSticks;
        }
    }

    public class HardAIPlayer : Player
    {
        public HardAIPlayer(string name) : base(name) { }

        public override int GetMatchSticks(int remainingMatchSticks)
        {
            return (remainingMatchSticks % 4) switch
            {
                0 => 3,
                2 => 1,
                3 => 2,
                _ => 1,
            };
        }
    }

    public class EasyAIPlayer : Player
    {
        public EasyAIPlayer(string name) : base(name) { }

        public override int GetMatchSticks(int remainingMatchSticks)
        {
            return 1;
        }
    }

    public class ChaoticAIPlayer : Player
    {
        public ChaoticAIPlayer(string name) : base(name) { }

        public override int GetMatchSticks(int remainingMatchSticks)
        {
            Random random = new();
            return random.Next(1, int.Min(4, remainingMatchSticks + 1));
        }
    }

    class Helpers
    {
        static public void Print(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
        static public int IntPrompt(Action PromptMessage)
        {
            int result;
            do
            {
                PromptMessage();
            } while (!int.TryParse(Console.ReadLine(), out result));

            return result;
        }
        static public string StringPrompt(Action PromptMessage)
        {
            string result;
            do
            {
                PromptMessage();
            } while ((result = Console.ReadLine()) == "");

            return result;
        }
    }

    public class Game
    {
        int turn;
        int matchSticks;
        private Player[] players;

        public Game(int turn, int matchSticks, Player[] players)
        {
            this.players = players;
            this.matchSticks = matchSticks;
            if (turn == -1)
                this.turn = DecideTurn();
            else
                this.turn = turn;
        }

        private int DecideTurn()
        {
            Random random = new Random();

            if (players.Length == 3)
            {
                return random.Next(0, 3);
            }

            string coinChoice;
            do
            {
                coinChoice = Helpers.StringPrompt(() => Helpers.Print(string.Format("{0}, choose 'heads' or 'tails'? ", players[1].Name)));
            } while (coinChoice != "heads" && coinChoice != "tails");

            int coinOutcome = random.Next(0, 2);
            Helpers.Print(string.Format("The coin flipped {0}\n", coinOutcome == 0 ? "heads" : "tails"), ConsoleColor.Blue);

            if ((coinOutcome == 0 && coinChoice == "heads") || (coinOutcome == 1 && coinChoice == "tails")) // guessed correctly
                return 1;

            return 0;
        }

        private void PrintMatchsticks(int nOfMatchSticks)
        {
            for (int rows = 0; rows < 4; rows++)
            {
                for (int column = 0; column < nOfMatchSticks; column++)
                {
                    if (rows == 0) Helpers.Print("0 ", ConsoleColor.Red);
                    else Helpers.Print("| ", ConsoleColor.Yellow);
                }
                Helpers.Print("\n");
            }
        }

        public Player Play()
        {
            while (true)
            {
                Helpers.Print(string.Format("There are {0} matches remaining\n", this.matchSticks), ConsoleColor.Green);
                PrintMatchsticks(this.matchSticks);

                Player player = this.players[this.turn % this.players.Length];

                int playerMatchSticks = player.GetMatchSticks(this.matchSticks);
                Helpers.Print(string.Format("{0} played {1}\n", player.Name, playerMatchSticks));
                this.matchSticks -= playerMatchSticks;

                if (this.matchSticks == 0)
                {
                    Helpers.Print(string.Format("{0} Lost!\n", player.Name), ConsoleColor.Red);
                    return player;
                }
                this.turn++;
            }
        }
    }

    internal class Program
    {
        static int startingMatchSticks = 12;
        static int playBestOutOf = 1;
        static int aiDifficulty = 2;

        static void LoadGame()
        {

        }

        static void Play(Player[] players)
        {
            Dictionary<Player, int> playerLosses = new Dictionary<Player, int>();
            foreach (Player player in players)
            {
                playerLosses[player] = 0;
            }

            for (int i = 0; i < playBestOutOf; i++)
            {
                Game game = new Game(-1,
                    startingMatchSticks, players
                );
                playerLosses[game.Play()]++;
            }


            if (playBestOutOf > 1)
            {
                Dictionary<Player, int> Leaderboard = playerLosses.OrderBy(p => p.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                Helpers.Print($"{Leaderboard.First().Key.Name} won!\n", ConsoleColor.Magenta);
                Helpers.Print("Details: \n", ConsoleColor.Gray);
                foreach (var p in Leaderboard)
                {
                    Helpers.Print($"{p.Key.Name} lost {p.Value} times\n", ConsoleColor.Gray);
                }
            }
        }

        static Player ChooseAI() {
            return aiDifficulty switch
            {
                0 => new ChaoticAIPlayer("Chaotic AI"), 
                1 => new EasyAIPlayer("Easy AI"),
                _ => new HardAIPlayer("Hard AI"),
            };
        }
        
        static void SinglePlayerGame()
        {
            Play(new Player[] { ChooseAI(), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 1 Name: "))) });
        }
        static void TwoPlayerGame()
        {
            Play(new Player[] { new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 1 Name: "))), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 2 Name: "))) });
        }
        static void ThreePlayerGame()
        {
            Play(new Player[] { new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 1 Name: "))), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 2 Name: "))), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 3 Name: "))) });
        }
        static void OptionsPage()
        {
            startingMatchSticks = Helpers.IntPrompt(() => Helpers.Print("Enter the starting matchsticks: "));
            playBestOutOf = Helpers.IntPrompt(() => Helpers.Print("Play best out of: "));
            aiDifficulty = Helpers.IntPrompt(() => Helpers.Print("AI Difficulty: (0: chaotic, 1: easy, 2: hard)"));
            ShowMenu();
        }

        static void ShowMenu()
        {
            var menu = new Dictionary<string, Action> {
                { "Load game", LoadGame },
                { "Single player", SinglePlayerGame },
                { "Two player", TwoPlayerGame },
                { "Three player", ThreePlayerGame },
                { "Options", OptionsPage }
            };

            int chosenMenuOption = 0;
            string menuOptionChoiceInput = "";

            while (menuOptionChoiceInput != "Enter")
            {
                Console.Clear();
                for (int i = 0; i < menu.Count; i++)
                {
                    if (i == chosenMenuOption)
                    {
                        Helpers.Print(menu.ElementAt(i).Key + "\n", ConsoleColor.Blue);
                    }
                    else
                    {
                        Helpers.Print(menu.ElementAt(i).Key + "\n");
                    }
                }

                menuOptionChoiceInput = Console.ReadKey().Key.ToString();
                switch (menuOptionChoiceInput)
                {
                    case "DownArrow":
                        if (chosenMenuOption + 1 < menu.Count)
                        {
                            // Go to next option
                            chosenMenuOption++;
                        }
                        else
                        {
                            // If at the end, go back to the first option
                            chosenMenuOption = 0;
                        }
                        break;
                    case "UpArrow":
                        if (chosenMenuOption - 1 >= 0)
                        {
                            // Go to previous option
                            chosenMenuOption--;
                        }
                        else
                        {
                            // If at the start, go to the last option
                            chosenMenuOption = menu.Count - 1;
                        }
                        break;
                    default:
                        break;
                }
            }
            menu.ElementAt(chosenMenuOption).Value();
        }
        static void Main(string[] args)
        {
            ShowMenu();
            Console.ReadKey();
        }
    }
}
