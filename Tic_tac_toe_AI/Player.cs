using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_tac_toe_AI
{
    class Player
    {
        public int type;
        public int mark;

        public int maxDepth;
        public int maxBreadth;
        public int maxSearchTime;

        public Player(int type, int mark)
        {
            this.type = type;
            this.mark = mark;
            maxDepth = 10;
            maxBreadth = 10;
            maxSearchTime = 5_000; // ms
        }

        public Player(int type, int mark, int maxDepth = 20, int maxBreadth = 8, int maxSearchTime = 2_000) : this(type, mark)
        {
            this.type = type;
            this.mark = mark;
            this.maxDepth = maxDepth;
            this.maxBreadth = maxBreadth;
            this.maxSearchTime = maxSearchTime;
        }
    }
}
