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

            Game game = new Game(19, 5);

            game.debug = false;
            game.pressEnterToContinue = false;

            game.startGame();
            //game.startGame("CompVsComp");
            //game.startGame("HumanVsHuman");

        }
    }
}