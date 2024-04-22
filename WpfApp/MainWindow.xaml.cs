using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tic_tac_toe_AI;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        GameWrapper gameWrapper = new GameWrapper();

        UniformGrid grid;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = gameWrapper;
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.RemoveRange(2, 1); // Removing wining line, do not try this approach at home

            MainGrid.ColumnDefinitions[0] = new ColumnDefinition { Width = new GridLength(MainW.ActualHeight - 39) };


            if (int.TryParse(BoardSizeTextBox.Text, out int boardSize))
            {
                if (boardSize <= 0) {
                    MessageBox.Show("Board size should be greater that 0");
                    return;
                }
                gameWrapper.boardSize = boardSize;
            }
            else
            {
                MessageBox.Show("Invalid input for Board size");
                return;
            }
            if (int.TryParse(WinLengthTextBox.Text, out int winLen))
            {
                if (winLen <= 0)
                {
                    MessageBox.Show("Win length should be greater that 0");
                    return;
                }
                if (winLen > boardSize)
                {
                    winLen = boardSize;
                    WinLengthTextBox.Text = winLen.ToString();
                }
                if (winLen == 1)
                {
                    MessageBox.Show("Really?");
                }
                gameWrapper.winLen = winLen;
            }
            else
            {
                MessageBox.Show("Invalid input for Win length");
                return;
            }
            if (int.TryParse(SearchTimeTextBox.Text, out int searchTime))
            {
                if (searchTime <= 0)
                {
                    MessageBox.Show("Search time should be greater that 0");
                    return;
                }
                gameWrapper.searchTime = searchTime;
            }
            else
            {
                MessageBox.Show("Invalid input for SearchTime");
                return;
            }
            gameWrapper.computerMovesFirst = (bool)ComputerMovesFirstCheckBox.IsChecked;
            gameWrapper.Reset();
            ProcessGameState();
            UpdateBoard();
        }

        private void UpdateBoard(int index = -1)
        {
            if (index >= 0)
            {
                grid.Children.RemoveRange(index, 1);
                grid.Children.Insert(index, getBoardElement(gameWrapper.board[index], gameWrapper.boardSize, index));
                return;
            }
            int n = gameWrapper.boardSize;
            grid = new UniformGrid();
            grid.Columns = n;
            grid.Rows = n;

            for (int i = 0; i < n*n; i++)
            {
                Border el = getBoardElement(gameWrapper.board[i], n, i);
                grid.Children.Add(el);
            }
            BoardPanel.Child = grid;
        }

        private Border getBoardElement(int mark, int n, int index)
        {
            Border border = new Border();
            border.Background = Brushes.White;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(1);

            Grid tile = new Grid();

            double size = (MainW.ActualHeight - 39) / n;
            if (mark == Game.X)
            {
                Line line1 = new Line
                {
                    X1 = size * 0.1,
                    Y1 = size * 0.1,
                    X2 = size * 0.9,
                    Y2 = size * 0.9,
                    Stroke = Brushes.Black,
                    StrokeThickness = Math.Floor(size * 0.1),
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                };
                Line line2 = new Line
                {
                    X1 = size * 0.1,
                    Y1 = size * 0.9,
                    X2 = size * 0.9,
                    Y2 = size * 0.1,
                    Stroke = Brushes.Black,
                    StrokeThickness = Math.Floor(size * 0.1),
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                };

                tile.Children.Add(line1);
                tile.Children.Add(line2);
            }
            else if (mark == Game.O) 
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = size * 0.9,
                    Height = size * 0.9,
                    Stroke = Brushes.Black,
                    StrokeThickness = Math.Floor(size * 0.1),
                };
                tile.Children.Add(ellipse);
            }
            else
            {
                Button button = new Button();
                button.BorderThickness = new Thickness(0);
                button.Background = Brushes.Transparent;
                button.Click += (sender, e) => ProcessMove(index);
                tile.Children.Add(button);
            }

            border.Child = tile;
            return border;
        }

        public void ProcessMove(int move)
        {
            try
            {
                if (gameWrapper.game.gameState() != Game.IN_PROGRESS) return;

                if (gameWrapper.computerToMove) return;

                if (gameWrapper.MoveIsValid(move))
                {
                    gameWrapper.ProcessHumanMove(move);
                }
                else
                {
                    return;
                }
                UpdateBoard(move);
                ProcessGameState();
                if (gameWrapper.game.gameState() != Game.IN_PROGRESS) return;

                gameWrapper.computerToMove = true;
                Task.Factory.StartNew(() => { gameWrapper.ProcessCompMove(); }).ContinueWith(t =>
                {
                    gameWrapper.computerToMove = false;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateBoard(gameWrapper.game.compMove);
                        ProcessGameState();
                    });

                });
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public void ProcessGameState() 
        {
            switch (gameWrapper.game.gameState())
            {
                case Game.X_WIN:
                    DrawWinningLine(Game.X);
                    MessageBox.Show("X WIN");
                    break;
                case Game.O_WIN:
                    DrawWinningLine(Game.O);
                    MessageBox.Show("O WIN");
                    break;
                case Game.DRAW:
                    MessageBox.Show("DRAW");
                    break;
                default: break;
            }

        }


        public void DrawWinningLine(int mark) 
        {
            (int start, int end) = gameWrapper.game.findWinningLine(mark);
            int n = gameWrapper.boardSize;
            double size = (MainW.ActualHeight - 39) / n;
            MainGrid.Children.Add(new Line
            {
                X1 = (start % n + 0.5) * size,
                Y1 = (start / n + 0.5) *size,
                X2 = (end % n + 0.5) *size,
                Y2 = (end / n + 0.5) * size,
                Stroke = Brushes.Red,
                StrokeThickness = 3,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
            });

        }

    }
}