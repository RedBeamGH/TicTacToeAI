using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tic_tac_toe_AI
{
    public class ConsoleUI
    {
        const int EMPTY = 0;
        const int X = 1;
        const int O = 2;

        public static int boardSize;

        public static string boardStyle;

        static string ABC = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" + " ".PadLeft(1000);

        static string ABV = "абвгдежзиклмнопрстуфхцчшщъыьэюя" + " ".PadLeft(1000);

        public static int askDepth() 
        {
            while (true)
            {
                try
                {
                    Console.Write("Search depth: ");
                    int depth = int.Parse(Console.ReadLine());
                    if (depth >= 0 && depth <= 30)
                        return depth;
                    Console.WriteLine("Invalid depth!");
                }
                catch { }
            }
        }

        public static int askTime()
        {
            while (true)
            {
                try
                {
                    Console.Write("Search time in ms: ");
                    int time = int.Parse(Console.ReadLine());
                    if (time >= 10 && time <= 1_000_000)
                        return time;
                    Console.WriteLine("Invalid time, enter (10 - 1_000_000)");
                }
                catch { }
            }
        }

        public static string askSym() 
        {
            while (true)
            {
                try
                {
                    Console.Write("Choose x or o: ");
                    string sym = Console.ReadLine();
                    if (sym == "x" || sym == "o")
                        return sym;
                    Console.Beep(100, 500);
                    Console.WriteLine("Invalid symbol!");
                }
                catch { }
            }
        }

        public static int askMove(int[] board)
        {
            char firstLowerLetter = ' ';
            char lastLowerLetter = ' ';
            char firstCapitalLetter = ' ';
            char lastCapitalLetter = ' ';

            if(boardStyle == "en") 
            {
                firstLowerLetter = 'a';
                lastLowerLetter = 'z';
                firstCapitalLetter = 'A';
                lastCapitalLetter = 'Z';
            }
            else if(boardStyle == "ru") 
            {
                firstLowerLetter = 'а';
                lastLowerLetter = 'я';
                firstCapitalLetter = 'А';
                lastCapitalLetter = 'Я';
            }


            while (true)
            {
                try
                {
                    int pos;
                    Console.Write("Your move: ");
                    string input = Console.ReadLine();
                    if (input[0] >= firstLowerLetter && input[0] <= lastLowerLetter)
                    {
                        int y = (input[0] - firstLowerLetter);

                        if (boardStyle == "ru") y = FromRuToInt(input[0]);

                        int x = 0;
                        if(boardStyle == "en") x = int.Parse(input.Substring(1)) - 1;
                        else if(boardStyle == "ru") x = boardSize - int.Parse(input.Substring(1));

                        pos = boardSize * x + y;
                    }
                    else if (input[0] >= firstCapitalLetter && input[0] <= lastCapitalLetter)
                    {
                        int y = (input[0] - firstCapitalLetter);

                        int x = 0;
                        if (boardStyle == "en") x = int.Parse(input.Substring(1)) - 1;
                        else if (boardStyle == "ru") x = boardSize - int.Parse(input.Substring(1));

                        pos = boardSize * x + y;

                    }
                    else
                    {
                        pos = int.Parse(input);
                    }
                    if (pos >= 0 && pos < boardSize * boardSize && board[pos] == EMPTY)
                    {
                        return pos;
                    }
                    else
                    {
                        Console.Beep(500, 500);
                        Console.WriteLine("Invalid move, try again");
                    }
                }
                catch { }
            }
        }

        public static int askBoardSize()
        {
            while (true)
            {
                try
                {
                    Console.Write("Board size: ");
                    int size = int.Parse(Console.ReadLine());
                    if (size > 1 && size <= 1000)
                        return size;
                    Console.Write("Invalid size!");
                }
                catch { }
            }
        }


        public static void showBoard(int[] board)
        {
            string res = "";
            string a = "+";
            string b;

            string letters = new string(' ', 1000);

            if (boardStyle == "en") letters = ABC;
            else if (boardStyle == "ru") letters = ABV;

            for (int i = 0; i < boardSize; i++) a += "---+";
            res += a + "\n";
            for (int i = 0; i < boardSize; i++)
            {
                b = "|";
                for (int j = 0; j < boardSize; j++)
                {
                    string content = ConvertToSym(board[boardSize * i + j]);
                    b += $" {content} |";
                }

                int digit = 0;

                if (boardStyle == "en") digit = i + 1;
                else if (boardStyle == "ru") digit = boardSize - i;

                b += " " + digit.ToString();
                res += b + "\n";
                res += a + "\n";
            }
            a = "";
            for (int i = 0; i < boardSize; i++) a += $"  {letters[i]} ";
            res += a + "\n";
            Console.Write(res);

        }

        public static void showIndexes(int[] board)
        {

            string res = "";
            string a = "+";
            string b;

            string letters = new string(' ', 1000);

            if (boardStyle == "en") letters = ABC;
            else if (boardStyle == "ru") letters = ABV;

            for (int i = 0; i < boardSize; i++) a += "-----+";
            res += a + "\n";
            for (int i = 0; i < boardSize; i++)
            {
                b = "|";
                for (int j = 0; j < boardSize; j++)
                {
                    string content = (i * boardSize + j).ToString().PadLeft(3);
                    if (board[i * boardSize + j] != 0) content = ConvertToSym(board[i * boardSize + j]).ToString().PadLeft(3);
                    b += $" {content} |";
                }

                int digit = 0;

                if (boardStyle == "en") digit = i + 1;
                else if (boardStyle == "ru") digit = boardSize - i;


                b += " " + digit.ToString();
                res += b + "\n";
                res += a + "\n";
            }
            a = "";
            for (int i = 0; i < boardSize; i++) a += $"   {letters[i]}  ";
            res += a + "\n";
            Console.Write(res);

        }

        public static void noShow(int[] board) 
        {
        }


        public static string ConvertToSym(int x)
        {
            switch (x)
            {
                case EMPTY:
                    return " ";
                case X:
                    return "X";
                case O:
                    return "O";
                default:
                    return " ";
            }
        }

        public static string ConvertToMove(int move)
        {
            if (move < 0 || move >= boardSize*boardSize) return "?";

            string letters = new string(' ', 1000);

            int digit = 0;

            if (boardStyle == "en") letters = ABC;
            else if (boardStyle == "ru") letters = ABV;

            if (boardStyle == "en") digit = move / boardSize + 1;
            else if (boardStyle == "ru") digit = boardSize - move / boardSize;

            return letters[move % boardSize] + digit.ToString();

        }

        public static int FromRuToInt(char letter) 
        {
           
            if (letter <= 'и') return letter - 'а';
            return letter - 'а' - 1;

        }
    }
}
