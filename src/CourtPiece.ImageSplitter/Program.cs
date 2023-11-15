// See https://aka.ms/new-console-template for more information
using System.Drawing;

Console.WriteLine("Hello, World!");

foreach (var file in new string[] { "diamond.jpg","club.jpg","hearts.jpg","spades.jpg" })
{
    var bitmap = new Bitmap(file);
    var w = bitmap.Width / 5 - 13;
    var h = bitmap.Height / 3 - 38;

    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 5; j++)
        {
            var n = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    n.SetPixel(x, y, bitmap.GetPixel(x + j * w + j * 16, y + i * h + i * 15));
                }

            n.Save($"D:\\Playing Cards\\{file[0]}{j + 1 + i * 5}.jpg");
            //i += j * 13;
        }
        //j += j * 38;
    }
}


