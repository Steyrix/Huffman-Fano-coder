using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMPparserConverterToGS
{
    public class Node
    {
        public int pref;
        public int count;
        public int rank = 0;
        public int symbol;
        public string code;
        public string txt;
        public float possibility;
        public bool isParent = false;
        public bool isChild = false;
        public Node child;
        public Node left;
        public Node right;
        public Node parent;
        public List<float> possibilities;
        public List<int> freqs;
        //создать конструктор для объявления начальных узлов для методов Шеннона-Фано и Хаффмана


       //Для Хаффмана
        public Node (int symbol, int count) //Создание узла
        {
            this.symbol = symbol;
            this.count = count;
            
            this.rank = 1;
        }

        public Node(Node left, Node right) //Создание узла по двум узлам-родителям
        {
            this.symbol = (char)0;
            this.left = left;
            this.right = right;
            this.count = left.count + right.count;
            
            left.isParent = true;
            right.isParent = true;
        }
           
        

        public Node(int frequency)
        {
            this.count = frequency;
           
        }

        public Node(int frequency, string codeel, List<int> freqs)
        {
            this.count = frequency;
            this.code += codeel;
            this.freqs = freqs;
        }

        public Node(int frequency, List<int> freqs)
        {
            this.rank = 1;
            this.count = frequency;
            this.freqs = freqs;
        }
    }
}
