using System.Collections.Generic;

namespace Chess.PieceClasses
{
    public class Pawn : Piece
    {
        public int[]? EnpassantPos;
        public Pawn(PieceColor c, int i, int j) : base(c, i, j)
        {
            Body = c == PieceColor.White ? Pawn[0] : Pawn[1];
        }

        public override List<int[]> PieceMoves(bool byPlayer, Piece[,] Pieces)
        {
            List<int[]> toreturn = new List<int[]>();
            int idir = Color == PieceColor.White ? -1 : 1;
            bool forward = false;

            if (CheckMove(I + idir, J, false, Pieces))
            {
                toreturn.Add(new int[] { I + idir, J });
                forward = true;
            }

            if (CheckMove(I + idir, J - 1, true, Pieces))
                toreturn.Add(new int[] { I + idir, J - 1 });
            if (CheckMove(I + idir, J + 1, true, Pieces))
                toreturn.Add(new int[] { I + idir, J + 1 });

            if (!Moved && forward)
            {
                if (CheckMove(I + idir * 2, J, false, Pieces))
                    toreturn.Add(new int[] { I + idir * 2, J });
            }
            if (EnpassantPos != null)
            {
                toreturn.Add(EnpassantPos);
            }

            return toreturn;
        }
    }
}
