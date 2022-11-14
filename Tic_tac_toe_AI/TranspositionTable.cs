using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_tac_toe_AI
{
    class TranspositionTable
    {
        public ulong hash;
        public ulong[,] table;
        public ulong size;

        public Evaluation[] evaluations;

        public const int notFound = -1;

        public const int Exact = 0;

        public const int LowerBound = 1;

        public const int UpperBound = 2;

        public bool enabled = true;

        public int numTranspositions;

        public TranspositionTable(int[] board, ulong size)
        {
            numTranspositions = 0;
            evaluations = new Evaluation[size];
            this.size = size;
            var random = new Random(12345);
            table = new ulong[board.Length, 2];

            hash = (ulong)random.NextInt64();

            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    table[i, j] = (ulong)random.NextInt64();
                }
            }
        }

        public void Clear()
        {
            numTranspositions = 0;
            for (int i = 0; i < evaluations.Length; i++)
            {
                evaluations[i] = new Evaluation();
            }
        }

        public ulong Index
        {
            get
            {
                return hash % size;
            }
        }

        public void updateHash(int pos, int mark)
        {
            hash ^= table[pos, mark - 1];
        }

        public int getStoredMove()
        {
            return evaluations[Index].move;
        }

        public ulong getHash(int[] board)
        {
            ulong h = 0;

            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] != 0)
                {
                    int mark = board[i];
                    h = h ^ table[i, mark - 1];
                }
            }
            return h;
        }

        public void StoreEvaluation(int depth, int eval, int evalType, int move)
        {
            if (!enabled)
            {
                return;
            }

            Evaluation evaluation = new Evaluation(hash, eval, (byte)move, (byte)depth, (byte)evalType);
            evaluations[Index] = evaluation;
        }

        public int CheckEvaluation(int depth, int alpha, int beta)
        {
            if (!enabled)
            {
                return notFound;
            }
            Evaluation evaluation = evaluations[Index];

            if (evaluation.hash != hash)
            { 
                return notFound; 
            }
            if (evaluation.depth < depth)
            {
                return notFound;
            }

            if (evaluation.type == Exact)
            {
                return evaluation.eval;
            }
            if (evaluation.type == UpperBound && evaluation.eval <= alpha) 
            {
                return evaluation.eval;
            }
            if (evaluation.type == LowerBound && evaluation.eval >= beta) 
            {
                return evaluation.eval;
            }

            return notFound;
        }

        public struct Evaluation
        {
            public ulong hash;
            public int eval;
            public int move;
            public byte depth;
            public byte type;

            public Evaluation(ulong h, int e, int m, byte d, byte t)
            {
                hash = h;
                eval = e;
                move = m;
                depth = d;
                type = t;
            }
        }

    }
}
