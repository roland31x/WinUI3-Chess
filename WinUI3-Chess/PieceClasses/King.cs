using System.Collections.Generic;

namespace Chess.PieceClasses
{
    public class King : Piece
    {
        public King(PieceColor c, int i, int j) : base(c, i, j)
        {
            Body = c == PieceColor.White ? King[0] : King[1];
        }

        public override List<int[]> PieceMoves(bool byPlayer, Piece[,] Pieces)
        {
            List<int[]> toreturn = new List<int[]>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    if (CheckMove(I + i, J + j, true, Pieces) || CheckMove(I + i, J + j, false, Pieces))
                    {
                        toreturn.Add(new int[] { I + i, J + j });
                    }
                }
            }
            if (byPlayer)
            {
                if (CanCastleKingSide(Pieces))
                    toreturn.Add(new int[] { I, 6 });
                if (CanCastleQueenSide(Pieces))
                    toreturn.Add(new int[] { I, 2 });
            }          

            return toreturn;
        }
        bool CanCastleKingSide(Piece[,] Pieces)
        {     
            if (Moved)
                return false;
            if (Pieces[I, 7] == null || Pieces[I, 7].Moved)
                return false;

            PieceColor other = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            for (int j = J + 1; j < 7; j++)
            {
                if (Pieces[I, j] != null)
                    return false;
                foreach(Piece p in Pieces)
                {
                    if(p == null || p.Color != other) 
                        continue;
                    List<int[]> moves = p.PieceMoves(false, Pieces);
                    foreach (int[] move in moves)
                        if (move[0] == I && move[1] == j)
                            return false;
                }
            }
            return true;
        }
        bool CanCastleQueenSide(Piece[,] Pieces)
        {
            if (Moved)
                return false;
            if (Pieces[I, 0] == null || Pieces[I, 0].Moved)
                return false;

            PieceColor other = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            for (int j = J - 1; j > 0; j--)
            {
                if (Pieces[I, j] != null)
                    return false;
                foreach (Piece p in Pieces)
                {
                    if (p == null || p.Color != other)
                        continue;
                    List<int[]> moves = p.PieceMoves(false, Pieces);
                    foreach (int[] move in moves)
                        if (move[0] == I && move[1] == j)
                            return false;
                }
            }
            return true;
        }
    }
}
