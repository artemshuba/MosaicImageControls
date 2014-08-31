using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MosaicImageControlDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void AddButtonClick_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            var files = await picker.PickMultipleFilesAsync();

            TreeMap.Children.Clear();

            var sources = new List<BitmapImage>();
            foreach (var file in files)
            {
                var stream = (await file.OpenStreamForReadAsync()).AsRandomAccessStream();//await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.PicturesView, 300);
                var imageSource = new BitmapImage();
                await imageSource.SetSourceAsync(stream);

                sources.Add(imageSource);

                var image = new Image();
                image.Stretch = Stretch.UniformToFill;
                image.Source = imageSource;
                TreeMap.Children.Add(image);
            }

            MosaicImage.Source = sources;
        }
    }
}
