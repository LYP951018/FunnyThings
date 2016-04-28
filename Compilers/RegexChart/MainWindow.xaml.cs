using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Msagl.WpfGraphControl;
using Microsoft.Msagl.Drawing;
using RegexChart.RegexParser;

namespace RegexChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            graphViewer.BindToPanel(GraphViewPanel);
           // var graph = new Graph();
           // graph.AddEdge("A", "B");
           // graph.Attr.LayerDirection = LayerDirection.LR;
           // graphViewer.Graph = graph;
              
        }

        private void RegexMenuItem_Click(object sender, RoutedEventArgs e)
        { 
            RegexInput inputWindow = new RegexInput();
            inputWindow.Owner = this;
            inputWindow.ShowDialog();
            regexString = inputWindow.RegexString;
            if(regexString != string.Empty)
            {
                RegexContent.Text = regexString;
                var parser = new RegexParser.Parser(regexString);
                var exp = parser.ParseExpression();
                EpsilonNfa = exp.GenerateExpsilonNfa();
                Nfa = Automaton.RemoveEpsilon(EpsilonNfa);
                MultiValueDictionary<State, State> statesMap;
                Dfa = Automaton.NfaToDfa(Nfa, out statesMap);
            }
           
        }

        void DrawGraph(Automaton automaton)
        {
            var graph = new Graph();
            graph.Attr.LayerDirection = LayerDirection.LR;
            int i = 0;
            foreach (var s in automaton.States)
            {
                foreach (var o in s.Output)
                {
                    var index = automaton.States.IndexOf(o.End);
                    var end = ReferenceEquals(automaton.StartState, o.End) ? "Start" : index.ToString();
                    if (ReferenceEquals(s, automaton.StartState))
                    {
                        graph.AddEdge("Start", o.ToString(), end);
                    }
                    else if (o.End.IsFinalState)
                    {
                        graph.AddEdge(i.ToString(), o.ToString(), "End");
                    }
                    else
                    {
                        graph.AddEdge(i.ToString(), o.ToString(), end);
                    }

                }
                i++;
            }
            graphViewer.Graph = graph;
        }


        private string regexString;
        Automaton EpsilonNfa;
        Automaton Nfa;
        Automaton Dfa;
        GraphViewer graphViewer = new GraphViewer();

        private void EpsilonNFA_Click(object sender, RoutedEventArgs e)
        {
            DrawGraph(EpsilonNfa);
        }

        private void NFA_Click(object sender, RoutedEventArgs e)
        {
            DrawGraph(Nfa);
        }

        private void DFA_Click(object sender, RoutedEventArgs e)
        {
            DrawGraph(Dfa);
        }
    }
}
