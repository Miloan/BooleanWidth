using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BooleanWidth.Algorithms.BooleanWidth.Linear.Heuristics;
using BooleanWidth.Algorithms.BooleanWidth.Preprocessing;
using BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;
using BooleanWidth.IO;

namespace BooleanWidth
{
    class Program
    {

        static void Main(string[] args)
        {

            string[] files = Directory.GetFiles("Graphs/sadiagraphs").ToArray();
            string namePattern = "{0,-" + files.Max(s => s.Length) + "}";
            string pattern = namePattern + " {1,-3} {2, -3} {3,6:0.00}% {4,5:0.00} {5,5:0.00} {6,6:0.00}%";
            string noRulesPattern = namePattern + " NO RULES APPLY";

            //foreach (string file in files)
            //{
            //    Stopwatch sw = new Stopwatch();
            //    sw.Start();
            //    Graph gr = Parser.ParseGraph(file);
            //    LinearDecomposition ld = IUNHeuristic.Compute(gr, CandidateStrategy.All, InitialVertexStrategy.BFS);
            //    sw.Stop();
            //    Console.WriteLine(namePattern + " {1}ms", file, sw.ElapsedMilliseconds);
            //}
            //Console.ReadLine();

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
                    LinearDecomposition ld = IunHeuristic.Compute(gr, CandidateStrategy.All, InitialVertexStrategy.DoubleBfs);

                    LinearDecomposition ld2 = IunHeuristic.Compute(gr2, CandidateStrategy.All, InitialVertexStrategy.DoubleBfs);
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
}
