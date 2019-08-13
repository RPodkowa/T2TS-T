using System.Collections.Generic;
using System.Drawing;
using Thea2Translator.Logic.Cache.Interfaces;

namespace Thea2Translator.Logic
{    
    public class StatusRectangle
    {
        public StatusRectangleType Type;
        public Rectangle Rectangle;
        public Color Background;
        public Color Bar;
    }

    public enum StatusRectangleType
    {
        Summary,
        TranslatedBar,
        ConfirmedBar
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public class ImageHelper
    {        
        private static Color FrameSummaryColor = Color.FromArgb(255, 0, 0);
        private static Color FrameTranslatedColor = Color.FromArgb(0, 255, 255);
        private static Color FrameConfirmedColor = Color.FromArgb(255, 0, 255);

        private static bool IsFrameColor(Color color)
        {
            return IsSameColor(color, FrameSummaryColor) || IsSameColor(color, FrameTranslatedColor) || IsSameColor(color, FrameConfirmedColor);
        }

        private static bool IsSameColor(Color color1, Color color2)
        {
            return (color1.R == color2.R && color1.G == color2.G && color1.B == color2.B);
        }

        private static StatusRectangleType GetStatusRectangleTypeByColor(Color color)
        {
            if (IsSameColor(color, FrameSummaryColor))
                return StatusRectangleType.Summary;

            if (IsSameColor(color, FrameTranslatedColor))
                return StatusRectangleType.TranslatedBar;

            return StatusRectangleType.ConfirmedBar;
        }

        public static void MakeStatisticFile(IStatistic statistic)
        {            
            var scrBitmap = new Bitmap(Resource.StatusDatabase);
            if (statistic.Type == FilesType.Modules) scrBitmap = new Bitmap(Resource.StatusDatabase);
            var statusRectangles = FindStatusRectangles(scrBitmap);

            foreach (var statusRectangle in statusRectangles)
            {
                DrawStatusRectangle(statusRectangle, statistic, scrBitmap);
            }

            scrBitmap.Save(@"D:\file.png");
        }

        private static void DrawStatusRectangle(StatusRectangle statusRectangle, IStatistic statistic, Bitmap srcBitmap)
        {
            if (statusRectangle.Type == StatusRectangleType.Summary) DrawSummaryStatusRectangle(statusRectangle, statistic, srcBitmap);
            if (statusRectangle.Type == StatusRectangleType.TranslatedBar) DrawProgressBarStatusRectangle(statusRectangle, statistic.TranslatedPercent, srcBitmap);
            if (statusRectangle.Type == StatusRectangleType.ConfirmedBar) DrawProgressBarStatusRectangle(statusRectangle, statistic.ConfirmedPercent, srcBitmap);
        }

        private static void DrawProgressBarStatusRectangle(StatusRectangle statusRectangle, int percent, Bitmap srcBitmap)
        {
            using (Graphics gr = Graphics.FromImage(srcBitmap))
            {
                int lenght = statusRectangle.Rectangle.Width;
                lenght = (int)(((double)(percent) / 100) * lenght);
                var progressRectangle = new Rectangle(statusRectangle.Rectangle.Location, new Size(lenght, statusRectangle.Rectangle.Height));
                var brush = new SolidBrush(statusRectangle.Bar);                
                gr.FillRectangle(brush, progressRectangle);
            }
        }

        private static void DrawSummaryStatusRectangle(StatusRectangle statusRectangle, IStatistic statistic, Bitmap srcBitmap)
        {
            using (Graphics gr = Graphics.FromImage(srcBitmap))
            {
                using (Font font = new Font("PT Serif", 10))
                {
                    var brush = new SolidBrush(Color.FromArgb(255, 245, 204));
                    gr.DrawString(statistic.GetSummary(), font, brush, statusRectangle.Rectangle);
                }
            }
        }

        private static List<StatusRectangle> FindStatusRectangles(Bitmap srcBitmap)
        {
            var ret = new List<StatusRectangle>();

            for (int x = 0; x < srcBitmap.Width; x++)
            {
                for (int y = 0; y < srcBitmap.Height; y++)
                {
                    var pixel = srcBitmap.GetPixel(x, y);
                    if (IsFrameColor(pixel))
                    {
                        var statusRectangle = TryFindStatusRectangle(srcBitmap, x, y, pixel);
                        if (statusRectangle != null) ret.Add(statusRectangle);
                    }
                }
            }

            return ret;
        }

        private static StatusRectangle TryFindStatusRectangle(Bitmap srcBitmap, int x, int y, Color color)
        {
            var tl = new Point(x, y);
            var right = GetLenght(srcBitmap, tl, color, Direction.Right);
            var tr = new Point(tl.X + right, tl.Y);
            var down = GetLenght(srcBitmap, tr, color, Direction.Down);
            var br = new Point(tr.X, tr.Y + down);
            var left = GetLenght(srcBitmap, br, color, Direction.Left);
            var bl = new Point(br.X - left, br.Y);
            var up = GetLenght(srcBitmap, bl, color, Direction.Up);
            var tl2 = new Point(bl.X, bl.Y - up);

            if (tl != tl2)
                return null;

            var ret = new StatusRectangle();
            ret.Rectangle = new Rectangle(tl.X, tl.Y, right + 1, down + 1);
            ret.Type = GetStatusRectangleTypeByColor(color);
            ret.Bar = srcBitmap.GetPixel(tl.X + 1, tl.Y + 1);
            ret.Background = srcBitmap.GetPixel(tl.X + 2, tl.Y + 2);

            EraseRectangle(ret, srcBitmap);
            return ret;
        }

        private static void EraseRectangle(StatusRectangle statusRectangle, Bitmap srcBitmap)
        {
            using (Graphics gr = Graphics.FromImage(srcBitmap))
            {
                var brush = new SolidBrush(statusRectangle.Background);
                gr.FillRectangle(brush, statusRectangle.Rectangle);
            }
        }

        private static int GetLenght(Bitmap srcBitmap, Point point, Color color, Direction direction)
        {
            var lenght = 0;

            var firstIndex = 0;
            var lastIndex = 0;
            var vertical = false;
            var iterator = 1;

            switch (direction)
            {
                case Direction.Up:
                    firstIndex = point.Y;
                    lastIndex = 0;
                    vertical = true;
                    iterator = -1;                    
                    break;
                case Direction.Down:
                    firstIndex = point.Y;
                    lastIndex = srcBitmap.Height;
                    vertical = true;
                    break;
                case Direction.Left:
                    firstIndex = point.X;
                    lastIndex = 0;
                    iterator = -1;
                    break;
                case Direction.Right:
                    firstIndex = point.X;
                    lastIndex = srcBitmap.Width;
                    break;
            }

            for (int i = firstIndex + iterator; IsOkiIndex(i, lastIndex, iterator); i += iterator)
            {
                int x = point.X;
                int y = point.Y;
                if (vertical) y = i;
                else x = i;

                var pixel = srcBitmap.GetPixel(x, y);
                if (IsSameColor(pixel, color)) lenght++;
                else break;
            }

            return lenght;
        }

        private static bool IsOkiIndex(int index, int lastIndex, int iterator)
        {
            if (iterator < 0) return lastIndex < index;
            return lastIndex > index;
        }

        public static Bitmap ChangeColor(Bitmap scrBitmap)
        {
            //You can change your new color here. Red,Green,LawnGreen any..
            Color newColor = Color.Red;
            Color actualColor;
            //make an empty bitmap the same size as scrBitmap
            Bitmap newBitmap = new Bitmap(scrBitmap.Width, scrBitmap.Height);
            for (int i = 0; i < scrBitmap.Width; i++)
            {
                for (int j = 0; j < scrBitmap.Height; j++)
                {
                    //get the pixel from the scrBitmap image
                    actualColor = scrBitmap.GetPixel(i, j);
                    // > 150 because.. Images edges can be of low pixel colr. if we set all pixel color to new then there will be no smoothness left.
                    if (actualColor.A > 150)
                        newBitmap.SetPixel(i, j, newColor);
                    else
                        newBitmap.SetPixel(i, j, actualColor);
                }
            }
            return newBitmap;
        }
    }
}
