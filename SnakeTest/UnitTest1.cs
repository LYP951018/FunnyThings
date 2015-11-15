using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Snake;

namespace SnakeTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
        }
    }

    [TestClass]
    public class PriorityTest
    {
        [TestMethod]
        public void TestEnqueue()
        {
            PriorityQueue<int> queue = new PriorityQueue<int>();
            queue.Enqueue(3, 2, 5, 3, 4, 21, 100, 2, 3, 1, 3, 4, 5, 6, 7);
            Assert.IsTrue(IsQueueEpual(queue, new int[] { 1, 2, 4, 3, 2, 5, 6, 3, 3, 4, 3, 21, 5, 100, 7 }));
        }

        [TestMethod]
        public void TestDequeue()
        {
            PriorityQueue<int> queue = new PriorityQueue<int>();
            queue.Enqueue(3, 2, 5, 3, 4, 21, 100, 2, 3, 1, 3, 4, 5, 6, 7);
            var i = queue.Dequeue();
            Assert.IsTrue(i == 1);
            Assert.IsTrue(IsQueueEpual(queue, new int[] { 2, 2, 4, 3, 3, 5, 6, 3, 3, 4, 7, 21, 5, 100 }));
        }

        bool IsQueueEpual(PriorityQueue<int> queue, int[] param)
        {
            if (queue.Count != param.Length) return false;
            int i = 0;
            foreach (var c in queue)
            {
                if (c != param[i]) return false;
                i++;
            }
            return true;

        }

    }

    [TestClass]
    public class BfsTest
    {
        [TestMethod]
        public void TestBfs()
        {
            var graph = new MainWindow.GridState[16, 16]
            {
                {MainWindow.GridState.Wall,MainWindow.GridState.Wall,   MainWindow.GridState.Wall,      MainWindow.GridState.Wall,      MainWindow.GridState.Wall,      MainWindow.GridState.Wall,      MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   /**/MainWindow.GridState.Snake,     MainWindow.GridState.Snake,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Snake,     MainWindow.GridState.Snake,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Snake,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Snake,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Snake,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Snake,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Egg,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,   MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,/**/MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Nothing,MainWindow.GridState.Snake,     MainWindow.GridState.Snake,     MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,   MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Nothing,MainWindow.GridState.Wall},
                {MainWindow.GridState.Wall,MainWindow.GridState.Wall,   MainWindow.GridState.Wall,      MainWindow.GridState.Wall,      MainWindow.GridState.Wall,      MainWindow.GridState.Wall,      MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall,MainWindow.GridState.Wall}
            };
            var path = MainWindow.BfsSearch(graph, new Coordinate(1, 6), new Coordinate(9, 2));
            Assert.IsTrue(path != null);
        }

    }

}
