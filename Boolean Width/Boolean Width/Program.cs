using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Boolean_Width.Preprocessing.ReductionRules;
using Boolean_Width.Preprocessing;

namespace Boolean_Width
{
    class Program
    {

        static void Main(string[] args)
        {

            string[] files = Directory.GetFiles("Graphs/sadiagraphs").ToArray();
            string namePattern = "{0,-" + files.Max(s => s.Length) + "}";
            string pattern = namePattern + " {1,-3} {2, -3} {3,6:0.00}% {4,5:0.00} {5,5:0.00} {6,6:0.00}%";
            string noRulesPattern = namePattern + " NO RULES APPLY";

            object LOCK = new object();
            Parallel.ForEach(files, file =>
            {
                ConsoleLine line = new ConsoleLine();
                line.Write(namePattern, file);
                Graph gr = Parser.ParseGraph(file);

                Graph gr2 = gr.Clone();
                IReductionRuleCommand[] commands = GraphPreprocessor.ApplyRules(gr2, new IReductionRule[] { new TwinReductionRule(), new PendantReductionRule(), new IsletReductionRule() }).Reverse().ToArray();
                if (commands.Length == 0)
                {
                    line.Write(noRulesPattern, file);
                }
                else
                {
                    LinearDecomposition ld = IUNHeuristic.Compute(gr, CandidateStrategy.All, InitialVertexStrategy.All);

                    LinearDecomposition ld2 = IUNHeuristic.Compute(gr2, CandidateStrategy.All, InitialVertexStrategy.All);
                    Decomposition dec = Decomposition.FromLinearDecomposition(ld2);
                    Tree tree = dec.Tree;
                    foreach (IReductionRuleCommand command in commands)
                    {
                        tree = command.Expand(tree);
                    }
                    dec = new Decomposition(gr, tree);
                    line.Write(pattern, file, gr.Vertices.Count, gr2.Vertices.Count, gr2.Vertices.Count * 100.0 / gr.Vertices.Count, ld.BooleanWidth, ld2.BooleanWidth, (ld2.BooleanWidth - ld.BooleanWidth) / ld.BooleanWidth * 100);
                }
            });

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }

    class ConsoleLine
    {
        static object LOCK = new object();

        readonly int line;
        public ConsoleLine ()
        {
            lock (LOCK)
            {
                line = Console.CursorTop;
                Console.WriteLine();
            }
        }

        public void Write(string text)
        {
            lock (LOCK)
            {
                int temp = Console.CursorTop;
                Console.CursorTop = line;
                Console.CursorLeft = 0;
                Console.WriteLine(text.Length > Console.WindowWidth ? text.Substring(0, Console.WindowWidth) : text);
                Console.CursorTop = temp;
            }
        }

        public void Write(string text, params object[] arg)
        {
            Write(String.Format(text, arg));
        }

    }
}
