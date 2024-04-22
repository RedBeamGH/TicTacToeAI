using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Tic_tac_toe_AI;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            int winsAsX, winsAsO;
            int losesAsX, losesAsO;
            int drawsAsX, drawsAsO;
            int maxSearchTime = 10;

            int boardSize = 9;
            int winLen = 5;
            int numGamesOneSide = 10;

            int[] firstRandomMoves = new int[numGamesOneSide];

            List<int[]> grid = new List<int[]>();

            for(int i  = 0; i < 5; i++) 
            {
                for(int j = 0; j < 5; j++) 
                {
                    for (int k = 0; k < 5; k++) 
                    {
                        int[] W = {0, 1, k*2 + 2, j*10 + k*2 + 10, i*100 + j*10 + 40, 10000};
                        grid.Add(W);
                    }
                }
            }
            int numIterations = grid.Count;



            for (int i = 0 ; i < numGamesOneSide; i++) 
            {
                int move;
                Random r = new Random();
                double a = r.NextDouble();
                double b = r.NextDouble();
                int x = (int)Math.Floor((a / 4 + 0.375) * boardSize);
                int y = (int)Math.Floor((b / 4 + 0.375) * boardSize);
                move = x * boardSize + y;
                firstRandomMoves[i] = move;
                Console.WriteLine(firstRandomMoves[i]);
            }

            int[] bestWeights = { 0, 0, 10, 58, 480, 10000};
            int[] newWeights;

            grid[0] = new int[6] { 0, 0, 0, 10, 480, 10000};

            Game.EvalFunction evalFunction = (int[] linesX, int[] linesO) =>
            {
                int res = 0;

                res += (linesX[1] - linesO[1]) * 1;
                res += (linesX[2] - linesO[2]) * 10;
                res += (linesX[3] - linesO[3]) * 58;
                res += (linesX[4] - linesO[4]) * 480;
                res += (linesX[5] - linesO[5]) * 10000;

                return res;
            };

            numIterations = 1;

            for (int iteration = 0; iteration < numIterations; iteration++)
            {

                newWeights = grid[iteration];


                Player p1 = new Player(Game.COMP, Game.X, 12, 8, maxSearchTime);
                Player p2 = new Player(Game.COMP, Game.O, 12, 8, maxSearchTime);

                p1.evalFunction = evalFunction;

                Player pX = p1;
                Player pO = p2;

                winsAsX = winsAsO = losesAsO = losesAsX = drawsAsO = drawsAsX = 0;
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i < numGamesOneSide; i++)
                    {
                        Game game = new Game(boardSize, winLen);
                        game.playerX = pX;
                        game.playerO = pO;
                        game.startGame("TestGame", startFromRandomMove: firstRandomMoves[i]);
                        switch (game.gameResult)
                        {
                            case "X_WIN":
                                if (j == 0)
                                    winsAsX++;
                                else
                                    losesAsO++;
                                break;
                            case "O_WIN":
                                if (j == 0)
                                    losesAsX++;
                                else
                                    winsAsO++;
                                break;
                            case "DRAW":
                                if (j == 0)
                                    drawsAsX++;
                                else
                                    drawsAsO++;
                                break;
                            default:
                                break;
                        }
                        Console.WriteLine($"Game {j * numGamesOneSide + i + 1}: {game.gameResult}, moves: {game.numMoves}");
                    }
                    
                    pX = p2;
                    pX.mark = Game.X;
                    pO = p1;
                    pO.mark = Game.O;
                }
                int totalWins = winsAsX + winsAsO;
                int totalLoses = losesAsX + losesAsO;
                Console.WriteLine("Round Results");
                Console.WriteLine($"ResultsAsX: wins:{winsAsX}, loses:{losesAsX}, draws:{drawsAsX}");
                Console.WriteLine($"ResultsAsO: wins:{winsAsO}, loses:{losesAsO}, draws:{drawsAsO}");
                Console.WriteLine($"Total results: wins:{totalWins}, loses:{totalLoses}, draws:{drawsAsX + drawsAsO}");
                if (totalWins > totalLoses)
                {
                    bestWeights = newWeights;
                    Console.WriteLine("New best weights!!! ");
                    for (int j = 0; j < 6; j++)
                        Console.Write(newWeights[j].ToString() + " ");
                    Console.WriteLine();
                }
                else 
                {
                    Console.WriteLine("Bad iteration, current best: ");
                    for (int j = 0; j < 6; j++)
                        Console.Write(bestWeights[j].ToString() + " ");
                    Console.WriteLine();
                }
            }
            Console.WriteLine("Best weights after all: ");
            for (int j = 0; j < 6; j++)
                Console.Write(bestWeights[j].ToString() + " ");
            Console.WriteLine();
        }

        public static int[] GenerateNewWeights(int[] oldWeights) 
        {
            int[] dW = { 0, 1, 2, 5, 10, 0 };

            int[] newWeights = new int[6];

            Random r = new Random();
            for (int i = 0; i < 5; i++) 
            {
                newWeights[i] = oldWeights[i] + r.Next(-dW[i], dW[i]);
                newWeights[i] = int.Clamp(newWeights[i], 0, 1000);
            }
            newWeights[5] = oldWeights[5];
            return newWeights;
        }
    }
}
