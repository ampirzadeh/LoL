using System;
using System.Collections.Generic;
using System.Linq;

/** BINARY FILE SAVING GAME DATA

number of players (including ai if it exists)
playBestOutOf
nOfMatchsticks
ai difficulty: -1 no ai, 0 chaotic,  1 easy ai, 2 hard ai
player name * n (including ai if it exists)
first player turn
matchsticks
matchsticks
...
first player turn (for second game)
matchsticks
matchsticks
...
*/
namespace Last_One_Loses
{
    public class Player
    {
        public string Name { get; set; }
        public virtual int GetMatchSticks(int remainingMatchSticks)
        {
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
                playerMatchSticks = Helpers.IntPrompt(() => Helpers.Print($"{Name}'s turn (1, 2, or 3): "));
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
            string? result = "";
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
        private readonly Player[] players;

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
                    using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write);
                    using BinaryWriter writer = new(fileStream);

                    writer.Write(this.turn);

                    fileStream.Close();
                    writer.Close();
                }
                catch (IOException ex)
                {
                    Helpers.Print($"An error occurred: {ex.Message} \n", ConsoleColor.Red);
                }
            }
        }

        private int DecideTurn()
        {
            Random random = new();

            if (players.Length == 3)
                return random.Next(0, 3);

            string coinChoice;
            do
                coinChoice = Helpers.StringPrompt(() => Helpers.Print($"{players[1].Name}, choose 'heads' or 'tails': "));
            while (coinChoice != "heads" && coinChoice != "tails");

            int coinOutcome = random.Next(0, 2);
            Helpers.Print(coinOutcome == 0 ? "The coin flipped heads\n" : "The coin flipped tails\n", ConsoleColor.Blue);

            if ((coinOutcome == 0 && coinChoice == "heads") || (coinOutcome == 1 && coinChoice == "tails")) // guessed correctly
                return 1;

            return 0;
        }

        private static void PrintMatchsticks(int nOfMatchSticks)
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
            matchSticks -= playerMatchSticks;
            turn++;
        }

        public Player Play()
        {
            while (true)
            {
                Player player = players[turn % players.Length];

                if (matchSticks == 0)
                {
                    Helpers.Print($"{players[(turn - 1) % players.Length].Name} Lost!\n", ConsoleColor.Red);
                    return players[(turn - 1) % players.Length];
                }

                Helpers.Print($"There are {matchSticks} matches remaining\n", ConsoleColor.Green);
                PrintMatchsticks(matchSticks);

                int playerMatchSticks = player.GetMatchSticks(matchSticks);

                Helpers.Print($"{player.Name} played {playerMatchSticks}\n");
                try
                {
                    string filePath = "runninggame.dat";
                    using FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write);
                    using BinaryWriter writer = new(fileStream);

                    writer.Write(playerMatchSticks);

                    fileStream.Close();
                    writer.Close();
                }
                catch (IOException ex)
                {
                    Helpers.Print($"An error occurred: {ex.Message} \n", ConsoleColor.Red);
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
                using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new(fileStream);

                int nOfPlayers = reader.ReadInt32();
                playBestOutOf = reader.ReadInt32();
                startingMatchSticks = reader.ReadInt32();
                aiDifficulty = reader.ReadInt32();

                Helpers.Print($"Loaded {nOfPlayers} player game. Play best out of {playBestOutOf}. Starting Matchsticks: {startingMatchSticks} \n", ConsoleColor.Magenta);
                Player[] players = new Player[nOfPlayers];

                for (int i = 0; i < nOfPlayers; i++)
                {
                    players[i] = new HumanPlayer(reader.ReadString());

                    if (aiDifficulty != -1 && i == 0)
                        players[i] = ChooseAI(aiDifficulty);
                }

                Dictionary<Player, int> playerLosses = new();
                foreach (Player player in players)
                    playerLosses[player] = 0;


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

                Game game = new(turn, startingMatchSticks - playedMatchsticksSum, players, false);
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
                        Helpers.Print($"{p.Key.Name} lost {p.Value} times\n", ConsoleColor.Gray);
                }
            }
            catch (FileNotFoundException)
            {
                Helpers.Print($"File not found. \n", ConsoleColor.Red);
            }
            catch (IOException ex)
            {
                Helpers.Print($"An error occurred: {ex.Message} \n", ConsoleColor.Red);
            }
        }

        static void Play(Player[] players)
        {
            string filePath = "runninggame.dat";

            try
            {
                using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write);
                using BinaryWriter writer = new(fileStream);

                writer.Write(players.Length);
                writer.Write(playBestOutOf);
                writer.Write(startingMatchSticks);
                writer.Write(players[0] is AIPlayer ? aiDifficulty : -1);

                foreach (Player player in players)
                    writer.Write(player.Name);

                fileStream.Close();
                writer.Close();
            }
            catch (IOException ex)
            {
                Helpers.Print($"An error occurred: {ex.Message}\n", ConsoleColor.Red);
            }


            Dictionary<Player, int> playerLosses = new();
            foreach (Player player in players)
                playerLosses[player] = 0;

            for (int i = 0; i < playBestOutOf; i++)
            {
                Game game = new(-1,
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
                    Helpers.Print($"{p.Key.Name} lost {p.Value} times\n", ConsoleColor.Gray);
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
            aiDifficulty = Helpers.IntPrompt(() => Helpers.Print("AI Difficulty (0: chaotic, 1: easy, 2: hard): "));
            ShowMenu(HomeMenu());
        }

        static Dictionary<string, Action> HomeMenu()
        {
            return new Dictionary<string, Action> {
                { "Load game", LoadGame },
                { "New game", () => ShowMenu(NewGameMenu()) },
                { "Options", OptionsPage }
            };
        }
        static Dictionary<string, Action> NewGameMenu()
        {
            return new Dictionary<string, Action> {
                { "Main menu", () => ShowMenu(HomeMenu()) },
                { "Single player", SinglePlayerGame },
                { "Two player", TwoPlayerGame },
                { "Three player", ThreePlayerGame }
            };
        }

        static void ShowMenu(Dictionary<string, Action> menuOptions)
        {
            int chosenMenuOption = 0;
            string menuOptionChoiceInput = "";

            while (menuOptionChoiceInput != "Enter")
            {
                Console.Clear();
                for (int i = 0; i < menuOptions.Count; i++)
                    if (i == chosenMenuOption)
                        Helpers.Print($"{menuOptions.ElementAt(i).Key}\n", ConsoleColor.Blue);
                    else
                        Helpers.Print($"{menuOptions.ElementAt(i).Key}\n");


                menuOptionChoiceInput = Console.ReadKey().Key.ToString();
                switch (menuOptionChoiceInput)
                {
                    case "DownArrow":
                        if (chosenMenuOption + 1 < menuOptions.Count)
                            // Go to next option
                            chosenMenuOption++;
                        else
                            // If at the end, go back to the first option
                            chosenMenuOption = 0;
                        break;
                    case "UpArrow":
                        if (chosenMenuOption - 1 >= 0)
                            // Go to previous option
                            chosenMenuOption--;
                        else
                            // If at the start, go to the last option
                            chosenMenuOption = menuOptions.Count - 1;
                        break;
                    default:
                        break;
                }
            }
            menuOptions.ElementAt(chosenMenuOption).Value();
        }
        static void Main(string[] args)
        {
            ShowMenu(HomeMenu());
            Console.ReadKey();
        }
    }
}
