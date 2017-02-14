using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;

namespace BMPparserConverterToGS
{
    class ShennonFano
    {
        

        class Comparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                return y.count - x.count;
            }
        }

        static string dotFile = "shenontree.dot";
        static string imgFile = "shenontree.png";

        static string path = AppDomain.CurrentDomain.BaseDirectory;

        List<Node> nodes = new List<Node>();
                    
        public List<int> frequences;
        public List<int> values;
        public int[] freqs = new int[128];
        public string[] codes = new string[260];     
        int sumfreq;
        public float codeLen, avLen, minLen;
        public string code; //debug

        
        StreamWriter file;

        public ShennonFano (List<int> collectionFrequences, List<int> collectionValues)
        {
            frequences = collectionFrequences;
            values = collectionValues;
            
        }

        public void build()
        {
            int max = 0;
            file = new StreamWriter(dotFile);
            file.Write("digraph Fano { \r\n ");
            int k = 0, j = 0;
            foreach (int frequency in frequences)
            {
                sumfreq += frequency; ; //Значение вероятности корня
                freqs[frequency] = values[k];
                k++;
            }
            frequences.Sort();
            frequences.Reverse();
            //Создаем корень
            nodes.Add(new Node(sumfreq, frequences));
            
            for (int i = 0; i < nodes.Count;i++)
            {
                if (nodes[i].freqs.Count > 1)
                {
                    //Для каждого узла создаем узлы-наследники. 
                    createChild(nodes[i]);
                }
            }
            code += "Коды Шеннона-Фано: ";

            foreach(Node node in nodes)
            {
                //Для каждого узла, который содержит только одну частоту, не является родителем
                if(node.freqs.Count == 1)
                {
                    code += "  '" + freqs[node.count] + "(" + node.count + ")" + "' - " + getCode(node) + ";";
                    codeLen += getCode(node).Length;
                    avLen += codeLen * node.freqs[0];
                    
                    if (node.freqs[0] >= max)
                    {
                        max = node.freqs[0];
                        minLen = getCode(node).Length;
                    }

                    codes[freqs[node.count]] = getCode(node);
                }

                if (j < nodes.Count - 1 && node.count == nodes[j + 1].count)
                {
                    node.txt = "b" + node.count;
                }
                else
                    node.txt += node.count;
                j++;
            }
            code += "Средняя длина кодовой комбинации: " + avLen + ".";
            writeNode();
            drawImg();
            
        }

        public void createChild(Node noda)
        {
            //Делим наборы частот на группы, создавая новые узлы, родителем которых будет аргумент функции.
            //Для этого нам нужен отсортированный по убыванию массив частот
            noda.freqs.Sort();
            noda.freqs.Reverse();

            List<int> set1 = new List<int>();
            List<int> set2 = new List<int>();
                        
            int g1 = 0, g2 = 0;
            int b = 0, e = noda.freqs.Count - 1;

            for (int i = 0; i < noda.freqs.Count; i++)
            {
                if (g1 <= g2)
                {
                    g1 += noda.freqs[b];
                    set1.Add(noda.freqs[b]);
                    b++;
                }
                else
                {
                    g2 += noda.freqs[e];
                    set2.Add(noda.freqs[e]);
                    e--;
                }
                
            }

            noda.left = new Node(g1, "1", set1);
            noda.left.parent = noda;
            nodes.Add(noda.left);

            noda.right = new Node(g2, "0", set2);
            noda.right.parent = noda;
            nodes.Add(noda.right);
        }

        public string getCode(Node noda)
        {
            //Рекурсивно получаем коды конечных узлов, складывая узлы предшествующих им родителей и их родителей
            if (noda.parent == null)
                return noda.code;
            else
                return getCode(noda.parent) + noda.code;
        }

        public void writeNode()
        {
            //Функция описания кодового дерева (графа) на языке dot. Принцип прост. Сначала объявляем все 
            //существующие узлы. А потом, основываясь на принадлежности одних узлов к другим как детей к родителям
            //устанавливаем между ними связь с определенными префиксом в зависимости от стороны "родителя"
            foreach (Node noda in nodes)
            {
                file.Write(noda.txt + "[shape=box]; \n");

                if (noda.left != null)
                {
                    file.Write(noda.txt + "->" + noda.left.txt + "[label = 1]; \n");
                }
                if (noda.right != null)
                {
                    file.Write(noda.txt + "->" + noda.right.txt + "[label = 0]; \n");
                }
            }

            file.Write("}");
            file.Close();
        }

        public void drawImg()
        {
            //Процесс запуска graphviz и конвертирования dot-файла в png
            Process dot = new Process();
            dot.StartInfo.WorkingDirectory = path;
            dot.StartInfo.FileName = path + "graphviz\\bin\\dot.exe";
            dot.StartInfo.Arguments = "-Tpng " + dotFile + " -o " + imgFile;

            dot.StartInfo.UseShellExecute = false;
            dot.StartInfo.CreateNoWindow = true;
            //Так как graphviz работает медленее программы, ждем, пока процесс завершится
            dot.Start();
            while (!dot.HasExited) ;
            dot.Close();
        }
    }
}
