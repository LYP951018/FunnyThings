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
            if(regexString!=string.Empty)
            {
                RegexContent.Text = regexString;
                var parser = new RegexParser.Parser(regexString);
                var exp = parser.ParseExpression();
                var nfa = exp.GenerateExpsilonNfa();
                var graph = new Graph();
                graph.Attr.LayerDirection = LayerDirection.LR;
                int i = 0;
                foreach (var s in nfa.States)
                {
                    foreach (var o in s.Output)
                    {
                        var index = nfa.States.IndexOf(o.End);
                        if (ReferenceEquals(s, nfa.StartState))
                        {
                            graph.AddEdge("Start", o.ToString(), index.ToString());
                        }
                        else if (o.End.IsFinalState)
                        {
                            graph.AddEdge(i.ToString(), o.ToString(), "End");
                        }
                        else
                        {
                            graph.AddEdge(i.ToString(), o.ToString(), index.ToString());
                        }

                    }
                    i++;
                }
                graphViewer.Graph = graph;
            }
           
        }

        private string regexString;
        GraphViewer graphViewer = new GraphViewer();
    }
}
