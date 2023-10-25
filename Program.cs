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
            return Helpers.IntPrompt(() => Helpers.Print(string.Format("{0}'s turn (1, 2, or 3): ", base.Name)));
        }
    }

    public class AIPlayer : Player
    {
        public AIPlayer(string name) : base(name) { }

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
            this.turn = turn;
            this.matchSticks = matchSticks;
            this.players = players;

            Play();
        }

        private void Play()
        {
            while (true)
            {
                this.turn++;
                int playerMatchSticks;
                Player player = this.players[this.turn % this.players.Length];
                do
                {
                    playerMatchSticks = player.GetMatchSticks(this.matchSticks);
                } while (playerMatchSticks <= 0 || playerMatchSticks > 3 || playerMatchSticks > this.matchSticks);

                Helpers.Print(string.Format("{0} played {1}\n", player.Name, playerMatchSticks));
                this.matchSticks -= playerMatchSticks;

                Helpers.Print(string.Format("There are {0} matches remaining\n", this.matchSticks), ConsoleColor.Green);

                if (this.matchSticks == 0)
                {
                    Helpers.Print(string.Format("{0} Lost!", player.Name), ConsoleColor.Red);
                    break;
                }
            }
        }
    }

    internal class Program
    {
        static int startingMatchSticks = 12;

        static void LoadGame()
        {

        }
        static void SinglePlayerGame()
        {
            Game game = new Game(0,
                startingMatchSticks,
                new Player[] { new AIPlayer("Computer"), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 1 Name: "))) }
            );
        }
        static void TwoPlayerGame()
        {
            Game game = new Game(0,
                            startingMatchSticks,
                            new Player[] { new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 1 Name: "))), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 2 Name: "))) }
                        );
        }
        static void ThreePlayerGame()
        {
            Game game = new Game(0,
                            startingMatchSticks,
                            new Player[] { new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 1 Name: "))), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 2 Name: "))), new HumanPlayer(Helpers.StringPrompt(() => Helpers.Print("Player 3 Name: "))) }
                        );

        }
        static void OptionsPage()
        {
            startingMatchSticks = Helpers.IntPrompt(() => Helpers.Print("Enter the starting matchsticks: "));
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
