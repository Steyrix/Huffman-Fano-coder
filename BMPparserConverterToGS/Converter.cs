using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BMPparserConverterToGS
{
    class Converter
    {

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

        static void Main(string[] args)
        {
            const int n = 128;
            Bitmap sourceImage;
            sourceImage = (Bitmap)Image.FromFile(@"img.jpg");
            
            int[] targetpixels = new int[n];
            int[] quanted = new int[n];
            int[] frequency = new int[1000];

            ConvertToGrayScale(sourceImage, targetpixels);
            
            for (int i = 0; i < n; i++)
            {

                Console.WriteLine();
                //Смотрю значения исходных пикселей
                Console.Write("Tpix: " + targetpixels[i]);

                //Формула квантования  X = round(X / 20) * 20
                quanted[i] = (int)Math.Round((float)(targetpixels[i] / 20)) * 20;

                Console.Write(". Qnted: " + quanted[i]);
                frequency[quanted[i]]++;
            }

            
            Console.ReadKey();
        }
    }
}
