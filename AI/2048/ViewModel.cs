using System.Collections.ObjectModel;
using _2048.Model;
using static System.Linq.Enumerable;

namespace _2048
{
    class ViewModel
    {
        private ObservableCollection<uint> _cells = new ObservableCollection<uint>();
        public ObservableCollection<uint> Boards => _cells;
        private Game _game = new Game();
        public Game TheGame => _game;
        private bool _hasMoved;
        private bool _hasWon;
        public bool HasMoved => _hasMoved;
        public bool HasWon => _hasWon;

        public ViewModel()
        {
            foreach (var _ in Range(0, 16))
                _cells.Add(0);
        }

        public void PreUpdate(TransformInfo[,] transformations)
        {
            foreach(var i in Range(0, transformations.GetLength(0)))
                foreach(var j in Range(0, transformations.GetLength(1)))
                {
                    var des = transformations[i, j].Destination;
                    if (des != Coordinate.Nowhere)
                        _cells[new Coordinate(i, j).ToIndex()] = 0;
                }
        }

        public void Update()
        {
            if (_hasMoved)
                GenerateEgg();
            int i = 0;
            foreach (var x in TheGame.TraverseNumbers())
                Boards[i++] = x;
        }

        public void GenerateEgg()
        {
            var egg = TheGame.GenerateEgg();
            _cells[egg.ToIndex()] = 2;
        }

        public TransformInfo[,] Move(Direction direction)
        {
            var res = TheGame.Update(direction);
            _hasWon = res.HasWon;
            _hasMoved = res.HasMoved;
            return TheGame.Transformations;
        }        
    }
}
