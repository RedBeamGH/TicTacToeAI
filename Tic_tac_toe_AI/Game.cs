using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Tic_tac_toe_AI
{

    public class Game
    {
        public bool tmpDebug = false;

        public const int EMPTY = 0;
        public const int X = 1;
        public const int O = 2;

        public const int HUMAN = 3;
        public const int COMP = 4;

        public const int IN_PROGRESS = 0;
        public const int X_WIN = 1;
        public const int O_WIN = 2;
        public const int DRAW = 3;

        public bool doNotShowBoard = false;

        public bool debug = false;
        public bool cheating = false;
        public bool pressEnterToContinue = false;
        public bool bruteForceEvaluation = false;
        public bool mtdfSearch = false;

        public int moveOrderingDepth;

        public int curMaxDepth;

        public int curMaxBreadth;

        public ulong transpositionTableSize = 100_000;

        public string boardStyle = "en";

        public TranspositionTable tt;

        public int[] linesWeights;
        public int[] defaultLinesWeights;

        public Player playerX;
        public Player playerO;

        public int delay = 0;

        string gameType;
        public string gameResult = "Undefined";

        int numEvaluatedMoves;
        int numOrderings;

        int boardSize;
        int winLen;

        int bestMove;
        public int compMove;

        public int numMoves;

        public int[] board;

        HashSet<int> availableMoves;

        int[] allDirections;

        int[] top;
        int[] bottom;
        int[] left;
        int[] right;

        int[][] sideDir;

        int[] topLeft;
        int[] topRight;
        int[] bottomLeft;
        int[] bottomRigth;

        int[] corners;
        int[][] cornerDir;
        public delegate bool isSide(int pos);

        isSide[] isSides;

        public int[] lines_playerX;
        public int[] lines_playerO;

        public delegate int EvalFunction(int[] linesX, int[] linesO);

        public EvalFunction evalFunction;
        public static EvalFunction defaultEvalFunction;

        int[][] startsGrid;
        int[][] endsGrid;

        int[] steps;

        bool isTop(int pos) => pos < boardSize;

        bool isBottom(int pos) => pos >= boardSize * (boardSize - 1);

        bool isLeft(int pos) => pos % boardSize == 0;

        bool isRight(int pos) => pos % boardSize == boardSize - 1;


        public Game(int size, int len)
        {
            boardSize = size;
            winLen = len;
            bestMove = -1;
            numMoves = 0;

            board = new int[boardSize * boardSize];
            availableMoves = new HashSet<int>();

            allDirections = new int[] { -boardSize - 1, -boardSize, -boardSize + 1, +1, boardSize + 1, boardSize, boardSize - 1, -1 };

            top = new int[] { -1, -boardSize - 1, -boardSize, -boardSize + 1, +1 };
            bottom = new int[] { +1, boardSize + 1, boardSize, boardSize - 1, -1 };
            right = new int[] { -boardSize, -boardSize + 1, +1, boardSize + 1, boardSize };
            left = new int[] { boardSize, boardSize - 1, -1, -boardSize - 1, -boardSize };

            sideDir = new int[][] { bottom, top, right, left };

            topLeft = new int[] { -1, -boardSize - 1, -boardSize };
            topRight = new int[] { -boardSize, -boardSize + 1, +1 };
            bottomRigth = new int[] { +1, boardSize + 1, boardSize };
            bottomLeft = new int[] { boardSize, boardSize - 1, -1 };

            corners = new int[] { 0, boardSize - 1, boardSize * (boardSize - 1), boardSize * boardSize - 1 };

            cornerDir = new int[][] { bottomRigth, bottomLeft, topRight, topLeft };

            isSides = new isSide[] { isTop, isBottom, isLeft, isRight };

            lines_playerX = new int[winLen + 1];
            lines_playerO = new int[winLen + 1];

            defaultLinesWeights = new int[winLen + 1];
            for (int i = 1; i < winLen; i++) defaultLinesWeights[i] = 2* defaultLinesWeights[i - 1] + i;
            defaultLinesWeights[winLen] = 10000;

            defaultEvalFunction = (int[] linesX, int[] linesO) =>
            {
                int res = 0;
                for (int i = 0; i <= winLen; i++)
                    res += (lines_playerX[i] - lines_playerO[i]) * defaultLinesWeights[i];
                return res;
            };


            curMaxBreadth = boardSize * boardSize;

            steps = new int[] { boardSize, 1, boardSize + 1, boardSize - 1 };

            tt = new TranspositionTable(board, transpositionTableSize);

            ConsoleUI.boardSize = boardSize;

            initializeEvaluationGrid();
        }

        public int getRandomMove()
        {
            Random r = new Random();

            double a = r.NextDouble();
            double b = r.NextDouble();
            int x = (int)Math.Floor((a / 4 + 0.375) * boardSize);
            int y = (int)Math.Floor((b / 4 + 0.375) * boardSize);

            //Console.WriteLine($"Random move: {ConsoleUI.ConvertToMove(x * boardSize + y)}");
            return Math.Clamp(x * boardSize + y, 0, board.Length - 1);
        }


        public void initializeEvaluationGrid()
        {
            startsGrid = new int[boardSize * boardSize][];
            endsGrid = new int[boardSize * boardSize][];

            for (int pos = 0; pos < boardSize * boardSize; pos++)
            {
                startsGrid[pos] = new int[4];
                endsGrid[pos] = new int[4];

                startsGrid[pos][0] = Math.Max(pos - boardSize * (winLen - 1), pos % boardSize); // Vertical
                endsGrid[pos][0] = Math.Min(pos, boardSize * (boardSize - winLen) + pos % boardSize);

                startsGrid[pos][1] = Math.Max(pos - (winLen - 1), pos - (pos % boardSize));     // Horizontal
                endsGrid[pos][1] = Math.Min(pos, pos + (boardSize - pos % boardSize) - winLen);


                startsGrid[pos][2] = pos;                                                       // Diagonal 1

                for (int i = 0; i < winLen - 1; i++)
                {
                    if (startsGrid[pos][2] < boardSize || ((startsGrid[pos][2] % boardSize) == 0))
                        break;

                    startsGrid[pos][2] -= steps[2];
                }

                endsGrid[pos][2] = pos;

                for (int i = 0; i < winLen - 1; i++)
                {
                    if (endsGrid[pos][2] >= boardSize * (boardSize - 1) || ((endsGrid[pos][2] % boardSize) == boardSize - 1))
                        break;

                    endsGrid[pos][2] += steps[2];
                }

                endsGrid[pos][2] -= steps[2] * (winLen - 1);

                startsGrid[pos][3] = pos;                                                       // Diagonal 2

                for (int i = 0; i < winLen - 1; i++)
                {
                    if (startsGrid[pos][3] < boardSize || ((startsGrid[pos][3] % boardSize) == boardSize - 1))
                        break;

                    startsGrid[pos][3] -= steps[3];
                }

                endsGrid[pos][3] = pos;

                for (int i = 0; i < winLen - 1; i++)
                {
                    if (endsGrid[pos][3] >= boardSize * (boardSize - 1) || ((endsGrid[pos][3] % boardSize) == 0))
                        break;

                    endsGrid[pos][3] += steps[3];
                }

                endsGrid[pos][3] -= steps[3] * (winLen - 1);

            }

        }

        public int gameState()
        {
            if (lines_playerX[winLen] != 0)
                return X_WIN;
            if (lines_playerO[winLen] != 0)
                return O_WIN;
            if (numMoves == boardSize * boardSize)
                return DRAW;
            return IN_PROGRESS;
        }

        public void startGame(string type = "HumanVsComp", int startFromRandomMove = -1)
        {
            gameType = type;

            ConsoleUI.boardStyle = boardStyle;

            switch (gameType)
            {
                case "HumanVsComp":
                    string sym = ConsoleUI.askSym();
                    int time = ConsoleUI.askTime();
                    if (sym == "x")
                    {
                        playerX = new Player(HUMAN, X);
                        playerO = new Player(COMP, O, 20, 8, time);
                    }
                    else if (sym == "o")
                    {
                        playerO = new Player(HUMAN, O);
                        playerX = new Player(COMP, X, 20, 8, time);
                    }
                    break;

                case "CompVsComp":
                    playerX = new Player(COMP, X, 9, 10, 500);
                    playerO = new Player(COMP, O, 11, 10, 500, "MTDF");
                    break;

                case "HumanVsHuman":
                    playerX = new Player(HUMAN, X);
                    playerO = new Player(HUMAN, O);
                    break;
                case "TestGame":
                    doNotShowBoard = true;
                    break;

                default: break;
            }

            if (startFromRandomMove != -1)
                MakeRandomFirstMove(playerX, startFromRandomMove);
            else
                AddInitialAvailableMoves();

            gameLoop();

            switch (gameState())
            {
                case X_WIN:
                    gameResult = "X_WIN"; break;
                case O_WIN:
                    gameResult = "O_WIN"; break;
                case DRAW:
                    gameResult = "DRAW"; break;
                default: break;
            }
            if (gameType != "TestGame")
                Console.WriteLine(gameResult);
        }

        public void gameLoop()
        {
            var show = ConsoleUI.noShow;
            if (!doNotShowBoard) show = ConsoleUI.showBoard;
            if (debug) show = ConsoleUI.showIndexes;
            show(board);

            Player[] players = numMoves % 2 == 0 ? new Player[] { playerX, playerO } : new Player[] { playerO, playerX };

            while (!GameIsOver())
            {
                foreach (Player player in players)
                {
                    Move(player);
                    show(board);
                    if (GameIsOver()) break;
                }
            }
        }


        public bool GameIsOver()
        {
            switch (gameState())
            {
                case X_WIN:
                    return true;
                case O_WIN:
                    return true;
                case DRAW:
                    return true;
                default:
                    return false;
            }
        }

        public void loadPosition(int[] loadBoard) 
        {
            loadBoard.CopyTo(board, 0);

            tt.hash = tt.getHash(board);

            bruteForceEvaluate();

            for (int i = 0; i < board.Length; i++) if (board[i] != EMPTY) numMoves++;

        }

        public void AddInitialAvailableMoves() 
        {
            for (int i = boardSize / 3; i < (boardSize + 1) / 2; i++)
            {
                for (int j = i; j < (boardSize + 1) / 2; j++)
                {
                    availableMoves.Add(i * boardSize + j);
                }
            }
        }

        public void MakeRandomFirstMove(Player player, int randomMove = -1)
        {
            if (randomMove == -1)
                randomMove = getRandomMove();
            numMoves++;
            makeMove(randomMove, player.mark);
        }

        public void ProcessMove(Player player, int move = -1)
        {
            numMoves++;
            if (player.type == COMP)
            {

                compMove = getBestMove(player);
                if (compMove < 0)
                {
                    Random randomizer = new Random();
                    int[] moves = availableMoves.ToArray();
                    compMove = moves[randomizer.Next(moves.Length)];
                }

                if (numMoves == 1) for (int i = 0; i < board.Length; i++) availableMoves.Remove(i);

                makeMove(compMove, player.mark);
            }
            else if (player.type == HUMAN)
            {

                if (numMoves == 1) for (int i = 0; i < board.Length; i++) availableMoves.Remove(i);

                makeMove(move, player.mark);
            }
        }

        public void Move(Player player)
        {
            numMoves++;
            if (debug)
            {
                Console.WriteLine($"Hash: {Convert.ToString((uint)tt.Index, 16).PadLeft(8, '0')}");
                Console.WriteLine($"Move №{numMoves}");
                Console.WriteLine($"Turn: {ConsoleUI.ConvertToSym(player.mark)}");

                Console.Write("lines_playerO: ");
                for (int i = 1; i < lines_playerO.Length; i++)
                {
                    Console.Write($"{i}: {lines_playerO[i]}; ");
                }
                Console.WriteLine();

                Console.Write("lines_playerX: ");
                for (int i = 1; i < lines_playerX.Length; i++)
                {
                    Console.Write($"{i}: {lines_playerX[i]}; ");
                }
                Console.WriteLine();
            }

            if (player.type == COMP)
            {
                if (pressEnterToContinue) Console.ReadLine();
                else Thread.Sleep(delay);

                if (debug)
                {
                    Console.WriteLine("AvailableMoves:");
                    foreach (int i in availableMoves) Console.Write($"{ConsoleUI.ConvertToMove(i)} ");
                    Console.WriteLine();

                    Console.WriteLine($"Type: Computer");
                }
                int move = getBestMove(player);
                if (move < 0)
                {
                    if (debug)
                        Console.WriteLine("BEST MOVE NOT FOUND!!! Trying random move");
                    Random randomizer = new Random();
                    int[] moves = availableMoves.ToArray();
                    move = moves[randomizer.Next(moves.Length)];
                }

                if (numMoves == 1) for (int i = 0; i < board.Length; i++) availableMoves.Remove(i);

                if (debug)
                    Console.WriteLine($"Computer move: {ConsoleUI.ConvertToMove(move)}");
                makeMove(move, player.mark);
            }
            else if (player.type == HUMAN)
            {
                Console.WriteLine($"Type: Human");
                if (cheating)
                    Console.WriteLine($"Best move for you: {ConsoleUI.ConvertToMove(getBestMove(player))}");

                int move = ConsoleUI.askMove(board);

                if (numMoves == 1) for (int i = 0; i < board.Length; i++) availableMoves.Remove(i);

                makeMove(move, player.mark);
            }
        }

        public int[] updateAvailableMoves(int pos) // Adds adjacent squares to available moves and returns them
        {

            // Corners
            int[] res;
            for (int i = 0; i < 4; i++)
            {
                if (pos == corners[i])
                {
                    res = new int[3] { -1, -1, -1 };
                    for (int j = 0; j < 3; j++)
                    {
                        int p = pos + cornerDir[i][j];
                        if (p < 0 || p >= board.Length) continue;
                        if (board[p] == EMPTY && !availableMoves.Contains(p))
                        {
                            availableMoves.Add(p);
                            res[j] = p;
                        }
                    }
                    return res;
                }
            }

            // Sides

            for (int i = 0; i < 4; i++)
            {
                if (isSides[i](pos))
                {
                    res = new int[5] { -1, -1, -1, -1, -1 };
                    for (int j = 0; j < 5; j++)
                    {
                        int p = pos + sideDir[i][j];
                        if (board[p] == EMPTY && !availableMoves.Contains(p))
                        {
                            availableMoves.Add(p);
                            res[j] = p;
                        }
                    }
                    return res;
                }
            }

            // Others
            res = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < 8; i++)
            {
                int p = pos + allDirections[i];
                if (board[p] == EMPTY && !availableMoves.Contains(p))
                {
                    availableMoves.Add(p);
                    res[i] = p;
                }
            }
            return res;

        }

        public int[] makeMove(int pos, int player)
        {
            tt.updateHash(pos, player);
            updateLines(pos, -1);
            board[pos] = player;
            updateLines(pos, 1);


            availableMoves.Remove(pos);
            return updateAvailableMoves(pos); // Return new available moves
        }

        public void unMakeMove(int pos, int player)
        {
            tt.updateHash(pos, player);
            updateLines(pos, -1);
            board[pos] = EMPTY;
            updateLines(pos, 1);
        }

        public void updateLines(int pos, int factor)
        {
            if (winLen > boardSize) return;

            int[] starts = startsGrid[pos];
            int[] ends = endsGrid[pos];

            for (int dir = 0; dir < 4; dir++)
            {
                //Console.WriteLine(starts[dir].ToString() + " " +  ends[dir].ToString() + " " + steps[dir].ToString());
                for (int i = starts[dir]; i <= ends[dir]; i += Math.Max(steps[dir], 1))
                {
                    int count_playerX = 0;
                    int count_playerO = 0;
                    for (int j = 0; j < winLen; j++)
                    {
                        int curSquare = board[i + steps[dir] * j];
                        if (curSquare == X) count_playerX++;
                        if (curSquare == O) count_playerO++;
                    }
                    if (count_playerO == 0) lines_playerX[count_playerX] += factor;
                    if (count_playerX == 0) lines_playerO[count_playerO] += factor;
                }
            }
        }


        public int Evaluate()
        {
            numEvaluatedMoves++;

            if (bruteForceEvaluation)
                return bruteForceEvaluate();

            return evalFunction(lines_playerX, lines_playerO);
        }

        public int bruteForceEvaluate()
        {

            for (int i = 0; i <= winLen; i++)
            {
                lines_playerX[i] = 0;
                lines_playerO[i] = 0;
            }

            int count_playerX;
            int count_playerO;
            int curSquare;

            // Vertical

            for (int i = 0; i <= boardSize - winLen; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    count_playerX = 0;
                    count_playerO = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k * boardSize];
                        if (curSquare == X) count_playerX++;
                        if (curSquare == O) count_playerO++;

                    }
                    if (count_playerO == 0) lines_playerX[count_playerX]++;
                    if (count_playerX == 0) lines_playerO[count_playerO]++;
                }
            }

            // Horizontal

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j <= boardSize - winLen; j++)
                {
                    count_playerX = 0;
                    count_playerO = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k];
                        if (curSquare == X) count_playerX++;
                        if (curSquare == O) count_playerO++;

                    }
                    if (count_playerO == 0) lines_playerX[count_playerX]++;
                    if (count_playerX == 0) lines_playerO[count_playerO]++;
                }
            }

            // Diagonal 1

            for (int i = 0; i <= boardSize - winLen; i++)
            {
                for (int j = 0; j <= boardSize - winLen; j++)
                {
                    count_playerX = 0;
                    count_playerO = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k * (boardSize + 1)];
                        if (curSquare == X) count_playerX++;
                        if (curSquare == O) count_playerO++;

                    }
                    if (count_playerO == 0) lines_playerX[count_playerX]++;
                    if (count_playerX == 0) lines_playerO[count_playerO]++;
                }
            }

            // Diagonal 2

            for (int i = 0; i <= boardSize - winLen; i++)
            {
                for (int j = winLen - 1; j < boardSize; j++)
                {
                    count_playerX = 0;
                    count_playerO = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k * (boardSize - 1)];
                        if (curSquare == X) count_playerX++;
                        if (curSquare == O) count_playerO++;

                    }
                    if (count_playerO == 0) lines_playerX[count_playerX]++;
                    if (count_playerX == 0) lines_playerO[count_playerO]++;
                }
            }

            return evalFunction(lines_playerX, lines_playerO);
        }

        public (int, int) findWinningLine(int mark) 
        {
            int count;
            int curSquare;

            // Vertical

            for (int i = 0; i <= boardSize - winLen; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    count = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k * boardSize];
                        if (curSquare == mark) count++;
                        else break;
                    }
                    if (count == winLen) 
                        return (boardSize * i + j, boardSize * i + j + (winLen - 1) * boardSize);
                }
            }

            // Horizontal

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j <= boardSize - winLen; j++)
                {
                    count = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k];
                        if (curSquare == mark) count++;
                        else break;
                    }
                    if (count == winLen) 
                        return (boardSize * i + j, boardSize * i + j + winLen - 1);
                }
            }

            // Diagonal 1

            for (int i = 0; i <= boardSize - winLen; i++)
            {
                for (int j = 0; j <= boardSize - winLen; j++)
                {
                    count = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k * (boardSize + 1)];
                        if (curSquare == mark) count++;
                        else break;
                    }
                    if (count == winLen)
                        return (boardSize * i + j, boardSize * i + j + (winLen - 1) * (boardSize + 1));
                }
            }

            // Diagonal 2

            for (int i = 0; i <= boardSize - winLen; i++)
            {
                for (int j = winLen - 1; j < boardSize; j++)
                {
                    count = 0;
                    for (int k = 0; k < winLen; k++)
                    {
                        curSquare = board[boardSize * i + j + k * (boardSize - 1)];
                        if (curSquare == mark) count++;
                        else break;
                    }
                    if (count == winLen)
                        return (boardSize * i + j, boardSize * i + j + (winLen - 1) * (boardSize - 1));
                }
            }

            return (0, boardSize*boardSize);
        }

        public int getBestMove(Player player)
        {
            if (winLen > boardSize) return -1;


            bestMove = -1;

            int score = 0;

            long dTime = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            tt.Clear();

            curMaxBreadth = player.maxBreadth;
            if (debug)
            {
                Console.WriteLine($"{player.searchAlgorithm} Search");
                Console.WriteLine("Depth    Time Move Score Transpositions Evaluations       Speed");
            }

            if (player.evalFunction != null) evalFunction = player.evalFunction;
            else evalFunction = defaultEvalFunction;

            watch.Restart();
            for (curMaxDepth = 1; curMaxDepth <= player.maxDepth; curMaxDepth++)
            {
                moveOrderingDepth = curMaxDepth - 2;
                numEvaluatedMoves = 0;
                tt.numTranspositions = 0;
                //tt.Clear();
                numOrderings = 0;
              
                if (player.searchAlgorithm == "alpha-beta")
                    score = alphaBetaSearch(curMaxDepth, -10_000, 10_000, player.mark);
                else if(player.searchAlgorithm == "MTDF")
                    score = MTDFSearch(score, curMaxDepth, player.mark);

                if (debug)
                {
                    Console.Write($"{curMaxDepth}".PadLeft(3));
                    Console.Write($"{watch.ElapsedMilliseconds}ms".PadLeft(10));
                    Console.Write($"{ConsoleUI.ConvertToMove(bestMove)}".PadLeft(5));
                    Console.Write($"{score}".PadLeft(6));
                    Console.Write($"{tt.numTranspositions}".PadLeft(15));
                    Console.Write($"{numEvaluatedMoves}".PadLeft(12));
                    Console.Write($"{(numEvaluatedMoves / ((watch.ElapsedMilliseconds - dTime) / 1000.0)):f0} Evals/sec".PadLeft(22));
                    Console.WriteLine();
                }
                dTime = watch.ElapsedMilliseconds;

                if (debug)
                {
                    if (score > 9000)
                    {
                        if (10000 - score - 1 == 1)
                            Console.WriteLine($"99% X will win after 1 move");
                        else
                            Console.WriteLine($"99% X will win after {10000 - score - 1} moves");

                        //tmpDebug = true;
                        //tt.Clear();
                        //alphaBetaSearch(curMaxDepth, -10_000, 10_000, player.mark);

                        break;
                    }
                    else if (score < -9000)
                    {
                        if (10000 + score - 1 == 1)
                            Console.WriteLine($"99% O will win after 1 move");
                        else
                            Console.WriteLine($"99% O will win after {10000 + score - 1} moves");
                        break;
                    }
                }

                if (watch.ElapsedMilliseconds > player.maxSearchTime)
                    break;
            }

            return bestMove;
        }

        public int MTDFSearch(int f, int depth, int player) 
        {
            int g = f;
            int upperBound = 10000;
            int lowerBound = -10000;

            while (lowerBound < upperBound)
            {
                int beta;
                if (g == lowerBound)
                    beta = g + 5;
                else
                    beta = g;
                g = alphaBetaSearch(depth, beta - 5, beta, player);

                if (g < beta)
                    upperBound = g;
                else
                    lowerBound = g;
            }

            return g;
        }


        public int alphaBetaSearch(int depth, int alpha, int beta, int player)
        {
            switch (gameState())
            {
                case X_WIN:
                    return 10000 - (curMaxDepth - depth);
                case O_WIN:
                    return -10000 + (curMaxDepth - depth);
                default:
                    break;
            }

            int bestEval;

            int ttVal = tt.CheckEvaluation(depth, alpha, beta);
            if (ttVal != TranspositionTable.notFound)
            {
                tt.numTranspositions++;
                if (depth == curMaxDepth)
                {
                    bestMove = tt.getStoredMove();
                }
                return ttVal;
            }

            if (depth == 0)
            {
                return Evaluate();
            }

            int[] moves = availableMoves.ToArray();

            if (moves.Length == 0)
            {
                return Evaluate();
            }

            bool movesNotOrdered = false;
            if (curMaxDepth - depth < moveOrderingDepth)
                OrderMoves(moves, player, depth);
            else
                movesNotOrdered = true;

            int curBestMove = -1;
            int[] addedAvailableMoves;

            int evalType = -1;
            if (player == X)
            {

                bestEval = -20_000;

                int end = Math.Max(moves.Length - curMaxBreadth, 0);
                if (movesNotOrdered)
                    end = 0;

                for (int i = moves.Length - 1; i >= end; i--)
                {
                    addedAvailableMoves = makeMove(moves[i], player);
                    int evaluation = alphaBetaSearch(depth - 1, alpha, beta, O);
                    unMakeMove(moves[i], player);
                    availableMoves.Add(moves[i]);
                    for (int j = 0; j < addedAvailableMoves.Length; j++)
                    {
                        availableMoves.Remove(addedAvailableMoves[j]);
                    }
                    if (bestEval < evaluation)
                    {
                        evalType = TranspositionTable.Exact;
                        bestEval = evaluation;
                        curBestMove = moves[i];
                        if (depth == curMaxDepth) bestMove = moves[i];
                    }
                    alpha = Math.Max(alpha, bestEval);
                    if (bestEval >= beta)
                    {
                        evalType = TranspositionTable.LowerBound;
                        tt.StoreEvaluation(depth, bestEval, evalType, curBestMove);
                        break;
                    }
                }
            }
            else
            {

                bestEval = 20_000;

                int end = Math.Min(curMaxBreadth, moves.Length);
                if (movesNotOrdered)
                    end = moves.Length;

                for (int i = 0; i < end; i++)
                {
                    addedAvailableMoves = makeMove(moves[i], player);
                    int evaluation = alphaBetaSearch(depth - 1, alpha, beta, X);
                    unMakeMove(moves[i], player);
                    availableMoves.Add(moves[i]);
                    for (int j = 0; j < addedAvailableMoves.Length; j++)
                    {
                        availableMoves.Remove(addedAvailableMoves[j]);
                    }
                    if (bestEval > evaluation)
                    {
                        evalType = TranspositionTable.Exact;
                        bestEval = evaluation;
                        curBestMove = moves[i];
                        if (depth == curMaxDepth) bestMove = moves[i];
                    }
                    beta = Math.Min(beta, bestEval);
                    if (bestEval <= alpha)
                    {
                        evalType = TranspositionTable.UpperBound;
                        tt.StoreEvaluation(depth, bestEval, evalType, curBestMove);
                        break;
                    }
                }
            }
            if (evalType == TranspositionTable.Exact)
                tt.StoreEvaluation(depth, bestEval, evalType, curBestMove);
            return bestEval;
        }

        public void OrderMoves(int[] moves, int player, int depth) // Not working if Breadth <= 4
        {
            numOrderings++;
            int[] evals = new int[moves.Length];
            for (int i = 0; i < moves.Length; i++)
            {
                updateLines(moves[i], -1);
                board[moves[i]] = player;
                updateLines(moves[i], 1);

                switch (gameState())
                {
                    case X_WIN:
                        evals[i] = 10000 - (curMaxDepth - depth) - 1;
                        break;
                    case O_WIN:
                        evals[i] = -10000 + (curMaxDepth - depth) + 1;
                        break;
                    default:
                        evals[i] = Evaluate();
                        break;
                }
                updateLines(moves[i], -1);
                board[moves[i]] = EMPTY;
                updateLines(moves[i], 1);
            }
            Array.Sort(evals, moves);
        }
    }
}
