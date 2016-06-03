using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _2048.Model;
using System.Diagnostics;
using _2048;

namespace Game2048Test
{
    [TestClass]
    public class GameTest
    {
        //[TestMethod]
        //public void TestMoveDownTrivial()
        //{
        //    var game = new Game();
        //    game.Numbers[1, 0] = 2;
        //    game.Update(Direction.Down);
        //    Assert.AreEqual(2, game.Numbers[3, 0]);
        //    Assert.AreEqual(0, game.Numbers[1, 0]);
        //    Assert.AreEqual(0, game.Numbers[2, 0]);
        //}

        //[TestMethod]
        //public void TestMoveDownEasy()
        //{
        //    var game = new Game();
        //    game.Numbers[1, 0] = 2;
        //    game.Numbers[2, 0] = 2;
        //    game.Update(Direction.Down);
        //    Assert.AreEqual(4, game.Numbers[3, 0]);
        //    Assert.AreEqual(0, game.Numbers[1, 0]);
        //    Assert.AreEqual(0, game.Numbers[2, 0]);
        //    var info = game.Transformations[1, 0];
        //    Assert.AreEqual(new Coordinate(3, 0), info.Destination);
        //    Assert.AreEqual(2, info.PreviousNumber);
        //    info = game.Transformations[3, 0];
        //    Assert.AreEqual(true, info.WasNew);
        //    info = game.Transformations[2, 0];
        //    Assert.AreEqual(new Coordinate(3, 0), info.Destination);
        //    Assert.AreEqual(2, info.PreviousNumber);
        //}

        [TestMethod]
        public void TestMoveDownBottom()
        {
            var game = new Game();
            game.SetNumber(3, 0, 4);
            game.Update(Direction.Down);
            Assert.AreEqual<uint>(4, game.GetNumber(3, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(2, 0));
            foreach (var trans in game.Transformations)
                Assert.AreEqual(Coordinate.Nowhere, trans.Destination);
        }

        [TestMethod]
        public void TestMoveDownMulti()
        {
            var game = new Game();
            game.SetNumber(1, 0, 4);
            game.SetNumber(2, 0, 2);
            game.SetNumber(3, 0, 2);
            game.Update(Direction.Down);
            Assert.AreEqual<uint>(0, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(4, game.GetNumber(2, 0));
            Assert.AreEqual<uint>(4, game.GetNumber(3, 0));
            var info = game.Transformations[1, 0];
            Assert.AreEqual(new Coordinate(2, 0), info.Destination);
            Assert.AreEqual<uint>(4, info.PreviousNumber);
            info = game.Transformations[3, 0];
            Assert.AreEqual(true, info.WasNew);

            game.Update(Direction.Down);
            Assert.AreEqual<uint>(0, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(2, 0));
            Assert.AreEqual<uint>(8, game.GetNumber(3, 0));
            info = game.Transformations[2, 0];
            Assert.AreEqual(new Coordinate(3, 0), info.Destination);
            Assert.AreEqual<uint>(4, info.PreviousNumber);
            info = game.Transformations[3, 0];
            Assert.AreEqual(true, info.WasNew);
        }

        [TestMethod]
        public void TestMoveRightTrivial()
        {
            var game = new Game();
            game.SetNumber(1, 0, 2);
            game.Update(Direction.Right);
            Assert.AreEqual<uint>(2, game.GetNumber(1, 3));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
        }

        [TestMethod]
        public void TestMoveRightEasy()
        {
            var game = new Game();
            game.SetNumber(1, 0, 2);
            game.SetNumber(1, 1, 2);
            game.Update(Direction.Right);
            Assert.AreEqual<uint>(4, game.GetNumber(1, 3));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 0));
            var info = game.Transformations[1, 0];
            Assert.AreEqual(new Coordinate(1, 3), info.Destination);
            Assert.AreEqual<uint>(2, info.PreviousNumber);
            info = game.Transformations[1, 3];
            Assert.AreEqual(true, info.WasNew);
            info = game.Transformations[1, 1];
            Assert.AreEqual(new Coordinate(1, 3), info.Destination);
            Assert.AreEqual<uint>(2, info.PreviousNumber);
        }

        [TestMethod]
        public void TestMoveRightRight()
        {
            var game = new Game();
            game.SetNumber(1, 3, 4);
            game.Update(Direction.Right);
            Assert.AreEqual<uint>(4, game.GetNumber(1, 3));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            foreach (var trans in game.Transformations)
                Assert.AreEqual(Coordinate.Nowhere, trans.Destination);
        }

        [TestMethod]
        public void TestMoveRightMulti()
        {
            var game = new Game();
            game.SetNumber(1, 0, 2);
            game.SetNumber(1, 1, 2);
            game.SetNumber(1, 2, 4);
            game.Update(Direction.Right);
            Assert.AreEqual<uint>(0, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(4, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(4, game.GetNumber(1, 3));
            var info = game.Transformations[1, 0];
            Assert.AreEqual(new Coordinate(1, 2), info.Destination);
            Assert.AreEqual<uint>(2, info.PreviousNumber);
            info = game.Transformations[1, 2];
            Assert.AreEqual(true, info.WasNew);

            game.Update(Direction.Right);
            Assert.AreEqual<uint>(0, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(8, game.GetNumber(1, 3));            
        }

        [TestMethod]
        public void TestMoveLeftTrivial()
        {
            var game = new Game();
            game.SetNumber(1, 3, 2);
            game.Update(Direction.Left);
            Assert.AreEqual<uint>(0, game.GetNumber(1, 3));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(2, game.GetNumber(1, 0));
        }

        [TestMethod]
        public void TestMoveLeftEasy()
        {
            var game = new Game();
            game.SetNumber(1, 2, 2);
            game.SetNumber(1, 3, 2);
            game.Update(Direction.Left);
            Assert.AreEqual<uint>(4, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 3));
            var info = game.Transformations[1, 2];
            Assert.AreEqual(new Coordinate(1, 0), info.Destination);
            Assert.AreEqual<uint>(2, info.PreviousNumber);
            info = game.Transformations[1, 0];
            Assert.AreEqual(true, info.WasNew);
            info = game.Transformations[1, 3];
            Assert.AreEqual(new Coordinate(1, 0), info.Destination);
            Assert.AreEqual<uint>(2, info.PreviousNumber);
        }

        [TestMethod]
        public void TestMoveLeftMulti()
        {
            var game = new Game();
            game.SetNumber(1, 0, 2);
            game.SetNumber(1, 1, 2);
            game.SetNumber(1, 2, 4);
            game.Update(Direction.Left);
            Assert.AreEqual<uint>(4, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(4, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 3));
            var info = game.Transformations[1, 1];
            Assert.AreEqual(new Coordinate(1, 0), info.Destination);
            Assert.AreEqual<uint>(2, info.PreviousNumber);
            info = game.Transformations[1, 0];
            Assert.AreEqual(true, info.WasNew);

            game.Update(Direction.Left);
            Assert.AreEqual<uint>(8, game.GetNumber(1, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 1));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 2));
            Assert.AreEqual<uint>(0, game.GetNumber(1, 3));
        }

        [TestMethod]
        public void TestMixed()
        {
            var game = new Game();
            game.SetNumber(3, 1, 2);
            game.SetNumber(3, 2, 2);
            game.SetNumber(3, 3, 4);
            var res = game.Update(Direction.Up);
            Assert.AreEqual<uint>(2, game.GetNumber(0, 1));
            Assert.AreEqual<uint>(2, game.GetNumber(0, 2));
            Assert.AreEqual<uint>(4, game.GetNumber(0, 3));
            Assert.AreEqual(new Coordinate(0, 1), game.Transformations[3, 1].Destination);
            Assert.AreEqual(new Coordinate(0, 2), game.Transformations[3, 2].Destination);
            Assert.AreEqual(new Coordinate(0, 3), game.Transformations[3, 3].Destination);
            Assert.AreEqual(-1, game.Smoothness());
            Assert.AreEqual(1, game.Coherence());
            Assert.AreEqual<uint>(0, res.MergedCount);
            res = game.Update(Direction.Right);
            Assert.AreEqual<uint>(0, game.GetNumber(0, 1));
            Assert.AreEqual<uint>(4, game.GetNumber(0, 2));
            Assert.AreEqual<uint>(4, game.GetNumber(0, 3));
            Assert.AreEqual<uint>(1, res.MergedCount);
            Assert.AreEqual(new Coordinate(0, 2), game.Transformations[0, 1].Destination);
            Assert.AreEqual(true, game.Transformations[0, 2].WasNew);
            Assert.AreEqual(1, game.Coherence());
            res = game.Update(Direction.Left);
            Assert.AreEqual<uint>(8, game.GetNumber(0, 0));
            Assert.AreEqual<uint>(0, game.GetNumber(0, 2));
            Assert.AreEqual<uint>(0, game.GetNumber(0, 3));
            Assert.AreEqual(new Coordinate(0, 0), game.Transformations[0, 2].Destination);
            Assert.AreEqual(true, game.Transformations[0, 0].WasNew);
            Assert.AreEqual<uint>(1, res.MergedCount);
        }

        [TestMethod]
        public void TestMonotonicity()
        {
            var game = new Game();
            game.AddCellTrivial(2, 0, 2);
            game.AddCellTrivial(0, 3, 1);
            Assert.AreEqual(-3, game.Monotonicity());
            //game.AddCellTrivial(0, 2, 2);
            //Assert.AreEqual(1, game.Monotonicity());
            //game.AddCellTrivial(0, 2, 3);
            //Assert.AreEqual(2, game.Monotonicity());
            //game.AddCellTrivial(0, 0, 3);
            //Assert.AreEqual(1, game.Monotonicity());
            //game.AddCellTrivial(1, 0, 4);
            //Assert.AreEqual(2, game.Monotonicity());
        }

        [TestMethod]
        public void TestBitmap()
        {
            var game = new Game();
            game.AddCellTrivial(0, 0, 1);
            game.AddCellTrivial(0, 1, 2);
            Assert.AreEqual(0x3, game.GetBitmap());
        }

        [TestMethod]
        public void TestSmoothness()
        {
            var game = new Game();
            game.AddCellTrivial(0, 0, 1);
            game.AddCellTrivial(0, 1, 1);
            game.AddCellTrivial(0, 2, 2);
            Assert.AreEqual(-1, game.Smoothness());
            //game.AddCellTrivial(1, 0, 2);
            //Assert.AreEqual(-1, game.Smoothness());
            //game.AddCellTrivial(1, 1, 3);
            //Assert.AreEqual(-4, game.Smoothness());
        }

        [TestMethod]
        public void TestDispersion()
        {
            var game = new Game();
            game.AddCellTrivial(new Coordinate(0, 0), 1);
            Assert.AreEqual(1, game.Dispersion());
            game.AddCellTrivial(new Coordinate(0, 1), 1);
            Assert.AreEqual(1, game.Dispersion());
            game.AddCellTrivial(new Coordinate(3, 1), 1);
            Assert.AreEqual(2, game.Dispersion());
            game.AddCellTrivial(new Coordinate(3, 2), 1);
            Assert.AreEqual(2, game.Dispersion());
        }

        [TestMethod]
        public unsafe void TestStaticList()
        {
            int* ptr = stackalloc int[16];
            var list = new StaticList(ptr);
            Assert.IsTrue(ptr == list.Waterline);
            list.Add(2);
            Assert.IsTrue(ptr + 1 == list.Waterline);
            Assert.IsTrue(list[0] == 2);
            list.Pop();
            Assert.IsTrue(ptr == list.Waterline);
        }
    }
}
