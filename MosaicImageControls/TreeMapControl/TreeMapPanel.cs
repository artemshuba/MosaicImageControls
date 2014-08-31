using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MosaicImageControls.TreeMapControl
{
    public class TreeMapPanel : Panel
    {
        private Rect _emptyArea;
        private double _totalWeight = 0;
        private readonly List<TreeMapItem> _items = new List<TreeMapItem>();

        protected enum RowOrientation
        {
            Horizontal,
            Vertical
        }

        #region Weight property

        public static readonly DependencyProperty WeightProperty = DependencyProperty.RegisterAttached(
            "Weight", typeof(double), typeof(TreeMapPanel), new PropertyMetadata(1.0d, WeightPropertyChanged));

        public static void SetWeight(DependencyObject element, double value)
        {
            element.SetValue(WeightProperty, value);
        }

        public static double GetWeight(DependencyObject element)
        {
            return (double) element.GetValue(WeightProperty);
        }

        private static void WeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FrameworkElement) d;
            control.InvalidateMeasure();
            control.InvalidateArrange();
        }

        #endregion

        protected Rect EmptyArea
        {
            get { return _emptyArea; }
            set { _emptyArea = value; }
        }

        protected List<TreeMapItem> Items
        {
            get { return _items; }
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var item in Items)
                item.UIElement.Arrange(new Rect(item.ComputedLocation, item.ComputedSize));
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.EmptyArea = new Rect(0, 0, availableSize.Width, availableSize.Height);
            PrepareItems();

            double area = this.EmptyArea.Width * this.EmptyArea.Height;
            foreach (var item in Items)
                item.RealArea = area * item.Weight / _totalWeight;

            ComputeBounds();

            foreach (var child in Items)
            {
                if (this.IsValidSize(child.ComputedSize))
                    child.UIElement.Measure(child.ComputedSize);
                else
                    child.UIElement.Measure(new Size(0, 0));
            }

            return availableSize;
        }


        protected RowOrientation GetOrientation()
        {
            return (this.EmptyArea.Width > this.EmptyArea.Height ? RowOrientation.Horizontal : RowOrientation.Vertical);
        }

        protected virtual Rect GetRectangle(RowOrientation orientation, TreeMapItem item, double x, double y, double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
                return new Rect(x, y, item.RealArea / height, height);
            else
                return new Rect(x, y, width, item.RealArea / width);
        }

        protected double GetShortestSide()
        {
            return Math.Min(this.EmptyArea.Width, this.EmptyArea.Height);
        }

        protected virtual void ComputeBounds()
        {
            this.ComputeTreeMaps(Items);
        }

        protected void ComputeTreeMaps(List<TreeMapItem> items)
        {
            RowOrientation orientation = this.GetOrientation();

            double areaSum = 0;

            foreach (var item in items)
                areaSum += item.RealArea;

            Rect currentRow;
            if (orientation == RowOrientation.Horizontal)
            {
                currentRow = new Rect(_emptyArea.X, _emptyArea.Y, areaSum / _emptyArea.Height, _emptyArea.Height);
                _emptyArea = new Rect(_emptyArea.X + currentRow.Width, _emptyArea.Y, Math.Max(0, _emptyArea.Width - currentRow.Width), _emptyArea.Height);
            }
            else
            {
                currentRow = new Rect(_emptyArea.X, _emptyArea.Y, _emptyArea.Width, areaSum / _emptyArea.Width);
                _emptyArea = new Rect(_emptyArea.X, _emptyArea.Y + currentRow.Height, _emptyArea.Width, Math.Max(0, _emptyArea.Height - currentRow.Height));
            }

            double prevX = currentRow.X;
            double prevY = currentRow.Y;

            foreach (var item in items)
            {
                Rect rect = this.GetRectangle(orientation, item, prevX, prevY, currentRow.Width, currentRow.Height);

                item.AspectRatio = rect.Width / rect.Height;
                item.ComputedSize = new Size(rect.Width, rect.Height);
                item.ComputedLocation = new Point(rect.X, rect.Y);

                ComputeNextPosition(orientation, ref prevX, ref prevY, rect.Width, rect.Height);
            }
        }

        protected virtual void ComputeNextPosition(RowOrientation orientation, ref double xPos, ref double yPos, double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
                xPos += width;
            else
                yPos += height;
        }

        private void PrepareItems()
        {
            _totalWeight = 0;
            Items.Clear();

            foreach (var child in Children)
            {
                var element = new TreeMapItem(child, GetWeight(child));
                if (this.IsValidItem(element))
                {
                    _totalWeight += element.Weight;
                    Items.Add(element);
                }
                else
                {
                    element.ComputedSize = Size.Empty;
                    element.ComputedLocation = new Point(0, 0);
                    element.UIElement.Measure(element.ComputedSize);
                    element.UIElement.Visibility = Visibility.Collapsed;
                }
            }

            Items.Sort(TreeMapItem.CompareByValueDecreasing);
        }

        private bool IsValidItem(TreeMapItem item)
        {
            return (item != null && !double.IsNaN(item.Weight) && Math.Round(item.Weight, 0) != 0);
        }

        private bool IsValidSize(Size size)
        {
            return (!size.IsEmpty && size.Width > 0 && !double.IsNaN(size.Width) && size.Height > 0 && !double.IsNaN(size.Height));
        }
    }
}
