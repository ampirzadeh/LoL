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

    public class AIPlayer : Player
    {
        public AIPlayer(string name) : base(name)
        { }
    }

    public class HardAIPlayer : AIPlayer
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

    public class EasyAIPlayer : AIPlayer
    {
        public EasyAIPlayer(string name) : base(name) { }

        public override int GetMatchSticks(int remainingMatchSticks)
        {
            return 1;
        }
    }

    public class ChaoticAIPlayer : AIPlayer
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

        public Game(int turn, int matchSticks, Player[] players, bool writeToFile = true)
        {
            this.players = players;
            this.matchSticks = matchSticks;
            if (turn == -1)
                this.turn = DecideTurn();
            else
                this.turn = turn;

            if (writeToFile)
            {
                try
                {
                    string filePath = "runninggame.dat";
                    using FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                    using BinaryWriter writer = new BinaryWriter(fileStream);

                    Helpers.Print($"turn {this.turn}\n", ConsoleColor.Magenta);

                    writer.Write(this.turn);

                    fileStream.Close();
                    writer.Close();

                    Console.WriteLine("Data written to binary file. 1");
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred 4: " + ex.Message);
                }
            }
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

        public void PlayTurn(int playerMatchSticks)
        {
            this.matchSticks -= playerMatchSticks;
            this.turn++;
        }

        public Player Play()
        {
            while (true)
            {
                Player player = this.players[this.turn % this.players.Length];

                if (this.matchSticks == 0)
                {
                    Helpers.Print(string.Format("{0} Lost!\n", players[(this.turn - 1) % this.players.Length].Name), ConsoleColor.Red);
                    return players[(this.turn - 1) % this.players.Length];
                }

                Helpers.Print(string.Format("There are {0} matches remaining\n", this.matchSticks), ConsoleColor.Green);
                PrintMatchsticks(this.matchSticks);

                int playerMatchSticks = player.GetMatchSticks(this.matchSticks);

                Helpers.Print(string.Format("{0} played {1}\n", player.Name, playerMatchSticks));
                try
                {
                    string filePath = "runninggame.dat";
                    using FileStream fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                    using BinaryWriter writer = new BinaryWriter(fileStream);

                    writer.Write(playerMatchSticks);
                    Console.WriteLine(playerMatchSticks);

                    fileStream.Close();
                    writer.Close();

                    Console.WriteLine("Data written to binary file. 2");
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred 1: " + ex.Message);
                }
                PlayTurn(playerMatchSticks);
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
            string filePath = "runninggame.dat";

            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new BinaryReader(fileStream);

                int nOfPlayers = reader.ReadInt32();
                playBestOutOf = reader.ReadInt32();
                startingMatchSticks = reader.ReadInt32();
                aiDifficulty = reader.ReadInt32();

                Helpers.Print($"nOfPlayers {nOfPlayers}\n", ConsoleColor.Magenta);
                Helpers.Print($"playBestOutOf {playBestOutOf}\n", ConsoleColor.Magenta);
                Helpers.Print($"startingMatchSticks {startingMatchSticks}\n", ConsoleColor.Magenta);
                Helpers.Print($"aiDifficulty {aiDifficulty}\n", ConsoleColor.Magenta);


                Game game;
                Player[] players = new Player[nOfPlayers];

                for (int i = 0; i < nOfPlayers; i++)
                {
                    players[i] = new HumanPlayer(reader.ReadString());

                    if (aiDifficulty != -1 && i == 0)
                        players[i] = ChooseAI(aiDifficulty);

                    Helpers.Print($"Name {i} {players[i].Name}\n", ConsoleColor.Magenta);
                }


                Dictionary<Player, int> playerLosses = new Dictionary<Player, int>();
                foreach (Player player in players)
                {
                    playerLosses[player] = 0;
                }


                int playedMatchsticksSum = 0, turn = -1, nOfSavedGames = 0;

                while (reader.PeekChar() != -1)
                {
                    int data = reader.ReadInt32();
                    if (turn == -1)
                    {
                        turn = data;
                        continue;
                    }

                    if (playedMatchsticksSum + data == startingMatchSticks)
                    {
                        playerLosses[players[turn % players.Length]]++;
                        playedMatchsticksSum = 0;
                        turn = -1;
                        nOfSavedGames++;
                    }
                    else
                    {
                        playedMatchsticksSum += data;
                        turn++;
                    }
                }
                reader.Close();

                foreach (var p in playerLosses)
                {
                    Helpers.Print($"{p.Key.Name} lost {p.Value} times\n", ConsoleColor.Gray);
                }

                game = new Game(turn, startingMatchSticks - playedMatchsticksSum, players, false);
                playerLosses[game.Play()]++;

                for (int i = 0; i < playBestOutOf - nOfSavedGames - 1; i++)
                {
                    game = new Game(-1,
                        startingMatchSticks, players, true
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
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found.");
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred 2: " + ex.Message);
            }
        }

        static void Play(Player[] players)
        {
            string filePath = "runninggame.dat";

            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                using BinaryWriter writer = new BinaryWriter(fileStream);

                writer.Write(players.Length);
                writer.Write(playBestOutOf);
                writer.Write(startingMatchSticks);
                Console.WriteLine((players[0] is AIPlayer) ? aiDifficulty : -1);
                writer.Write(players[0] is AIPlayer ? aiDifficulty : -1);

                foreach (Player player in players)
                {
                    writer.Write(player.Name);
                }

                fileStream.Close();
                writer.Close();

                Console.WriteLine("Data written to binary file. 3");
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred 3: " + ex.Message);
            }


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

        static Player ChooseAI(int aiDifficulty)
        {
            return aiDifficulty switch
            {
                0 => new ChaoticAIPlayer("Chaotic AI"),
                1 => new EasyAIPlayer("Easy AI"),
                _ => new HardAIPlayer("Hard AI"),
            };
        }

        static void SinglePlayerGame()
        {
            Play(new Player[] { ChooseAI(aiDifficulty), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 1 Name: "))) });
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
