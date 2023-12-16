using System.Collections.Generic;

namespace Chess.PieceClasses
{
    public class Queen : Piece
    {
        public Queen(PieceColor c, int i, int j) : base(c, i, j)
        {
            Body = c == PieceColor.White ? Queen[0] : Queen[1];
        }

        public override List<int[]> PieceMoves(bool byPlayer, Piece[,] Pieces)
        {
            List<int[]> toreturn = new List<int[]>();

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I + i, J, true, Pieces);
                bool move = CheckMove(I + i, J, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I + i, J });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I - i, J, true, Pieces);
                bool move = CheckMove(I - i, J, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I - i, J });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I, J - i, true, Pieces);
                bool move = CheckMove(I, J - i, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I, J - i });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I, J + i, true, Pieces);
                bool move = CheckMove(I, J + i, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I, J + i });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I + i, J + i, true, Pieces);
                bool move = CheckMove(I + i, J + i, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I + i, J + i });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I - i, J - i, true, Pieces);
                bool move = CheckMove(I - i, J - i, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I - i, J - i });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I + i, J - i, true, Pieces);
                bool move = CheckMove(I + i, J - i, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I + i, J - i });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                bool ok = false;
                bool attack = CheckMove(I - i, J + i, true, Pieces);
                bool move = CheckMove(I - i, J + i, false, Pieces);
                if (attack || move)
                {
                    toreturn.Add(new int[] { I - i, J + i });
                    ok = true;
                    if (attack)
                        ok = false;
                }
                if (!ok)
                    break;
            }

            return toreturn;
        }
    }
}
