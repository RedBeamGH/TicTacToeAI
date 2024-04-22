using System.Collections.Generic;

namespace Tic_tac_toe_AI
{
    class Program
    {
        static void Main()
        {
            /*int boardSize = UI.askBoardSize();

            int winLen;
            if (boardSize < 5) winLen = boardSize;
            else if (boardSize == 5) winLen = 4;
            else winLen = 5;

            Game game = new Game(boardSize, winLen);*/

            Game game = new Game(1, 1);

            game.debug = true;
            game.cheating = true;
            game.pressEnterToContinue = false;
            game.delay = 100;

            game.startGame();
            //game.startGame("CompVsComp");
            //game.startGame("HumanVsHuman");

        }
    }
}