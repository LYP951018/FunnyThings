using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using iSchemeInterpreter;

namespace InterpreterTest
{
    [TestClass]
    public class TokenizerTest
    {
        [TestMethod]
        public void TokenizeTest()
        {
            Assert.IsTrue("['a']" == Tokenizer.Print(Tokenizer.Tokenize("a")));
            Assert.IsTrue("['(', 'def', 'a', '3', ')']" == Tokenizer.Print(Tokenizer.Tokenize("(def a 3)")));
            Assert.IsTrue("['(', 'begin', '(', 'def', 'a', '3', ')', '(', '*', 'a', 'a', ')', ')']" == Tokenizer.Print(
                Tokenizer.Tokenize("(begin (def a 3) (* a a))")));
        }
    }
}
