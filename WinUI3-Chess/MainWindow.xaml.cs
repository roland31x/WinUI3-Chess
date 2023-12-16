using Chess.PieceClasses;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3_Chess
{
    public sealed partial class MainWindow : Window
    {
        List<Piece> wpieces = new List<Piece>();
        List<Piece> bpieces = new List<Piece>();

        King WhiteKing => wpieces.OfType<King>().First();
        King BlackKing => bpieces.OfType<King>().First();

        Rectangle[,] labels = new Rectangle[8, 8];
        Rectangle[,] plabels = new Rectangle[8, 8];
        Ellipse[,] ebg = new Ellipse[8, 8];
        Rectangle[,] bg = new Rectangle[8, 8];

        Piece[,] Pieces = new Piece[8, 8];
        Piece? selected;
        List<int[]>? legalmoves;

        PieceColor _t = PieceColor.White;
        PieceColor Turn { get { return _t; } set { _t = value; if (_t == PieceColor.White) { WhiteTimerBG.Fill = new SolidColorBrush(Colors.Goldenrod); BlackTimerBG.Fill = new SolidColorBrush(Colors.White); whitetimer.Start(); blacktimer.Stop(); } else { BlackTimerBG.Fill = new SolidColorBrush(Colors.Goldenrod); WhiteTimerBG.Fill = new SolidColorBrush(Colors.White); blacktimer.Start(); whitetimer.Stop(); } } }

        bool WhiteEnpassantTurn = false;
        bool BlackEnpassantTurn = false;

        bool whiteincheck = false;
        bool blackincheck = false;
        bool calc = false;
        bool checkmate = false;

        Stopwatch whitetimer = new Stopwatch();
        Stopwatch blacktimer = new Stopwatch();
        DispatcherTimer timer = new DispatcherTimer();

        Piece? promotion;

        public MainWindow()
        {
            InitializeComponent();
            InitGame();
            UpdateUI();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 700));
            BaseGrid.SizeChanged += BaseGrid_SizeChanged;
            ExtendsContentIntoTitleBar = true;
        }

        private void BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newheight = (BaseGrid.ActualHeight < 700 ? 700 : BaseGrid.ActualHeight) - 100;
            double newcellheight = (double)newheight / 8d;
            foreach(Shape s in BoardGrid.Children.OfType<Shape>())
            {              
                s.Height = newcellheight;
                s.Width = newcellheight;
                if (s is Ellipse)
                {
                    s.Height *= 0.33;
                    s.Width *= 0.33;
                }
            }
            BoardBackground.Height = newheight + newheight / 60;
            BoardBackground.Width = newheight + newheight / 60;
        }

        private void Timer_Tick(object? sender, object e)
        {
            BlackTimerRectangle.Text = blacktimer.Elapsed.ToString(@"mm\:ss");
            WhiteTimerRectangle.Text = whitetimer.Elapsed.ToString(@"mm\:ss");
        }
        void UpdateUI()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ebg[i, j].Fill = new SolidColorBrush(Colors.Transparent);
                    bg[i, j].Fill = new SolidColorBrush(Colors.Transparent);
                    if (Pieces[i, j] != null)
                    {
                        plabels[i, j].Fill = Pieces[i, j].Body;
                        labels[i, j].Tag = Pieces[i, j];
                    }
                    else
                    {
                        plabels[i, j].Fill = new SolidColorBrush(Colors.Transparent);
                        labels[i, j].Tag = null;
                    }

                }
            }
            if (selected != null)
                bg[selected.I, selected.J].Fill = new SolidColorBrush(Colors.LimeGreen);
            if (legalmoves != null)
                foreach (int[] p in legalmoves)
                    ebg[p[0], p[1]].Fill = new SolidColorBrush(Colors.LimeGreen);
            if (whiteincheck)
                bg[wpieces.OfType<King>().First().I, wpieces.OfType<King>().First().J].Fill = new SolidColorBrush(Colors.Orange);
            if (blackincheck)
                bg[bpieces.OfType<King>().First().I, bpieces.OfType<King>().First().J].Fill = new SolidColorBrush(Colors.Orange);
            BlackCaptures.Children.Clear();
            foreach (Piece p in bpieces.Where(x => !x.isAlive))
                BlackCaptures.Children.Add(new Rectangle() { Height = 32, Width = 32, Fill = p.Body });
            WhiteCaptures.Children.Clear();
            foreach (Piece p in wpieces.Where(x => !x.isAlive))
                WhiteCaptures.Children.Add(new Rectangle() { Height = 32, Width = 32, Fill = p.Body });

        }
        public void ResetStateTo(Piece[,] state)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Pieces[i, j] = state[i, j];
                    if (state[i, j] != null)
                    {
                        Pieces[i, j].I = i;
                        Pieces[i, j].J = j;
                        Pieces[i, j].isAlive = true;
                    }
                }
            }
        }
        void ResetGame()
        {
            Turn = PieceColor.White;
            WhiteEnpassantTurn = false;
            BlackEnpassantTurn = false;
            whiteincheck = false;
            blackincheck = false;
            calc = false;
            checkmate = false;
            selected = null;
            legalmoves = null;
            whitetimer.Reset();
            blacktimer.Reset();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    Pieces[i, j] = null!;
            wpieces.Clear();
            bpieces.Clear();
            LoadDefaultPositionPieces();
            UpdateUI();
        }
        void InitGame()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Rectangle l = new Rectangle();
                    l.Height = 100;
                    l.Width = 100;
                    l.Fill = (i + j) % 2 == 0 ? new SolidColorBrush(Colors.Beige) : new SolidColorBrush(Colors.BurlyWood);
                    BoardGrid.Children.Add(l);
                    Grid.SetRow(l, i);
                    Grid.SetColumn(l, j);

                    Rectangle l3 = new Rectangle();
                    l3.Height = 100;
                    l3.Width = 100;
                    l3.Opacity = 0.66;
                    BoardGrid.Children.Add(l3);
                    Grid.SetRow(l3, i);
                    Grid.SetColumn(l3, j);
                    bg[i, j] = l3;

                    Rectangle p = new Rectangle();
                    p.Height = 100;
                    p.Width = 100;
                    BoardGrid.Children.Add(p);
                    Grid.SetRow(p, i);
                    Grid.SetColumn(p, j);
                    plabels[i, j] = p;

                    Ellipse l2 = new Ellipse();
                    l2.Height = 100 * 0.33;
                    l2.Width = 100 * 0.33;
                    l2.Opacity = 0.66;
                    BoardGrid.Children.Add(l2);
                    Grid.SetRow(l2, i);
                    Grid.SetColumn(l2, j);
                    ebg[i, j] = l2;

                    Rectangle p2 = new Rectangle();
                    p2.Height = 100;
                    p2.Width = 100;
                    p2.PointerEntered += P_MouseEnter;
                    p2.PointerExited += P_MouseLeave;
                    p2.Fill = new SolidColorBrush(Colors.Transparent);
                    p2.PointerPressed += PieceSelect;
                    BoardGrid.Children.Add(p2);
                    Grid.SetRow(p2, i);
                    Grid.SetColumn(p2, j);
                    labels[i, j] = p2;
                }
            }
            LoadDefaultPositionPieces();

        }
        void LoadDefaultPositionPieces()
        {
            Queen whiteq = new Queen(PieceColor.White, 7, 3);
            Queen blackq = new Queen(PieceColor.Black, 0, 3);
            wpieces.Add(whiteq);
            bpieces.Add(blackq);

            Rook whiter1 = new Rook(PieceColor.White, 7, 0);
            Rook blackr1 = new Rook(PieceColor.Black, 0, 0);
            wpieces.Add(whiter1);
            bpieces.Add(blackr1);

            Rook whiter2 = new Rook(PieceColor.White, 7, 7);
            Rook blackr2 = new Rook(PieceColor.Black, 0, 7);
            wpieces.Add(whiter2);
            bpieces.Add(blackr2);

            Bishop whiteb1 = new Bishop(PieceColor.White, 7, 2);
            Bishop blackb1 = new Bishop(PieceColor.Black, 0, 2);
            wpieces.Add(whiteb1);
            bpieces.Add(blackb1);

            Bishop whiteb2 = new Bishop(PieceColor.White, 7, 5);
            Bishop blackb2 = new Bishop(PieceColor.Black, 0, 5);
            wpieces.Add(whiteb2);
            bpieces.Add(blackb2);

            King whiteking = new King(PieceColor.White, 7, 4);
            King blackking = new King(PieceColor.Black, 0, 4);
            wpieces.Add(whiteking);
            bpieces.Add(blackking);

            Knight whitek1 = new Knight(PieceColor.White, 7, 1);
            Knight blackk1 = new Knight(PieceColor.Black, 0, 1);
            wpieces.Add(whitek1);
            bpieces.Add(blackk1);

            Knight whitek2 = new Knight(PieceColor.White, 7, 6);
            Knight blackk2 = new Knight(PieceColor.Black, 0, 6);
            wpieces.Add(whitek2);
            bpieces.Add(blackk2);

            for (int i = 0; i < 8; i++)
            {
                Pawn white = new Pawn(PieceColor.White, 6, i);
                Pawn black = new Pawn(PieceColor.Black, 1, i);
                wpieces.Add(white);
                bpieces.Add(black);
            }
            foreach (Piece p in wpieces.Union(bpieces))
                Pieces[p.I, p.J] = p;
        }
        private async void PieceSelect(object sender, PointerRoutedEventArgs e)
        {
            if (calc || checkmate)
                return;
            Rectangle l = (Rectangle)sender;
            if (selected == null)
            {
                if (l.Tag != null && (l.Tag as Piece)!.Color == Turn)
                {
                    Piece selection = (Piece)((sender as Rectangle)!.Tag);
                    selected = selection;
                    legalmoves = selection.PieceMoves(true, Pieces);
                }
            }
            else
            {
                calc = true;
                bool okmove = false;
                foreach (int[] m in legalmoves!)
                {
                    if (l == labels[m[0], m[1]])
                    {
                        int starti = selected.I;
                        int startj = selected.J;

                        Piece[,] savestate = GetState(Pieces);
                        selected.Move(m[0], m[1], Pieces);
                        bool legal = await LegalMove(Turn);
                        if (!legal)
                        {
                            ResetStateTo(savestate);
                            (int flashi, int flashj) = Turn == PieceColor.White ? (WhiteKing.I, WhiteKing.J) : (BlackKing.I, BlackKing.J);
                            await FlashAnimation(flashi,flashj);
                        }
                        else
                        {
                            okmove = true;
                            selected.Moved = true;
                            InvalidateEnpassantTurns();
                            if (selected.GetType() == typeof(Pawn))
                            {
                                CheckEnpassant((Pawn)selected, starti);
                                await CheckPromotion(selected);
                            }

                        }

                        await CheckForCheck();
                        if (whiteincheck)
                            await Task.Run(() => CheckForMate(PieceColor.White));
                        if (blackincheck)
                            await Task.Run(() => CheckForMate(PieceColor.Black));

                        if (checkmate)
                        {
                            selected = null;
                            legalmoves = null;
                            UpdateUI();
                            ShowEndGame();
                            return;
                        }
                            
                        break;
                    }
                }
                if (okmove)
                    Turn = Turn == PieceColor.White ? PieceColor.Black : PieceColor.White;
                selected = null;
                legalmoves = null;
            }
            UpdateUI();
            calc = false;
        }
        public void ShowEndGame()
        {
            whitetimer.Stop();
            blacktimer.Stop();
            Flyout f = new Flyout();
            string winner = Turn == PieceColor.White ? "WHITE" : "BLACK";
            f.Content = new TextBlock() { Text = $"CHECK MATE! {winner} WINS!" };
            f.ShowAt(BaseGrid, new FlyoutShowOptions() { Position = new Point(BaseGrid.ActualWidth / 2, BaseGrid.ActualHeight / 2) });          
        }
        async Task FlashAnimation(int I, int J)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 1000)
            {
                if ((sw.ElapsedMilliseconds / 100) % 2 == 0)
                    bg[I, J].Fill = new SolidColorBrush(Colors.Red);
                else                    
                    bg[I, J].Fill = new SolidColorBrush(Colors.Orange);
                await Task.Delay(30);
            }
            sw.Stop();
        }
        async Task<bool> LegalMove(PieceColor c)
        {
            await Task.Run(CheckForCheck);

            if (c == PieceColor.Black && blackincheck)
                return false;
            if (c == PieceColor.White && whiteincheck)
                return false;
            return true;
        }
        Piece[,] GetState(Piece[,] p)
        {
            Piece[,] tor = new Piece[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    tor[i, j] = p[i, j];
                }
            }
            return tor;
        }
        async Task CheckForMate(PieceColor c)
        {
            Piece[,] save = GetState(Pieces);
            bool mate = true;
            foreach (Piece p in wpieces.Union(bpieces).Where(x => x.isAlive && x.Color == c))
            {
                List<int[]> legal = p.PieceMoves(false, Pieces);
                foreach (int[] l in legal)
                {
                    p.Move(l[0], l[1], Pieces);
                    await CheckForCheck();
                    if ((whiteincheck && c == PieceColor.White) || (blackincheck && c == PieceColor.Black))
                    {
                        ResetStateTo(save);
                        continue;
                    }
                    else
                    {
                        mate = false;
                        break;
                    }

                }
                if (!mate)
                    break;
            }
            if (mate)
                checkmate = true;

            ResetStateTo(save);
            await CheckForCheck();           
        }
        Task CheckForCheck()
        {
            whiteincheck = false;
            blackincheck = false;
            Piece targetking = bpieces.OfType<King>().First();
            foreach (Piece p in wpieces.Where(x => x.isAlive))
            {
                List<int[]> legal = p.PieceMoves(false, Pieces);
                foreach (int[] l in legal)
                    if (l[0] == targetking.I && l[1] == targetking.J)
                        blackincheck = true;
            }

            targetking = wpieces.OfType<King>().First();
            foreach (Piece p in bpieces.Where(x => x.isAlive))
            {
                List<int[]> legal = p.PieceMoves(false, Pieces);
                foreach (int[] l in legal)
                    if (l[0] == targetking.I && l[1] == targetking.J)
                        whiteincheck = true;
            }

            return Task.CompletedTask;
        }
        void CheckEnpassant(Pawn selected, int starti)
        {
            if (Math.Abs(starti - selected.I) == 2)
            {
                int leftj = selected.J - 1;
                int rightj = selected.J + 1;
                if (leftj > 0 && leftj < 8 && Pieces[selected.I, leftj] != null)
                    if (Pieces[selected.I, leftj].GetType() == typeof(Pawn) && Pieces[selected.I, leftj].Color != selected.Color)
                    {
                        (Pieces[selected.I, leftj] as Pawn)!.EnpassantPos = new int[] { selected.I - (selected.Color == PieceColor.White ? -1 : 1), selected.J };
                        if (Turn == PieceColor.White)
                            BlackEnpassantTurn = true;
                        else
                            WhiteEnpassantTurn = true;
                    }

                if (rightj > 0 && rightj < 8 && Pieces[selected.I, rightj] != null)
                    if (Pieces[selected.I, rightj].GetType() == typeof(Pawn) && Pieces[selected.I, rightj].Color != selected.Color)
                    {
                        (Pieces[selected.I, rightj] as Pawn)!.EnpassantPos = new int[] { selected.I - (selected.Color == PieceColor.White ? -1 : 1), selected.J };
                        if (Turn == PieceColor.White)
                            BlackEnpassantTurn = true;
                        else
                            WhiteEnpassantTurn = true;
                    }
            }
        }
        void InvalidateEnpassantTurns()
        {
            if (BlackEnpassantTurn)
            {
                foreach (Pawn p in bpieces.OfType<Pawn>())
                    p.EnpassantPos = null;
                BlackEnpassantTurn = false;
            }
            if (WhiteEnpassantTurn)
            {
                foreach (Pawn p in wpieces.OfType<Pawn>())
                    p.EnpassantPos = null;
                WhiteEnpassantTurn = false;
            }
        }
        private async Task CheckPromotion(Piece topromote)
        {
            if (topromote.Color == PieceColor.White && topromote.I != 0)
                return;
            if (topromote.Color == PieceColor.Black && topromote.I != 7)
                return;

            Piece promoted = await PromotionBoxResult(topromote.Color);
            promoted.I = topromote.I;
            promoted.J = topromote.J;
            if (topromote.Color == PieceColor.White)
            {
                wpieces.Remove(topromote);
                wpieces.Add(promoted);
            }
            else
            {
                bpieces.Remove(topromote);
                bpieces.Add(promoted);
            }
            Pieces[topromote.I, topromote.J] = promoted;
        }
        async Task<Piece> PromotionBoxResult(PieceColor color)
        {
            Rectangle overlay = new Rectangle() { Fill = new SolidColorBrush(Colors.White), Opacity = 0.33 };
            BaseGrid.Children.Add(overlay);
            Canvas.SetZIndex(overlay, 10);

            PromotionCanvas.Visibility = Visibility.Visible;
            Canvas.SetZIndex(PromotionCanvas, 100);

            int c = (int)color;
            QueenPromotion.Fill = Piece.Queen[c];
            KnightPromotion.Fill = Piece.Knight[c];
            BishopPromotion.Fill = Piece.Bishop[c];
            RookPromotion.Fill = Piece.Rook[c];

            while (promotion == null)
                await Task.Delay(100);

            Piece toreturn = promotion;

            promotion = null;

            BaseGrid.Children.Remove(overlay);
            PromotionCanvas.Visibility = Visibility.Collapsed;
            return toreturn;
        }
        private void RookPromotion_Click(object sender, PointerRoutedEventArgs e) => promotion = new Rook(Turn, 0, 0);
        private void QueenPromotion_Click(object sender, PointerRoutedEventArgs e) => promotion = new Queen(Turn, 0, 0);
        private void KnightPromotion_Click(object sender, PointerRoutedEventArgs e) => promotion = new Knight(Turn, 0, 0);
        private void BishopPromotion_Click(object sender, PointerRoutedEventArgs e) => promotion = new Bishop(Turn, 0, 0);
        private void P_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            BaseGrid.ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Arrow));
        }

        private void P_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            if (checkmate)
                return;
            Rectangle l = (Rectangle)sender;
            if (selected == null)
            {
                if (l.Tag != null && (l.Tag as Piece)!.Color == Turn)
                    BaseGrid.ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Hand));
            }
            else
            {
                foreach (int[] m in legalmoves!)
                {
                    if (l == labels[m[0], m[1]])
                    {
                        BaseGrid.ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Hand));
                        return;
                    }
                }
            }
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e) => ResetGame();

        private void Rectangle_MouseEnter(object sender, PointerRoutedEventArgs e) => BaseGrid.ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Hand));

        private void Rectangle_MouseLeave(object sender, PointerRoutedEventArgs e) => BaseGrid.ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Arrow));
    }
    public sealed partial class BaseGrid : Grid
    {
        public void ChangeCursor(InputSystemCursor cursor)
        {
            this.ProtectedCursor = cursor;
        }
    }
}
