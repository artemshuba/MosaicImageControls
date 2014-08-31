using Windows.Foundation;
using Windows.UI.Xaml;

namespace MosaicImageControls.TreeMapControl
{
    public class TreeMapItem
    {
        private double _weight;
        private double _area;
        private UIElement _element;
        private Size _desiredSize;
        private Point _desiredLocation;
        private double _ratio;

        public TreeMapItem(UIElement element, double weight)
        {
            _element = element;
            _weight = weight;
        }

        internal Size ComputedSize
        {
            get { return _desiredSize; }
            set { _desiredSize = value; }
        }

        internal Point ComputedLocation
        {
            get { return _desiredLocation; }
            set { _desiredLocation = value; }
        }

        public double AspectRatio
        {
            get { return _ratio; }
            set { _ratio = value; }
        }

        public double Weight
        {
            get { return _weight; }
        }

        public double RealArea
        {
            get { return _area; }
            set { _area = value; }
        }

        public UIElement UIElement
        {
            get { return _element; }
        }

        public static int CompareByValueDecreasing(TreeMapItem x, TreeMapItem y)
        {
            if (x == null)
            {
                return y == null ? -1 : 0;
            }
            else
            {
                return y == null ? 1 : x.Weight.CompareTo(y.Weight) * -1;
            }
        }
    }
}
