using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Word = Microsoft.Office.Interop.Word;
using System.Reflection;

namespace BMPparserConverterToGS
{
    public partial class MainForm : Form
    {
        //Переменная-индикатор того, какой из графов показывается на экране
        bool shenon = false;

        //Классы для кодирования по Хаффману и Шеннону-Фано (кодировщики)
        HuffmanCoder Huff;
        ShennonFano Fano;

        
        static string path = AppDomain.CurrentDomain.BaseDirectory;
        
        string target, quant;//Первичный  алфавит и отквантованный алфавит
        string BinaryCode = null;//Равномерный код
        string freqsAndValues = null;
        string huffcode, fanocode; //Кодовые последовательности после кодирования по Хаффману и Шеннону-Фано
        int huff1, huff0, fano1, fano0;//Количество единиц и нулей в последовательности из кодов Хаффмана и Шеннона-Фано
        //Энтропия первичного алфавита, энтропия отквантованного алфавита, кол-во информации, избыточности, средняя длина равномерного кода,
        //степени сжатия, кол-ва информации в кодах Хаффмана и Фано
        double Hprimary = 0, Hsecondary = 0, Isecondary = 0, I = 0, Qhuffman = 0, Qfano = 0, avLen = 0, Khuffman = 0, Kfano = 0, huffI = 0, fanoI = 0;
        double huffH = 0, fanoH = 0;

        const int n = 128; //Исследуем строку из 128 пикселей
        Bitmap sourceImage;

        //Динамические массивы для последующей передачи значений в кодировщики
        List<int> collectionFrequences = new List<int>();
        List<double> possibilities = new List<double>();
        List<double> possibilitiesPR = new List<double>();
        List<int> collectionValues = new List<int>();

        int[] equalCode = new int[300];

        int minLen = 999999;
        string temp = null;
        int number = 1;
        //Первичные значения пикселей
        int[] targetpixels = new int[n];
        //Значения пикселей после квантования
        int[] quanted = new int[n];
        //Частоты квантованных символов
        int[] frequency = new int[300];
        //Для подсчета уникальных символов
        int[] NUfrequency = new int[300];

        int sumFrequency = 0;
        int primaryAlphabet = 0;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            
        }

        public MainForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }
       
        

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {

            label1.Text = "Значения после квантования: ";
            label2.Text = "Первичный алфавит: ";
            label3.Text = "Уникальные символы и их частота [символ(частота)]: ";
            label4.Text = "Анализ первичного алфавита: ";
            
            //Настраиваю диалог для открытия изображений
            openFileDialog1.Filter = "Image files (*.jpg;*.png) | *.jpg;*.png |All files(*.*)|*.*";
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    
                    sourceImage = (Bitmap)Image.FromFile(openFileDialog1.FileName);
                    sourceImage.Save("source.png");
                    pictureBox1.Width = sourceImage.Width;
                    pictureBox1.Height = sourceImage.Height;
                    pictureBox1.Image = sourceImage;
                    ConvertToGrayScale(sourceImage, targetpixels);
                    
                }

                catch (Exception)
                {
                    label1.Text = "Some error!!!";
                }
            }
            
            for (int i = 0; i < n; i++)
            {
                
                
                //Формула квантования  X = round(X / 20) * 20
                quanted[i] = (int)Math.Round((float)(targetpixels[i] / 20)) * 20;
                NUfrequency[targetpixels[i]]++;
                if (equalCode[quanted[i]] == 0)
                {
                    equalCode[quanted[i]] = number;
                    number++;
                }
                frequency[quanted[i]]++;
                //при любом увеличении частоты, общая частота увеличивается, очевидно
                sumFrequency++;

                //Выписываем в лейблы и переменные последовательности символов первичного алфавита и отквантованного алфавита
                if (i != n - 1)
                {
                    label1.Text += (quanted[i] + ", ");
                    quant += (quanted[i] + ", ");
                    label2.Text += (targetpixels[i] + ",");
                    target += (targetpixels[i] + ",");
                }
                else
                {
                    label1.Text += (quanted[i] + ".");
                    quant += (quanted[i] + ".");
                    label2.Text += (targetpixels[i] + ".");
                    target += (targetpixels[i] + ".");
                }

            }

            for (int i = 0; i < 300; i++)
            {
                if (frequency[i] != 0)
                {
                    //Занесение значений символов и их частот, вероятностей в динамические массивы
                    collectionValues.Add(i);
                    collectionFrequences.Add(frequency[i]);
                    possibilities.Add((double)frequency[i] / (double)sumFrequency);
                    
                    label3.Text += " " + i + "(" + frequency[i] + ";" + possibilities[possibilities.Count-1] + ");";
                    freqsAndValues += " " + i + "(" + frequency[i] + ");";
                }

                if (NUfrequency[i] != 0)
                {
                    possibilitiesPR.Add((double)NUfrequency[i] / 128);
                    primaryAlphabet++;
                    
                }               
            }
            //Нахождение равномерного кода
            for (int i = 0; i < n; i++)
            {
                temp = Convert.ToString(equalCode[quanted[i]], 2).PadLeft(8, '0');
                BinaryCode += temp;
                avLen += frequency[quanted[i]] * temp.Length;
                if (temp.Length < minLen)
                    minLen = temp.Length;
            }

            
            //Энтропия первичного алфавита
            for (int i = 0; i < possibilitiesPR.Count; i++)
            {
                Hprimary -= possibilitiesPR[i] * Math.Log(possibilitiesPR[i], 2);
            }
            I = Hprimary * n; //Кол-во информации первичного алфавита

            //Те же вычисления для вторичного
            for (int i = 0; i < possibilities.Count; i++)
            {
                Hsecondary -= possibilities[i] * Math.Log(possibilities[i], 2);
            }
            Isecondary = Hsecondary * n;
            label3.Text += "Энтропия вторичного алфавита = " + Hsecondary;


            possibilities.Sort();

            label4.Text += " Кол-во уникальных символов в ПА - " + primaryAlphabet + ". Значение инф. энтропии: " + Hprimary + ". Ср. длина ДК: " + avLen +
                ". Двоичный равномерный код: " + BinaryCode + ". Минимальная длина кода: " + minLen;

            collectionFrequences.Sort();

            //Инициализируем наши кодировщики
            Huff = new HuffmanCoder(collectionFrequences, collectionValues);
            Fano = new ShennonFano(collectionFrequences, collectionValues);

            try
            {
                if (pictureBox2.Image != null)
                {

                    pictureBox2.Image.Dispose();
                    pictureBox2.Image = null;

                    File.Delete("huffmantree.png");
                    File.Delete("shennontree.png");

                    label5.Text = "";
                    label6.Text = "";

                }

                button2.Visible = true;

                //Активируем кодировщик Хаффмана, затем расчитываем энтропию и кол-во информации, используя значения из класса-кодировщика
                //Рассчитываем необходимые для задания данные. Аналогично поступает в случае с кодировщиком Шеннона-Фано.
                Huff.build();
                for (int i = 0; i < 128; i++)
                {
                    if (quanted[i] != 0)
                        huffcode += Huff.codes[quanted[i]];
                }
                for (int i = 0; i < huffcode.Length; i++)
                {
                    if (huffcode[i] == '1') huff1++;
                    else huff0++;
                }
                for (int i = 0; i < huffcode.Length; i++)
                {
                    if (huffcode[i] == '1')
                        huffH -= ((double)huff1 / (double)huffcode.Length) * Math.Log((double)huff1 / (double)huffcode.Length, 2);
                    else
                        huffH -= ((double)huff0 / (double)huffcode.Length) * Math.Log((double)huff0 / (double)huffcode.Length, 2);
                }
                huffI = huffH * huffcode.Length;
                Qhuffman = 1 - I / (Isecondary * Huff.averageLen);
                Khuffman = avLen / Huff.averageLen;
                label5.Text += Huff.code + " Избыточность: " + Qhuffman + ". Степень сжатия: " + Khuffman + ". Минимальная длина: " + Huff.minLen + ". Код: " + huffcode;
                label5.Visible = true;


                Fano.build();
                for (int i = 0; i < 128; i++)
                {
                    if (quanted[i] != 0)
                        fanocode += Fano.codes[quanted[i]];
                }
                for (int i = 0; i < fanocode.Length; i++)
                {
                    if (fanocode[i] == '1') fano1++;
                    else fano0++;
                }
                for (int i = 0; i < fanocode.Length; i++)
                {
                    if (fanocode[i] == '1')
                        fanoH -= ((double)fano1 / (double)fanocode.Length) * Math.Log((double)fano1 / (double)fanocode.Length, 2);
                    else
                        fanoH -= ((double)fano0 / (double)fanocode.Length) * Math.Log((double)fano0 / (double)fanocode.Length, 2);
                }
                fanoI = fanoH * fanocode.Length;
                Qfano = 1 - I / (Isecondary * Fano.avLen);
                Kfano = avLen / Fano.avLen;
                label6.Text += Fano.code + " Избыточность: " + Qfano + ". Степень сжатия: " + Kfano + ". Минимальная длина: " + Fano.minLen + ". Код: " + fanocode;
                label6.Visible = true;

                
                panel1.AutoScroll = true;
                pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox2.Image = Image.FromFile("huffmantree.png");

                
            }

            catch(Exception)
            {
                label1.Text = "Some error!!!";
            }

            //Создаем отчёт
            createWordDoc();

        }

        static void ConvertToGrayScale(Bitmap source, int[] targpix)
        {
            for (int i = 0; i < source.Height; ++i)
                for (int j = 0; j < source.Width; ++j)
                {
                    Color pix = source.GetPixel(j, i);
                    int gray = (int)(0.3 * pix.R + 0.59 * pix.G + 0.11 * pix.B);
                    
                    //Сохраняем центральную строку грейскейл-изображения 
                    if (i == 63)
                    {
                        targpix[j] = gray;
                    }
                    source.SetPixel(j, i, Color.FromArgb(pix.A, gray, gray, gray));
                }
            source.Save("grayscale.jpg");
        }


        private void button2_Click(object sender, EventArgs e)
        {
            

            if (!shenon)
            {
                panel1.AutoScroll = true;
                pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox2.Image = Image.FromFile("shenontree.png");
                shenon = true;
                button2.Text = "Показать дерево Хаффмана";
            }
            else
            {
                panel1.AutoScroll = true;
                pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox2.Image = Image.FromFile("huffmantree.png");
                shenon = false;
                button2.Text = "Показать дерево Шеннона-Фано";
            }
        }

        //Функция для поиска текста в документе, для последующей вставки изображений после
        //Определенного текста
        private Word.Range findText(object findTextObj, Word.Document document)
        {
            Object _missingObj = System.Reflection.Missing.Value;
            Word.Range wordRange;
            bool rangeFound = false;
            for (int i = 1; i <= document.Sections.Count; i++)
            {
                wordRange = document.Sections[i].Range;
                Word.Find wordFindObj = wordRange.Find;

                object[] wordFindParameters = new object[15] { findTextObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj, _missingObj };
                rangeFound = (bool)wordFindObj.GetType().InvokeMember("Execute", BindingFlags.InvokeMethod, null, wordFindObj, wordFindParameters);

                if (rangeFound) { return wordRange; }
            }

            return null;
        }

        //Функция для создания отчёта для лабораторной работы по шаблону
        private void createWordDoc()
        {
            Word.Application application = new Word.Application();
            //Путь к файлу шаблона
            Object templatePathObj = path + "report3.dotx";
            Object fileName = path + "LabReport.docx";

            Word.Document WordDocument = application.Documents.Add(templatePathObj);
            object findTextObj;
            Word.Range range;
            Word.InlineShape inline_shape;
            Word.Shape shape;

            if(pictureBox1.Image != null)
            {
                findTextObj = "Исходное изображение";
                range = findText(findTextObj, WordDocument); //ищем Range на текст
                range.End++; //переходим на след строку
                range.Start = range.End;
                inline_shape = range.InlineShapes.AddPicture(path + "source.png"); //вставляем изображение
                shape = inline_shape.ConvertToShape();
                shape.WrapFormat.Type = Word.WdWrapType.wdWrapTopBottom; //обтекание сверху и снизу
                shape.Left = (float)Word.WdShapePosition.wdShapeCenter; //ставим в центр
                shape.Top = (float)Word.WdShapePosition.wdShapeCenter;

                findTextObj = "Черно-белая версия изображения (Grayscale)";
                range = findText(findTextObj, WordDocument); //ищем Range на текст
                range.End++; //переходим на след строку
                range.Start = range.End;
                inline_shape = range.InlineShapes.AddPicture(path + "grayscale.jpg"); //вставляем изображение
                shape = inline_shape.ConvertToShape();
                shape.WrapFormat.Type = Word.WdWrapType.wdWrapTopBottom; //обтекание сверху и снизу
                shape.Left = (float)Word.WdShapePosition.wdShapeCenter; //ставим в центр
                shape.Top = (float)Word.WdShapePosition.wdShapeCenter;
            }
            //Для вставки всех рассчетов и необходимых данных я обновляю значения полей DocVariables в шаблоне

            //Первичный алфавит и алфавит после квантования
            WordDocument.Variables["target"].Value = target;
            WordDocument.Variables["quant"].Value = quant;
            //Равномерное кодирование и первичные алфавит
            WordDocument.Variables["binaryCode"].Value = BinaryCode;
            WordDocument.Variables["binaryCodeLength"].Value = BinaryCode.Length.ToString();
            WordDocument.Variables["primaryH"].Value = Hprimary.ToString();
            WordDocument.Variables["primaryI"].Value = I.ToString();
            WordDocument.Variables["primaryCount"].Value = primaryAlphabet.ToString();
            WordDocument.Variables["binaryCodeAverageLength"].Value = avLen.ToString();
            WordDocument.Variables["minLength"].Value = minLen.ToString();
            WordDocument.Variables["freqs"].Value = freqsAndValues;
            //Huffman
            WordDocument.Variables["huffcode"].Value = huffcode;
            WordDocument.Variables["hufflength"].Value = huffcode.Length.ToString();
            WordDocument.Variables["huffI"].Value = huffI.ToString();
            WordDocument.Variables["huffK"].Value = Khuffman.ToString();
            WordDocument.Variables["huffcount"].Value = "2";
            WordDocument.Variables["huffAvLen"].Value = Huff.averageLen.ToString();
            WordDocument.Variables["huffMinLen"].Value = Huff.minLen.ToString();
            WordDocument.Variables["huffQ"].Value = Qhuffman.ToString();
            WordDocument.Variables["hufffreqs"].Value = Huff.code;

            findTextObj = "Кодовое дерево Хаффмана:";
            range = findText(findTextObj, WordDocument); //ищем Range на текст
            range.End++; //переходим на след строку
            range.Start = range.End;
            inline_shape = range.InlineShapes.AddPicture(path + "huffmantree.png"); //вставляем изображение
            shape = inline_shape.ConvertToShape();
            shape.WrapFormat.Type = Word.WdWrapType.wdWrapTopBottom; //обтекание сверху и снизу
            shape.Left = (float)Word.WdShapePosition.wdShapeCenter; //ставим в центр
            shape.Top = (float)Word.WdShapePosition.wdShapeCenter;
            //Fano
            WordDocument.Variables["fanocode"].Value = fanocode;
            WordDocument.Variables["fanolength"].Value = fanocode.Length.ToString();
            WordDocument.Variables["fanoI"].Value = fanoI.ToString();
            WordDocument.Variables["fanoK"].Value = Kfano.ToString();
            WordDocument.Variables["fanocount"].Value = "2";
            WordDocument.Variables["fanoAvLen"].Value = Fano.avLen.ToString();
            WordDocument.Variables["fanoMinLen"].Value = Fano.minLen.ToString();
            WordDocument.Variables["fanoQ"].Value = Qfano.ToString();
            WordDocument.Variables["fanofreqs"].Value = Fano.code;

            findTextObj = "Кодовое дерево Фано:";
            range = findText(findTextObj, WordDocument); //ищем Range на текст
            range.End++; //переходим на след строку
            range.Start = range.End;
            inline_shape = range.InlineShapes.AddPicture(path + "shenontree.png"); //вставляем изображение
            shape = inline_shape.ConvertToShape();
            shape.WrapFormat.Type = Word.WdWrapType.wdWrapTopBottom; //обтекание сверху и снизу
            shape.Left = (float)Word.WdShapePosition.wdShapeCenter; //ставим в центр
            shape.Top = (float)Word.WdShapePosition.wdShapeCenter;

            WordDocument.Fields.Update();//Обновляем переменные в документе
            WordDocument.SaveAs2(fileName);//Сохраняем
            WordDocument.Close();
            application.Quit();
        }


    }
}
