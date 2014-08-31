using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace MosaicImageControls.MosaicImageControl
{
    internal class MosaicItem
    {
        public Rect Location { get; set; }

        public int Row { get; set; }

        public MosaicItem()
        {

        }

        public MosaicItem(Rect location, int row)
        {
            Location = location;
            Row = row;
        }
    }

    internal class MosaicItem1
    {
        public Point ComputedLocation { get; set; }

        public Size ComputedSize { get; set; }

        public Size OriginalSize { get; set; }

        public Image Image { get; set; }

        public MosaicItem1(Image image, Size originalSize)
        {
            Image = image;
            OriginalSize = originalSize;
        }
    }
}
