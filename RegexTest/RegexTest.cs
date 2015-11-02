using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexChart;
using RegexChart.RegexParser;

namespace RegexTest
{
    [TestClass]
    public class RegexTest
    {
        [TestMethod]
        public void TestParser()
        {
            Assert.IsTrue(IsEqual("[a-z]", RegexNode.GetCharSetExpression('a', 'z')));
            Assert.IsTrue(IsEqual("a{1,2}", RegexNode.GetCharSetExpression('a').Loop(1, 2)));
            Assert.IsTrue(IsEqual("ab*c", RegexNode.GetCharSetExpression('a') +
                RegexNode.GetCharSetExpression('b').Any() +
                RegexNode.GetCharSetExpression('c')));
            Assert.IsTrue(IsEqual("(<captured>a*)", 
                RegexNode.GetCapture("captured", RegexNode.GetCharSetExpression('a').Any())));
            Assert.IsTrue(IsEqual(
                "(=a*)", +RegexNode.GetCharSetExpression('a').Any()));
        }

        [TestMethod]
        public void TestComplexParser()
        {
            Assert.IsTrue(IsEqual("a+(bc)*",
                RegexNode.GetCharSetExpression('a').Some() +
                (RegexNode.GetCharSetExpression('b') +
                RegexNode.GetCharSetExpression('c')).Any()));

        }

        [TestMethod]
        public void TestCharSetNormalize()
        {
            Assert.IsTrue(IsCharSetEqual("[a-z][A-Z]", RegexNode.GetCharSetExpression('a','z') + RegexNode.GetCharSetExpression('A','Z')));
            Assert.IsTrue(IsCharSetEqual("[a-g][b-z]", (RegexNode.GetCharSetExpression('a', 'a') % RegexNode.GetCharSetExpression('b','g')) + (RegexNode.GetCharSetExpression('b', 'g') % RegexNode.GetCharSetExpression('h', 'z'))));
            Assert.IsTrue(IsCharSetEqual("[^C-X][A-Z]",
                RegexNode.GetCharSetExpression((char)1, (char)('A' - 1)) % RegexNode.GetCharSetExpression('A', 'B') %
                RegexNode.GetCharSetExpression('Y', 'Z') % RegexNode.GetCharSetExpression((char)('Z' + 1), char.MaxValue)
                + RegexNode.GetCharSetExpression('A', 'B') % RegexNode.GetCharSetExpression('C', 'X') %
                RegexNode.GetCharSetExpression('Y', 'Z')));
        }

        public bool IsEqual(string input,RegexNode node)
        {
            var parser = new Parser(input);
            var exp = parser.ParseExpression();
            return exp.Equals(node.Exp);
        }

        public bool IsCharSetEqual(string input, RegexNode node)
        {
            var parser = new Parser(input);
            var exp = parser.ParseExpression();
            var sets = new FlatSet<CharRange>();
            exp.NormalizeCharSet(out sets);
            return exp.Equals(node.Exp);
        }
    }
}
