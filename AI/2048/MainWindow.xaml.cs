using _2048.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static System.Linq.Enumerable;

namespace _2048
{
    public static class StoryBoardExtension
    {
        public static Task BeginAsync(this Storyboard storyboard, FrameworkElement window)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;
                onComplete = (s, e) => {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                storyboard.Completed += onComplete;
                storyboard.Begin(window);
            }
            return tcs.Task;
        }

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GameBoard.ItemsSource = _viewModel.Boards;
            NameScope.SetNameScope(this, new NameScope());
            _viewModel.GenerateEgg();
            _viewModel.GenerateEgg();
        }

        private ViewModel _viewModel = new ViewModel();
        private Grid[,] _grids = new Grid[Game.BoardHeight, Game.BoardWidth];
        private UniformGrid _cellsContainer = null;
        private List<Grid> gridsHaveTempCell = new List<Grid>();
        private List<string> transformNames = new List<string>();
        private Storyboard storyBoard = new Storyboard();

        public async Task PlayAnimation(TransformInfo[,] transformations)
        {
            #region
            if (_cellsContainer == null)
            {
                _cellsContainer = GetItemsPanel(GameBoard) as UniformGrid;
                Debug.Assert(_cellsContainer != null);
                foreach (var i in Range(0, Game.BoardHeight))
                    foreach (var j in Range(0, Game.BoardWidth))
                    {
                        var content = _cellsContainer.Children[new Coordinate(i, j).ToIndex()] as ContentPresenter;
                        _grids[i, j] = content.ContentTemplate.FindName("CellGrid", content) as Grid;
                        Debug.Assert(_grids[i, j] != null);
                    }
            }
            #endregion
            storyBoard.Children.Clear();
            foreach (var i in Range(0, transformations.GetLength(0)))
                foreach (var j in Range(0, transformations.GetLength(1)))
                {
                    if (transformations[i, j].Destination != Coordinate.Nowhere)
                    {
                        var des = transformations[i, j].Destination;
                        var wasNew = transformations[i, j].WasNew;
                        var tempCell = new GridCell();
                        tempCell.Number = transformations[i, j].PreviousNumber;
                        var srcGrid = _grids[i, j];
                        gridsHaveTempCell.Add(srcGrid);
                        srcGrid.Children.Add(tempCell);
                        var desGrid = _grids.GetValue(des);
                        var srcPos = srcGrid.PointToScreen(new Point(0d, 0d));
                        var desPos = desGrid.PointToScreen(new Point(0d, 0d));
                        var xDist = desPos.X - srcPos.X;
                        var yDist = desPos.Y - srcPos.Y;
                        var xAnimation = new DoubleAnimation(xDist, new Duration(new TimeSpan(0, 0, 0, 0, Math.Abs((int)(xDist)))));
                        var yAnimation = new DoubleAnimation(yDist, new Duration(new TimeSpan(0, 0, 0, 0, Math.Abs((int)(yDist)))));                        
                        var translate = new TranslateTransform();
                        tempCell.RenderTransform = translate;
                        var transformName = $"TranslateTransform{i}{j}";
                        transformNames.Add(transformName);
                        RegisterName(transformName, translate);
                        Storyboard.SetTargetName(xAnimation, transformName);
                        Storyboard.SetTargetName(yAnimation, transformName);
                        Storyboard.SetTargetProperty(xAnimation, new PropertyPath(TranslateTransform.XProperty));
                        Storyboard.SetTargetProperty(yAnimation, new PropertyPath(TranslateTransform.YProperty));
                        storyBoard.Children.Add(xAnimation);
                        storyBoard.Children.Add(yAnimation);
                    }
                }
            //将动画的起点全部归 0。
            _viewModel.PreUpdate(transformations);
            await storyBoard.BeginAsync(this);
            //动画播放完毕后，更新。
            _viewModel.Update();
            storyBoard.Children.Clear();
            ProcessEggAnimation(transformations);
            //清理掉新的 GridCell。
            foreach (Grid grid in gridsHaveTempCell)
            {
                var children = grid.Children;
                children.RemoveRange(1, children.Count - 1);
            }
            foreach (var name in transformNames)
                UnregisterName(name);
            transformNames.Clear();
            gridsHaveTempCell.Clear();
        }

        public async void ProcessEggAnimation(TransformInfo[,] transformations)
        {
            //对于每一个 WasNew == true 的 cell，要播放放大动画。
            foreach (var i in Range(0, transformations.GetLength(0)))
                foreach (var j in Range(0, transformations.GetLength(1)))
                    if (transformations[i, j].WasNew)
                    {
                        var cell = _grids[i, j].Children[0];
                        var duration = new TimeSpan(0, 0, 0, 0, 100);
                        var animation = new DoubleAnimation(1.5, duration);
                        animation.AutoReverse = true;
                        var animation2 = new DoubleAnimation(1.5, duration);
                        animation2.AutoReverse = true;
                        var transformName = $"ScaleTransform{i}{j}";
                        var scale = new ScaleTransform(1.0, 1.0);
                        cell.RenderTransformOrigin = new Point(0.5, 0.5);
                        transformNames.Add(transformName);
                        RegisterName(transformName, scale);
                        Storyboard.SetTargetName(animation, transformName);
                        Storyboard.SetTargetName(animation2, transformName);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(ScaleTransform.ScaleXProperty));
                        Storyboard.SetTargetProperty(animation2, new PropertyPath(ScaleTransform.ScaleYProperty));
                        cell.RenderTransform = scale;
                        storyBoard.Children.Add(animation);
                        storyBoard.Children.Add(animation2);
                    }
            await storyBoard.BeginAsync(this);
        }

        private Panel GetItemsPanel(DependencyObject itemsControl)
        {
            ItemsPresenter itemsPresenter = GetVisualChild<ItemsPresenter>(itemsControl);
            Panel itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
            return itemsPanel;
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public /*async */void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Direction direction;
            //switch (e.Key)
            //{
            //    case Key.Right:
            //        direction = Direction.Right;
            //        break;
            //    case Key.Left:
            //        direction = Direction.Left;
            //        break;
            //    case Key.Down:
            //        direction = Direction.Down;
            //        break;
            //    case Key.Up:
            //        direction = Direction.Up;
            //        break;
            //    default:
            //        return;
            //}
            //var transformations = _viewModel.Move(direction);
            //await PlayAnimation(transformations);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchResultInfo result = new SearchResultInfo();
            Direction? direction = null;
            while (true)
            {                
                await Task.Run(() =>
                {
                    var current = DateTime.Now;
                    uint depth = 1;
                    do
                    {
                        Game.Logger("---------------Yet another search.-------------------");
                        result = GameAI.Search(depth, true, _viewModel.TheGame, int.MinValue, int.MaxValue);
                        if (result.MoveDirection.HasValue)
                            direction = result.MoveDirection.Value;
                        else continue;
                        ++depth;
                    } while ((DateTime.Now - current).TotalSeconds <= 1.0);
                });
                Debug.Assert(direction.HasValue);
                var transformations = _viewModel.Move(direction.Value);
                if (_viewModel.HasWon)
                    return;
                if (!_viewModel.HasMoved)
                    Debug.Assert(false);
                await PlayAnimation(transformations);
            }           
        }
    }
}
