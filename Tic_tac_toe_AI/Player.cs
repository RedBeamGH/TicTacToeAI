using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_tac_toe_AI
{
    public class Player
    {
        public int type;
        public int mark;

        public int maxDepth;
        public int maxBreadth;
        public int maxSearchTime;

        public string searchAlgorithm;
        public Game.EvalFunction evalFunction;

        public Player(int type, int mark, int maxDepth = 20, int maxBreadth = 8, int maxSearchTime = 2_000,
                    string searchAlgorithm = "alpha-beta")
        {
            this.type = type;
            this.mark = mark;
            this.maxDepth = maxDepth;
            this.maxBreadth = maxBreadth;
            this.maxSearchTime = maxSearchTime;
            this.searchAlgorithm = searchAlgorithm;
            this.evalFunction = Game.defaultEvalFunction;
        }
    }
}
