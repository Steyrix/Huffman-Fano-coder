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
    public class HuffmanCoder
    {
        //Чтобы сортировать массив узлов
        class Comparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                return x.count - y.count;
            }
        }

        public string[] codes = new string[260];

        const string dotFile = "huffmantree.dot";
        const string imgFile = "huffmantree.png";

        static string path = AppDomain.CurrentDomain.BaseDirectory;

        List<int> frequences;
        List<int> values;

        int sum, firstcount;
        public float averageLen, minLen;

        StreamWriter file;

        public string code;
        public List<Node> nodes = new List<Node>();
        public double H;

        public HuffmanCoder(List<int> collectionFrequences, List<int> collectionValues)
        {
            frequences = collectionFrequences;
            values = collectionValues;
        }

        public void build()
        {
            //Редактируем dot-файл, описываем в нем новый граф
            file = new StreamWriter(dotFile);
            file.Write("digraph Huffman { \r\n ");
            

            int i = 0;
            foreach (int frequency in frequences)
            {
                
                nodes.Add(new Node(values[i], frequency));
                
                sum += frequency;
                i++;
            }
            nodes.Sort(new Comparer());
            firstcount = i;

            createNode();

            writeNode();
            
            drawImg();
       }

        public void createNode()
        {
            string temp = null;
            string use = null;
            int k = 0;
            int max = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                
                //Если узел не является родителем или корнем, создаем узел-ребенок между текущим узлом и следующим
                if (!nodes[i].isParent && nodes[i].count != sum)
                {
                    nodes.Add(new Node(nodes[i], nodes[i + 1]));

                    nodes[nodes.Count - 1].isChild = true;

                    nodes[i].isParent = true;
                    nodes[i].child = nodes[nodes.Count - 1];
                    nodes[i].code += "0";
                   
                    nodes[i + 1].isParent = true;
                    nodes[i + 1].child = nodes[nodes.Count - 1];
                    nodes[i + 1].code += "1";

                    nodes.Sort(new Comparer());

                }

            
                
             
            }
            code += "Коды Хаффмана: ";

            foreach (Node node in nodes)
            {
                //Рассчитываем коды для узлов первого ранга (начальных). Также рассчитываем необходимые для задания данные
                if (node.rank == 1)
                {
                    temp += getCode(node);
                    for (int i = temp.Length - 1; i >= 0; i--)
                    {
                        use += temp[i];
                    }

                    codes[node.symbol] = use;
                    
                    averageLen += use.Length * node.count;
                    code += " '" + node.symbol + "(" + node.count + ")" + "' - " + use + ";";
                    use = null;
                    temp = null;
                    sum += node.count;

                    if (node.count >= max)
                    {
                        max = node.count;
                        minLen = getCode(node).Length;
                    }
                }


                if (k < nodes.Count - 1 && node.count == nodes[k + 1].count)
                {
                    node.txt = "b" + node.count;
                }
                else
                    node.txt += node.count;

                k++;
            }
            
            code += "Средняя длина кодовой комбинации = " + averageLen + ".";

            
        }

        public void writeNode()
        {
            //Функция описания кодового дерева (графа) на языке dot. Принцип прост. Сначала объявляем все 
            //существующие узлы. А потом, основываясь на принадлежности одних узлов к другим как детей к родителям
            //устанавливаем между ними связь с определенными префиксом в зависимости от стороны "родителя"
            string txt = "{rank = same;";
            

            foreach (Node node in nodes)
            {
                
                file.Write(node.txt + "[shape=box]; \n");
                if (node.left != null)
                {
                    file.Write(node.left.txt + " -> " + node.txt + " [label = 0]; \n");
                }
                if (node.right != null)
                {
                    file.Write(node.right.txt + " -> " + node.txt + " [label = 1]; \n");
                }

                if (node.rank == 1)
                {
                    txt += node.txt + " ";
                }
            }   

            txt += "}";
            file.WriteLine(txt);
            file.Write("}");
            file.Close();
        }



        public string getCode(Node node)
        {
            //рекурсивно получаем код начального узла, складывая коды предшествующих ему родителей и их родителей
            if (node.isParent)
                return node.code + getCode(node.child);
            else
                return node.code;
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
