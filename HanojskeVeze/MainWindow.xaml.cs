using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HanojskeVeze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Variables
        private readonly Dictionary<int, Stack<int>> towers = new();
        private int discs;
        private Rectangle dragged;
        private Point draggingPoint;
        private int fromTower;
        private int toTower;
        private bool finished;
        private int moveCount;
        private int lvl;


        //Design values
        Color discColor = Color.FromRgb(83, 166, 45);
        Color discColorMoved = Color.FromRgb(156, 217, 24);
        Color frameColor = Color.FromRgb(112, 115, 101);
        Color backgroundColor = Color.FromRgb(234, 242, 206);
        private const int frameThickness = 2;
        
        public MainWindow()
        {
            InitializeComponent();
            //Set starting number of discs
            discs = 3;

            //Set starting level
            lvl = 1;

            platno.Background = new SolidColorBrush(backgroundColor);
            moves.Foreground = new SolidColorBrush(frameColor);
            level.Foreground = new SolidColorBrush(frameColor);
        }
        private void RedrawTowers()
        {
            canvas.Children.Clear();

            int actualTower = 0;
            double horizontalPadding = 10;
            foreach (Stack<int> tower in towers.Values)
            {
                int row = discs - tower.Count;
                foreach (int disc in tower)
                {
                    double padding = (canvas.ActualWidth / 3 / discs * (discs - disc)) + 20;
                    double discHeight = (canvas.ActualHeight / (discs + 1)) - horizontalPadding;
                    Rectangle rect = new()
                    {
                        Width = (canvas.ActualWidth / 3) - padding,
                        Height = discHeight,
                        RadiusX = discHeight / 3,
                        RadiusY = discHeight / 3,
                        Fill = new SolidColorBrush(discColor)
                    };
                    Canvas.SetTop(rect, discHeight + (horizontalPadding + discHeight) * row);
                    Canvas.SetLeft(rect, padding / 2 + (canvas.ActualWidth / 3 * actualTower));
                    canvas.Children.Add(rect);
                    row++;
                    if (row == discs - tower.Count + 1)
                    {
                        rect.MouseMove += Disc_MouseMove;
                        rect.MouseDown += Disc_MouseDown;
                    }
                }
                actualTower++;
            }
            Line line1 = new()
            {
                StrokeThickness = frameThickness,
                Stroke = new SolidColorBrush(frameColor),
                X1 = canvas.ActualWidth / 3,
                X2 = canvas.ActualWidth / 3,
                Y1 = 0,
                Y2 = canvas.ActualHeight
            };
            Line line2 = new()
            {
                StrokeThickness = frameThickness,
                Stroke = new SolidColorBrush(frameColor),
                X1 = canvas.ActualWidth / 3 * 2,
                X2 = canvas.ActualWidth / 3 * 2,
                Y1 = 0,
                Y2 = canvas.ActualHeight
            };
            Line line3 = new()
            {
                StrokeThickness = frameThickness,
                Stroke = new SolidColorBrush(frameColor),
                X1 = 0,
                X2 = canvas.ActualWidth,
                Y1 = 0,
                Y2 = 0
            };

            canvas.Children.Add(line1);
            canvas.Children.Add(line2);
            canvas.Children.Add(line3);


            moves.Content = $"Počet tahů: {moveCount}";
            level.Content = $"{lvl}. úroveň";

            //If done continue to next level
            if (finished)
            {
                //Screen that you successfully moved all disck
                MessageBox.Show("Vyhrál jsi.","Výhra",MessageBoxButton.OK,MessageBoxImage.Information);

                discs++;
                lvl++;
                Start();
            }
        }

        private void Disc_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Check if disc is dragged
            if (e.LeftButton == MouseButtonState.Pressed && sender is Rectangle)
            {
                //Get dragging position
                Point mousePosition = e.GetPosition(canvas);

                //Get the position of the tower from which the disc is moved
                fromTower = Convert.ToInt32(Math.Ceiling(mousePosition.X / (canvas.ActualWidth / 3)));
            }
        }
        private void Disc_MouseMove(object sender, MouseEventArgs e)
        {
            //Check if disc is dragged
            if(e.LeftButton == MouseButtonState.Pressed && sender is Rectangle)
            {
                //Set var dragged to actual dragged disc
                dragged = sender as Rectangle;

                //Set color of dragged disc
                dragged.Fill = new SolidColorBrush(discColorMoved);

                //Set dragging point
                draggingPoint = e.GetPosition(dragged);
                
                //Do drag & drop
                DragDrop.DoDragDrop(dragged, dragged, DragDropEffects.Move);
            }
                
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Redraw towers after main window size changed
            RedrawTowers();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Start game after fully loaded window
            Start();
        }
        private void DiscDrop(object sender, DragEventArgs e)
        {
            //Get actual mouse position on canvas
            Point mousePosition = e.GetPosition(canvas);

            //Check if dropped item comes from canvas (if is it disc)
            if (sender is Canvas)
            {
                toTower = Convert.ToInt32(Math.Ceiling(mousePosition.X / (canvas.ActualWidth / 3)));
                if(toTower < 1)
                    toTower = 1;
                if(toTower > 3)
                    toTower = 3;
                if (towers[fromTower].Count > 0)
                {
                    if (towers[toTower].Count == 0)
                    {
                        int presouvanyKotouc = towers[fromTower].Pop();
                        towers[toTower].Push(presouvanyKotouc);
                        moveCount++;
                    }
                    else if (towers[fromTower].Peek() < towers[toTower].Peek())
                    {
                        int presouvanyKotouc = towers[fromTower].Pop();
                        towers[toTower].Push(presouvanyKotouc);
                        moveCount++;
                    }
                }
                if (towers[2].Count == discs || towers[3].Count == discs)
                {
                    finished = true;
                }
            }

            //Draw towers
            RedrawTowers();
        }

        private void DiscDragOver(object sender, DragEventArgs e)
        {
            //Get actual mouse position on canvas
            Point mousePosition = e.GetPosition(canvas);

            //Set mouse disc position
            double moveY = mousePosition.Y - draggingPoint.Y;
            double moveX = mousePosition.X - draggingPoint.X;

            //Chech if disc is not out of canvas
            if(moveY > 0 && (moveY + dragged.Height) <= canvas.ActualHeight)
                Canvas.SetTop(dragged, moveY);
            if (moveX > 0 && (moveX + dragged.Width) < canvas.ActualWidth)
                Canvas.SetLeft(dragged, moveX);
        }
        private void Start()
        {
            //Set variables
            moveCount = 0;
            finished = false;
            towers[1] = new Stack<int>();
            towers[2] = new Stack<int>();
            towers[3] = new Stack<int>();

            //Fill 1. tower
            for (int i = discs; i >= 1; i--)
            {
                towers[1].Push(i);
            }

            //Draw towers
            RedrawTowers();
        }
    }
    
}