using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BooleanWidth.Algorithms.BooleanWidth;
using BooleanWidth.Algorithms.BooleanWidth.Linear.Heuristics;
using BooleanWidth.Algorithms.BooleanWidth.Preprocessing;
using BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;

namespace BooleanWidth
{
    static class Program
    {
        
        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            ReadDecompositions();
            

            Console.WriteLine();
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        public static void ReadDecompositions()
        {

            string[] files = Directory.GetFiles("Decompositions/DoubleBFS", "*.bdc", SearchOption.AllDirectories).ToArray();

            ConsoleTable<ExpandoObject> table = new ConsoleTable<ExpandoObject>();
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("Name", "{0}", files.Max(s => s.Length) - "Decompositions\\".Length, s => s.GetOrNull("Name")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("#V", "{0}", 4, s => s.GetOrNull("Vertices")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("BW", "{0:0.00}", 5, s => s.GetOrNull("BooleanWidth")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("LBW", "{0:0.00}", 5, s => s.GetOrNull("LinearBooleanWidth")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("Size", "{0}", 5, s => s.GetOrNull("Size")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("Size right", "{0}", 10, s => s.GetOrNull("RightSize")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("DFS", "{0:0.00}", 5, s => s.GetOrNull("DFS")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("DFS Time", "{0}", 20, s => s.GetOrNull("DFSTime")));

            IDictionary<string, Graph> graphs = new Dictionary<string, Graph>();
            dynamic[] expandos = new dynamic[files.Length];
            for(int i = 0; i < files.Length; i++)
            {
                dynamic obj = new ExpandoObject();
                table.Rows.Add(obj);
                obj.FileName = files[i];
                obj.Name = files[i].Substring("Decompositions\\".Length);
                expandos[i] = obj;
                string graph = Path.GetFileNameWithoutExtension(files[i]);
                if (!graphs.ContainsKey(graph))
                {
                    using (
                        StreamReader reader =
                            new StreamReader(
                                File.Open("Graphs/sadiagraphs/" + Path.GetFileNameWithoutExtension(files[i]) + ".dgf",
                                    FileMode.Open)))
                    {
                        graphs[graph] = Graph.Read(reader);
                    }
                }
            }

            Parallel.ForEach(expandos, dyn =>
            //foreach (dynamic dyn in expandos)
            {
                using (StreamReader decompReader = new StreamReader(File.Open(dyn.FileName, FileMode.Open)))
                {
                    Graph graph = graphs[Path.GetFileNameWithoutExtension(dyn.FileName)];
                    dyn.Vertices = graph.Vertices.Count;
                    Decomposition decomposition = Decomposition.Read(decompReader, graph);
                    

                    BinTree tree = (BinTree)decomposition.Tree;
                    tree.Tilt();
                    decomposition = new Decomposition(graph, (Tree)tree);

                    dyn.BooleanWidth = decomposition.BooleanWidth;
                    
                    {
                        int size = 0;
                        BitSet parent = decomposition.Tree.Root;
                        do
                        {
                            size = Math.Max(size, decomposition.Tree.LeftChild[parent].Count);
                        } while (decomposition.Tree.RightChild.TryGetValue(parent, out parent) && parent.Count > 1);
                        dyn.RightSize = size;
                    }

                    {
                        int size = 0;
                        BitSet parent = decomposition.Tree.Root;
                        do
                        {
                            size += decomposition.Tree.LeftChild[parent].Count - 1;
                        } while (decomposition.Tree.RightChild.TryGetValue(parent, out parent) && parent.Count > 1);
                        dyn.Size = size;
                    }

                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        LinearDecomposition ld = SemiLinearDecomposer.ConvertDecomposition(decomposition);
                        sw.Stop();
                        dyn.DFS = ld.BooleanWidth;
                        dyn.DFSTime = sw.Elapsed;
                    }

                    {
                        LinearDecomposition ld = IunHeuristic.Compute(graph, CandidateStrategy.All, InitialVertexStrategy.DoubleBfs);
                        dyn.LinearBooleanWidth = ld.BooleanWidth;
                        SaveDecomposition(ld, Path.GetDirectoryName(dyn.FileName), Path.GetFileNameWithoutExtension(dyn.FileName));
                    }

                    //SaveDecomposition(decomposition, "NewDec", Path.GetFileNameWithoutExtension(dyn.FileName));
                    //dyn.BooleanWidth = decomposition.BooleanWidth;
                }
            });
        }

        private static void Generate()
        {

            Console.WindowWidth = 100;
            string[] files = Directory.GetFiles("Graphs/sadiagraphs").ToArray();

            ConsoleTable<ExpandoObject> table = new ConsoleTable<ExpandoObject>();
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("File", "{0}", files.Max(s => s.Length), s => s.GetOrNull("FileName")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("#V", "{0}", 4, s => s.GetOrNull("Vertices")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("BW", "{0:0.00}", 5, s => s.GetOrNull("BooleanWidth")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("TIME", "{0}", 20, s => s.GetOrNull("Time")));
            table.Columns.Add(new ConsoleColumn<ExpandoObject>("Written", "{0}", 20, s => s.GetOrNull("Written")));
            dynamic[] statistics = files.Select(s => { dynamic dyn = new ExpandoObject();
                                                         dyn.FileName = s;
                                                         return dyn;
            }).ToArray();
            foreach (ExpandoObject s in statistics)
            {
                table.Rows.Add(s);
            }
            Parallel.ForEach(statistics, s =>
            {
                SemiLinear(s);
            });
        }
        
        private static void SemiLinear(dynamic expando)
        {
            Graph gr;
            using (TextReader reader = new StreamReader(File.Open(expando.FileName, FileMode.Open)))
            {
                gr = Graph.Read(reader);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Graph gr2 = gr.Clone();
            IReductionRuleCommand[] commands = GraphPreprocessor.ApplyRules(gr2, new TwinReductionRule(), new PendantReductionRule(), new IsletReductionRule()).Reverse().ToArray();
            expando.Vertices = gr2.Vertices.Count;
            if (commands.Length != 0)
            {
                LinearDecomposition ld = IunHeuristic.Compute(gr2, CandidateStrategy.All, InitialVertexStrategy.All);
                Decomposition dec = Decomposition.FromLinearDecomposition(ld);
                Tree tree = dec.Tree;
                foreach (IReductionRuleCommand command in commands)
                {
                    tree = command.Expand(tree);
                }
                dec = new Decomposition(gr, tree);

                sw.Stop();
                expando.Time = sw.Elapsed;
                //expando.BooleanWidth = dec.BooleanWidth;

                SaveDecomposition(dec, "Decompositions2", Path.GetFileNameWithoutExtension(expando.FileName));
                expando.Written = "Done";
            }
            else
            {
                expando.BooleanWidth = "-";
                expando.Time = "SKIP";
            }
        }

        private static void Linear(dynamic expando)
        {
            Graph gr;
            using (TextReader reader = new StreamReader(File.Open(expando.FileName, FileMode.Open)))
            {
                gr = Graph.Read(reader);
            }
            expando.Vertices = gr.Vertices.Count;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            LinearDecomposition ld = IunHeuristic.Compute(gr, CandidateStrategy.All, InitialVertexStrategy.All);
            sw.Stop();
            expando.Time = sw.Elapsed;
            expando.BooleanWidth = ld.BooleanWidth;
        }

        private static void SaveDecomposition(LinearDecomposition decomposition, string folder, string fileName)
        {

            Directory.CreateDirectory(folder);
            using (
                TextWriter writer =
                    new StreamWriter(
                        File.Open(
                            folder + "/" + fileName + ".lbdc",
                            FileMode.Create)))
            {
                decomposition.Write(writer);
                writer.Flush();
            }
        }

        private static void SaveDecomposition(Decomposition decomposition, string folder, string fileName)
        {

            Directory.CreateDirectory(folder);
            using (
                TextWriter writer =
                    new StreamWriter(
                        File.Open(
                            folder + "/" + fileName + ".bdc",
                            FileMode.Create)))
            {
                decomposition.Write(writer);
                writer.Flush();
            }
            Directory.CreateDirectory(folder + "/LaTeX/");
            using (
                TextWriter writer =
                    new StreamWriter(
                        File.Open(
                            folder + "/LaTeX/" + fileName + ".tex",
                            FileMode.Create)))
            {
                decomposition.Tree.WriteLatex(writer);
                writer.Flush();
            }
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "lualatex";
            p.StartInfo.Arguments = "\"" + folder + "/LaTeX/" + fileName + ".tex" + "\"";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (File.Exists(folder + "/LaTeX/" + fileName + ".pdf"))
            {
                File.Delete(folder + "/LaTeX/" + fileName + ".pdf");
            }
            File.Move(fileName + ".pdf", folder + "/LaTeX/" + fileName + ".pdf");
            
            File.Delete(fileName + ".log");
            File.Delete(fileName + ".aux");
        }
        
        public static object GetOrNull(this ExpandoObject expando, string key)
        {
            IDictionary<string, object> dict = expando;
            Object obj;
            if (dict.TryGetValue(key, out obj))
            {
                return obj;
            }
            return null;
        }
    }
}
