using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Tic_tac_toe_AI;

namespace WpfApp
{
    public class GameWrapper
    {
        public int boardSize = 5;
        public int winLen = 4;
        public int searchTime = 100;
        public bool computerMovesFirst = true;
        public bool computerToMove = true;

        public int[] board; 

        public Player HumanPlayer;
        public Player CompPlayer;

        public Game game;
        public GameWrapper()
        {
            Reset();
        }

        public void Reset() 
        {
            game = new Game(boardSize, winLen);
            computerToMove = computerMovesFirst;
            if (computerMovesFirst) 
            {
                CompPlayer = new Player(Game.COMP, Game.X, maxSearchTime:searchTime);
                HumanPlayer = new Player(Game.HUMAN, Game.O);
                game.MakeRandomFirstMove(CompPlayer);
                computerToMove = false;
            }
            else
            {
                HumanPlayer = new Player(Game.HUMAN, Game.X);
                CompPlayer = new Player(Game.COMP, Game.O, maxSearchTime: searchTime);
            }
            board = new int[boardSize*boardSize];
            Array.Copy(game.board, board, boardSize * boardSize);
        }

        public bool MoveIsValid(int move)
        {
            return move >= 0 && move < boardSize * boardSize && board[move] == Game.EMPTY;
        }

        public void ProcessCompMove() 
        {
            game.ProcessMove(CompPlayer);
            Array.Copy(game.board, board, boardSize * boardSize);
        }

        public void ProcessHumanMove(int move)
        {
            game.ProcessMove(HumanPlayer, move);
            Array.Copy(game.board, board, boardSize * boardSize);
        }

    }
}
