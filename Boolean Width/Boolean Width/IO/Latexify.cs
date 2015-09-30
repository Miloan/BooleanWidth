using System;
using System.IO;
using System.Text;

namespace BooleanWidth.IO
{
    public static class Latexify
    {
        public static void MakeTable(string fileIn, string fileOut, bool[] useAge, bool[] rounding, int digits)
        {
            StreamReader sr = new StreamReader(fileIn);
            StreamWriter sw = new StreamWriter(fileOut);

            int used = 0;
            foreach (bool b in useAge)
                used = b ? used + 1 : used;

            sw.WriteLine("\\begin{table}");
            sw.WriteLine("\\centering");
            string c = "|";
            for (int i = 0; i < used; i++)
                c = c + "c|";
            sw.WriteLine("\\caption{\\small{}}");
            sw.WriteLine("\\begin{tabular}{"+c+"}");
            sw.WriteLine("\\hline");
            
            string line = sr.ReadLine();
            string[] elements = line.Split('\t');
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < elements.Length; i++)
            {
                if (useAge[i])
                {
                    sb.Append(elements[i]);
                    sb.Append(" & ");
                }
            }
            sb.Remove(sb.Length - 2, 1);
            sb.Append("\\\\");
            sw.WriteLine(sb.ToString());
            
            sw.WriteLine("\\hline");

            while ((line = sr.ReadLine()) != null)
            {
                elements = line.Split('\t');
                sb.Clear();

                for (int i = 0; i < elements.Length; i++)
                {
                    if (useAge[i])
                    {
                        if (rounding[i])
                        {
                            Decimal d = Decimal.Parse(elements[i].Replace(',', '.'));
                            sb.Append(Decimal.Round(d, 2).ToString());
                        }
                        else
                        {
                            sb.Append(elements[i]);
                        }
                        sb.Append(" & ");
                    }
                }
                sb.Remove(sb.Length - 2, 1);
                sb.Append("\\\\");
                sw.WriteLine(sb.ToString());
            }

            sw.WriteLine("\\hline");
            sw.WriteLine("\\end{tabular}");
            
            sw.WriteLine("\\label{table:table}");
            sw.WriteLine("\\end{table}");


            sw.Close();
            sr.Close();
        }
    }
}
