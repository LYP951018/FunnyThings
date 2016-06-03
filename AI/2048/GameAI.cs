using _2048.Model;
using System;

namespace _2048
{
    public struct SearchResultInfo
    {
        public Direction? MoveDirection
        {
            get;
            set;
        }
        public int Score { get; set; }
    }

    static class GameAI
    {
        static readonly Direction[] Directions = new Direction[] { Direction.Right, Direction.Left, Direction.Down, Direction.Up};
        //static Dictionary<Game, SearchResultInfo> _games = new Dictionary<Game, SearchResultInfo>();
        public static unsafe SearchResultInfo Search(uint depth, bool isMax, Game theGame, int alpha, int beta)
        {
            Game.Logger($"-----------------Start searching depth = {depth} alpha = {alpha}------------------");
            theGame.LogGrid();
            if (isMax)
            {
                if (depth == 0)
                {
                    Game.Logger($"Depth = 0, score : {Evaluate(theGame, depth)}");
                    return new SearchResultInfo
                    {
                        Score = Evaluate(theGame, depth)
                    };                   
                }
                var newGame = new Game(theGame);
                Direction? bestMove = null;
                foreach (var direction in Directions)
                {
                    Game.Logger($"Searching {direction}");
                    //var game = new Game(theGame);
                    var res = newGame.Update(direction);
                    if (res.HasMoved)
                    {                       
                        if (res.HasWon)
                            return new SearchResultInfo
                            {
                                Score = int.MaxValue,
                                MoveDirection = direction
                            };
                        else
                        {
                            Game.Logger("Searching down by max.");
                            var result = Search(depth - 1, false, newGame, alpha, beta);
                            if (result.Score > alpha)
                            {
                                bestMove = direction;
                                alpha = result.Score;
                                if (alpha >= beta)
                                    break;
                            }
                        }                                           
                    }
                    newGame.Reset(theGame);
                }
                Game.Logger($"result = {alpha} {bestMove}");
                return new SearchResultInfo
                {
                    Score = alpha,
                    MoveDirection = bestMove
                };               
            }
            else
            {
                var bitmap = theGame.GetBitmap();
                var ptr = stackalloc int[48];
                var cellScores = new StaticList(ptr);
                var emptyCells = new StaticList(ptr + 16);
                int lowestScore = int.MaxValue;
                for (int i = 0; i < 16; ++i)
                {
                    ushort mask = (ushort)(1 << i);
                    if((bitmap & mask) == 0)
                    {
                        var x = i / 4;
                        var y = i % 4;
                        theGame.AddCellTrivial(x, y, 1);
                        var score = theGame.Smoothness() - theGame.Dispersion();
                        Game.Logger($"Add cell at {x}, {y} gets score {score}, Smooth = {theGame.Smoothness()}, Monotonic = {theGame.Monotonicity()} Dispersion = {theGame.Dispersion()}");
                        theGame.LogGrid();
                        cellScores.Add(score);
                        lowestScore = Math.Min(score, lowestScore);
                        theGame.RemoveCellTrivial(x, y);
                        emptyCells.Add(i);
                    }
                }

                var lowestCells = new StaticList(ptr + 32);
                for (int i = 0; i < emptyCells.Count; ++i)
                {
                    if (cellScores[i] == lowestScore)
                        lowestCells.Add(emptyCells[i]);
                }
                for(int i = 0; i < lowestCells.Count; ++i)
                {
                    var cell = lowestCells[i];
                    Game.Logger($"Add cell at {cell}, score is {lowestScore}");
                    var x = cell / 4;
                    var y = cell % 4;
                    theGame.AddCellTrivial(x, y, 1);
                    Game.Logger("Searching down by min.");
                    var result = Search(depth, true, theGame, alpha, beta);
                    theGame.RemoveCellTrivial(x, y);
                    if (result.Score < beta)
                    {
                        beta = result.Score;
                        Game.Logger($"Beta updated {beta}");
                        if (alpha >= beta)
                        {
                            Game.Logger("Cutoff");
                            break;
                        }
                    }
                }
                return new SearchResultInfo
                {
                    Score = beta,
                    MoveDirection = null
                };                
            }
        }

        static int Evaluate(Game game, uint depth)
        {
            var emptyCount = game.EmptyNumberCount();
            var smoothness = game.Smoothness();
            var monotonicity = game.Monotonicity();
            var maxNumber = game.MaxNumber();
            //if (emptyCount == 0)
            //    return smoothness + monotonicity * 3 + maxNumber * 10 + emptyCount * 50;
            return smoothness + monotonicity * 10 + maxNumber * 10 + emptyCount * 25; /*+ game.Coherence() * 2*///- (int)depth;
        }
    }
}
