using System.Collections.Generic;
using Windows.Foundation;

namespace MosaicImageControls.MosaicImageControl
{
    public class MosaicRow
    {
        public Point Location { get; set; }

        public Size Size { get; set; }

        public Size DesiredSize { get; set; }

        internal List<MosaicItem1> Items { get; set; }

        public MosaicRow()
        {
            Items = new List<MosaicItem1>();
        }
    }
}
