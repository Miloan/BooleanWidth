using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Boolean_Width
{
    class Program
    {

        static void Main(string[] args)
        {

            Graph graph = Parser.ParseGraph("Graphs/sadiagraphs/david.dgf");


                LinearDecomposition ld = IUNHeuristic.Compute(graph, CandidateStrategy.All, InitialVertexStrategy.DoubleBFS);

                LinearDecomposition ld2 = IUNHeuristic.Compute(graph, CandidateStrategy.All, InitialVertexStrategy.All);

                Console.WriteLine("ld 1 = {0}", ld.BooleanWidth);
                Console.WriteLine("ld ALL = {0}", ld2.BooleanWidth);
                

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
