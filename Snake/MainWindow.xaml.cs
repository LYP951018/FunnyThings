using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Windows.Threading;
using static Snake.SearchResource;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeGrid();
            InitializeSnake();
            InitializeGraph();
            
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            InitializeTimer();
            UpdateEgg();
        }

        private void InitializeSnake()
        {
            _snake = new EatingSnake(_graph);
        }

        private void InitializeGraph()
        {
            for (int i = 0; i < GridWidth; ++i)
            {
                _graph[0, i] = GridState.Wall;
                _graph[GridHeight - 1, i] = GridState.Wall;
            }
            for (int i = 0; i < GridHeight; ++i)
            {
                _graph[i, 0] = GridState.Wall;
                _graph[i, GridHeight - 1] = GridState.Wall;
            }
        }

        private void InitializeGrid()
        {
            mainGrid.Rows = GridWidth;
            mainGrid.Columns = GridHeight;

            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; ++j)
                {
                    var rect = new Rectangle();
                    rect.Fill = _gridBrush;
                    rect.Stroke = Brushes.Black;
                    rect.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rect.VerticalAlignment = VerticalAlignment.Stretch;
                    mainGrid.Children.Add(rect);
                }
            }
        }

        private void DrawSnake()
        {
            int index = 0;
            foreach (var state in _graph)
            {
                var rect = mainGrid.Children[index] as Rectangle;
                switch (state)
                {
                    case GridState.Snake:
                        if (index == ConvertToChildrenIndex(_snake.Head.X, _snake.Head.Y))
                        {
                            rect.Fill = Brushes.DarkBlue;
                        }
                        else rect.Fill = EatingSnake.SnakeBrush;
                        break;
                    case GridState.Nothing:
                        rect.Fill = _gridBrush;
                        break;
                    case GridState.Wall:
                        rect.Fill = Brushes.Black;
                        break;
                    case GridState.Egg:
                        rect.Fill = Brushes.LightGreen;
                        break;
                }
                ++index;
            }
        }

        private static int ConvertToChildrenIndex(int x, int y)
        {
            return x * GridWidth + y;
        }

       

        private bool UpdateEgg()
        {
            if (_snake.SnakeBody.Count == (GridWidth - 2) * (GridHeight - 2))
            {
                _eggPos = Nothing;          
                return true;
            }
            var x = RandomGen.Next(1, GridWidth - 1);
            var y = RandomGen.Next(1, GridHeight - 1);
            while (_snake.SnakeBody.Contains(new Coordinate(x, y)))
            {
                x = RandomGen.Next(1, GridWidth - 1);
                y = RandomGen.Next(1, GridHeight - 1);
            }
            _graph[x, y] = GridState.Egg;
            _eggPos.X = x;
            _eggPos.Y = y;
            _snake.EggPos = _eggPos;
            return false;
        }

        private void InitializeTimer()
        {
            _timer.Tick += OnTimer;
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Start();
        }

        
        private void OnTimer(object sender, EventArgs e)
        {
            var d = _snake.AutoMove();
            if(d == null)
            {
                _timer.Stop();
                return;
            }
            switch (d.Value)
            {
                //...
                case GridState.Egg:
                    if (UpdateEgg())
                    {
                        DrawSnake();
                        _timer.Stop();                       
                        return;
                    }                  
                    break;
                case GridState.Wall:
                case GridState.Snake:
                    throw new InvalidOperationException();
            }
            DrawSnake();
            //path.RemoveAt(path.Count - 1);
        }

       
        private Brush _gridBrush = Brushes.White;
        private GridState[,] _graph = new GridState[GridWidth, GridHeight];
        private EatingSnake _snake;
        
        private DispatcherTimer _timer = new DispatcherTimer();
        
        private Coordinate _eggPos = new Coordinate();


    }
}
