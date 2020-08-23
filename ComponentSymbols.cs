using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Grid;

namespace ResourceManager
{
    public static class ComponentSymbols
    {
        public static Dictionary<Type, ImageBrush> SymbolList = VisualGrid.SymbolList;
        public static string filePath = @"C:\Users\Czone\source\repos\Nodes\ouput.txt";
        public static bool makeTransparnt = true;
        static Output outputFileHandler = new Output(filePath);

        public static Dictionary<Type, String> SymbolDirectoryList = new Dictionary<Type, string>()
        {
            {typeof(Resistor), "pack://application:,,,/ResourceManager;component/Resources/resistor.png" },
            {typeof(VoltageSource), "pack://application:,,,/ResourceManager;component/Resources/voltage_independent.png" },
            {typeof(CurrentSource), "pack://application:,,,/ResourceManager;component/Resources/current_independent.png" },
            {typeof(Capacitor), "pack://application:,,,/ResourceManager;component/Resources/capacitor.png" },
            {typeof(Inductor), "pack://application:,,,/ResourceManager;component/Resources/meme1.png" },
            {typeof(GND), "pack://application:,,,/ResourceManager;component/Resources/gnd.png" },
            {typeof(Probe), "pack://application:,,,/ResourceManager;component/Resources/probeIndicator.png" }
        };

        /// <summary>
        /// Loads symbols for given list of components
        /// </summary>
        /// <param name="components"></param>
        /// <param name="Direcotory"></param>
        public static void LoadSymbols()
        {
            SymbolList = new Dictionary<Type, ImageBrush>();
            VisualGrid.SymbolList = SymbolList;
            foreach (Type t in SymbolDirectoryList.Keys)
            {
                SymbolList.Add(t, LoadImage(SymbolDirectoryList[t]));
            }
        }

        static ImageBrush LoadImage(string Dir)
        {
            BitmapImage bmp = new BitmapImage();

            bmp.BeginInit();
            bmp.UriSource = new Uri(Dir);
            bmp.DecodePixelWidth = 100;
            bmp.DecodePixelHeight = 100;
            bmp.EndInit();

            FormatConvertedBitmap convBMP = new FormatConvertedBitmap();
            convBMP.BeginInit();
            convBMP.Source = bmp;
            convBMP.DestinationFormat = PixelFormats.Bgra32;
            Console.WriteLine(convBMP.Source.Format);
            convBMP.EndInit();

            ImageBrush ibrush = new ImageBrush();
            ibrush.ImageSource = CreateTransparent(convBMP, makeTransparnt);
            return ibrush;
        }

        public static List<Type> GetSymbolList()
        {
            List<Type> sl = new List<Type>();
            foreach (Type t in SymbolList.Keys)
            {
                sl.Add(t);
            }
            return sl;
        }

        static BitmapSource CreateTransparent(FormatConvertedBitmap source, bool makeTransparent)
        {
            if (source.Format != PixelFormats.Bgra32)
            {
                Console.WriteLine("Could not convert image");
                return source;
            }

            var bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            var stride = bytesPerPixel * source.PixelWidth;
            var buffer = new byte[stride * source.PixelHeight];

            source.CopyPixels(buffer, stride, 0);

            if (makeTransparnt)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    for (int x = 0; x < source.PixelWidth; x++)
                    {
                        var i = stride * y + bytesPerPixel * x;
                        var b = buffer[i];
                        var g = buffer[i + 1];
                        var r = buffer[i + 2];
                        var a = buffer[i + 3];
                        //Console.WriteLine(r);
                        if (r > 100 && g > 100 && b > 100)
                        {
                            buffer[i + 3] = 0;
                        }
                        
                        else
                        {
                            buffer[i] = 0;
                            buffer[i + 1] = 0;
                            buffer[i + 2] = 0;
                        }
                        
                    }
                }
            }
            Console.WriteLine("done");
            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY,
                source.Format, null, buffer, stride);
        }
    }
}
