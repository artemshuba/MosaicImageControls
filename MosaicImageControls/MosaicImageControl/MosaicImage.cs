using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MosaicImageControls.MosaicImageControl
{
    public class MosaicImage : UserControl
    {
        private readonly Canvas _layoutRoot;
        private List<MosaicItem1> _items = new List<MosaicItem1>();
        private List<MosaicRow> _rows = new List<MosaicRow>(); 

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(IList<ImageSource>), typeof(MosaicImage), new PropertyMetadata(default(IList<BitmapImage>), SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (MosaicImage)d;

            c.PrepareImages();
            c.LayoutImages();
            c.ArrangeImages();
        }

        public IList<BitmapImage> Source
        {
            get { return (IList<BitmapImage>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public double MaxImageSize { get; set; }

        public MosaicImage()
        {
            _layoutRoot = new Canvas();
            Content = _layoutRoot;
            MaxImageSize = 300;

            SizeChanged += MosaicImage_SizeChanged;
        }

        void MosaicImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Source != null)
            {
                PrepareImages();
                LayoutImages();
                ArrangeImages();
            }
        }

        private void PrepareImages()
        {
            _rows.Clear();
            _items.Clear();
            _layoutRoot.Children.Clear();

            if (Source == null)
                return;

            foreach (var bitmapImage in Source)
            {
                var image = new Image();
                image.Stretch = Stretch.UniformToFill;
                image.Source = bitmapImage;

                var item = new MosaicItem1(image, new Size(bitmapImage.PixelWidth, bitmapImage.PixelHeight));
                _items.Add(item);
                _layoutRoot.Children.Add(image);
            }
        }

        private void LayoutImages()
        {
            if (_items.Count == 0)
                return;
            
            var currentRow = new MosaicRow() {Size = new Size(ActualWidth, 0)};
            _rows.Add(currentRow);

            double prevX = 0;
            double prevY = 0;

            foreach (var item in _items)
            {
                currentRow.Items.Add(item);

                //resize item to MaxImageSize if necessary and set ComputedSize
                MeasureItem(item);

                //set row height from the first image in the row
                if (currentRow.Size.Height == 0)
                {
                    currentRow.Size = new Size(currentRow.Size.Width, item.ComputedSize.Height);
                    currentRow.DesiredSize = currentRow.Size;
                }
                else
                {
                    //resize all other items in row to the row height
                    FitToRow(item, currentRow.Size.Height);
                }

                //calculate current item location
                var itemRect = ComputeItemLocation(item, new Rect(prevX, currentRow.Location.Y, currentRow.Size.Width, currentRow.Size.Height));

                currentRow.DesiredSize = new Size(itemRect.Right, currentRow.DesiredSize.Height);

                if (itemRect.Right > currentRow.Size.Width)
                {
                    //LINE BREAK

                    prevX = 0;
                    prevY += currentRow.Size.Height;

                    currentRow = new MosaicRow() { Location = new Point(0, prevY), Size = new Size(ActualWidth, 0) };

                    _rows.Add(currentRow);
                }
                else
                {
                    prevX += itemRect.Width;
                }

                //place item at specified location and set ComputedLocation
                PlaceItem(item, itemRect);
            }

            if (currentRow.Items.Count == 0)
                _rows.Remove(currentRow);

            Height = currentRow.Location.Y + currentRow.Size.Height;
        }

        //final pass to fit all rows to width and fix last row
        private void ArrangeImages()
        {
            if (_items.Count == 0)
                return;

            double prevY = 0;

            foreach (var row in _rows)
            {
                if (row.Location.Y != prevY)
                {
                    PlaceRow(row, new Rect(0, prevY, row.DesiredSize.Width, row.DesiredSize.Height));
                }

                //if current row fits to controls, just skip it
                if (row.DesiredSize.Width == ActualWidth)
                    continue;

                //if row requires more space then available, resize all items to fit
                if (row.DesiredSize.Width > ActualWidth)
                {
                    var resizeWidth = (row.DesiredSize.Width - ActualWidth) / row.Items.Count;
                    double prevX = 0;

                    foreach (var item in row.Items)
                    {
                        item.Image.Width -= resizeWidth;
                        item.Image.Height -= resizeWidth;

                        item.ComputedSize = new Size(item.Image.Width, item.Image.Height);

                        PlaceItem(item, new Rect(prevX, item.ComputedLocation.Y, item.ComputedSize.Width, item.ComputedSize.Height));
                        prevX += item.ComputedSize.Width;
                    }

                    row.DesiredSize = new Size(Width, row.DesiredSize.Height - resizeWidth);
                    row.Size = row.DesiredSize;
                    prevY += row.DesiredSize.Height;
                }
                //if there is empty space in row we can enlarge all items or combine it with previous row
                else
                {
                    var resizeWidth = (ActualWidth - row.DesiredSize.Width) / row.Items.Count;
                    double prevX = 0;

                    foreach (var item in row.Items)
                    {
                        item.Image.Width += resizeWidth;
                        item.Image.Height += resizeWidth;

                        item.ComputedSize = new Size(item.Image.Width, item.Image.Height);

                        PlaceItem(item, new Rect(prevX, item.ComputedLocation.Y, item.ComputedSize.Width, item.ComputedSize.Height));
                        prevX += item.ComputedSize.Width;
                    }

                    row.DesiredSize = new Size(Width, row.DesiredSize.Height + resizeWidth);
                    row.Size = row.DesiredSize;
                    prevY += row.DesiredSize.Height;
                }
            }

            var lastRow = _rows.Last();
            Height = lastRow.Location.Y + lastRow.Size.Height;
        }

        private void MeasureItem(MosaicItem1 item)
        {
            if (item.OriginalSize.Width > MaxImageSize || item.OriginalSize.Height > MaxImageSize)
            {
                var scale = MaxImageSize / (item.OriginalSize.Width > item.OriginalSize.Height ? item.OriginalSize.Width : item.OriginalSize.Height);
                item.Image.Width = item.OriginalSize.Width * scale;
                item.Image.Height = item.OriginalSize.Height * scale;
            }
            else
            {
                item.Image.Width = item.OriginalSize.Width;
                item.Image.Height = item.OriginalSize.Height;
            }

            item.ComputedSize = new Size(item.Image.Width, item.Image.Height);
        }

        private void FitToRow(MosaicItem1 item, double rowHeight)
        {
            var scale = rowHeight / item.ComputedSize.Height;
            item.Image.Width *= scale;
            item.Image.Height *= scale;

            item.ComputedSize = new Size(item.Image.Width, item.Image.Height);
        }

        private void PlaceItem(MosaicItem1 item, Rect targetRect)
        {
            item.ComputedLocation = new Point(targetRect.X, targetRect.Y);

            Canvas.SetLeft(item.Image, targetRect.X);
            Canvas.SetTop(item.Image, targetRect.Y);
        }

        private void PlaceRow(MosaicRow row, Rect targetRect)
        {
            double prevX = targetRect.X;
            foreach (var item in row.Items)
            {
                PlaceItem(item, new Rect(prevX, targetRect.Y, item.ComputedSize.Width, item.ComputedSize.Height));

                prevX += item.ComputedSize.Width;
            }

            row.Location = new Point(targetRect.X, targetRect.Y);
        }

        private Rect ComputeItemLocation(MosaicItem1 item, Rect targetRect)
        {
            return new Rect(targetRect.X, targetRect.Y, item.ComputedSize.Width, item.ComputedSize.Height);
        }
    }
}
