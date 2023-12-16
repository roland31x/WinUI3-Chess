using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;

namespace Chess.PieceClasses
{
    public abstract class Piece
    {
        public int I { get; set; }
        public int J { get; set; }
        public ImageBrush Body { get; protected set; }
        public PieceColor Color { get; protected set; }
        public bool Moved { get; set; } = false;
        public bool isAlive { get; set; } = true;
        protected Piece(PieceColor color, int i, int j)
        {
            Color = color;
            I = i;
            J = j;
        }
        protected bool CheckMove(int i, int j, bool canattack, Piece[,] Pieces)
        {
            try
            {
                if (Pieces[i, j] == null && !canattack)
                    return true;
                else if (Pieces[i, j] != null && canattack && Pieces[i, j].Color != Color)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        public abstract List<int[]> PieceMoves(bool byPlayer, Piece[,] Pieces);
        public virtual void Move(int newI, int newJ, Piece[,] Pieces)
        {
            if(Pieces[I, J].GetType() == typeof(Pawn))
            {
                if ((Pieces[I, J] as Pawn)!.EnpassantPos != null && (Pieces[I, J] as Pawn)!.EnpassantPos![0] == newI && (Pieces[I, J] as Pawn)!.EnpassantPos![1] == newJ)
                {
                    int dir = Color == PieceColor.White ? 1 : -1;
                    Pieces[newI + dir, newJ].isAlive = false;
                    Pieces[newI + dir, newJ] = null!;
                }            
            }
            if(Pieces[I, J].GetType() == typeof(King) && !(Pieces[I, J] as King)!.Moved)
            {
                if(Math.Abs(J - newJ) == 2)
                {
                    if(newJ > 3) // king side castle
                        Pieces[I, 7].Move(I, 5, Pieces);
                    else // queen side castle
                        Pieces[I, 0].Move(I, 3, Pieces);
                }
            }
            if (Pieces[newI, newJ] != null)
                Pieces[newI, newJ].isAlive = false;
            Pieces[I, J] = null!;
            I = newI;
            J = newJ;
            Pieces[I, J] = this;
        }
        #region IMAGES
        public readonly static ImageBrush[] Queen = new ImageBrush[]
        {
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/white-queen.png")) },
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/black-queen.png")) }
        };

        public readonly static ImageBrush[] King = new ImageBrush[]
        {
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/white-king.png")) },
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/black-king.png")) }
        };

        public readonly static ImageBrush[] Pawn = new ImageBrush[]
        {
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/white-pawn.png")) },
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/black-pawn.png")) }
        };

        public readonly static ImageBrush[] Knight = new ImageBrush[]
        {
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/white-knight.png")) },
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/black-knight.png")) }
        };

        public readonly static ImageBrush[] Bishop = new ImageBrush[]
        {
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/white-bishop.png")) },
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/black-bishop.png")) }
        };
        public readonly static ImageBrush[] Rook = new ImageBrush[]
        {
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/white-rook.png")) },
            new ImageBrush() { ImageSource = new BitmapImage(new Uri(@"ms-appx:///Pieces/black-rook.png")) }
        };
        #endregion
    }
}
