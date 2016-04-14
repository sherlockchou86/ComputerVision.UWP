using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComputerVision.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoPage : Page
    {
        public PhotoPage()
        {
            this.InitializeComponent();            
        }
        string key = "84cd5c6bd46f445ba8d75225d8711882";
        /// <summary>
        /// 菜单点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menu = sender as MenuFlyoutItem;
            if (menu != null)
            {
                if (menu.Text.Equals("From Camera"))  //照相
                {
                    CameraCaptureUI captureUI = new CameraCaptureUI();
                    captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
                    captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
                    StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
                    if (photo != null)
                    {
                        var stream = await photo.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        var stream_send = stream.CloneStream();
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        imgPhoto.Source = image;

                        //请求API
                        VisionServiceClient client = new VisionServiceClient(key);
                        var feature = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Faces, VisualFeature.Description, VisualFeature.Adult, VisualFeature.Categories };

                        var result = await client.AnalyzeImageAsync(stream_send.AsStream(), feature);
                        if (result != null)
                        {
                            DisplayData(result);
                        }
                    }
                }
                else if (menu.Text.Equals("From Album")) //从相册里选
                {
                    FileOpenPicker openPicker = new FileOpenPicker();
                    openPicker.ViewMode = PickerViewMode.Thumbnail;
                    openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    openPicker.FileTypeFilter.Add(".jpg");
                    openPicker.FileTypeFilter.Add(".png");
                    openPicker.FileTypeFilter.Add(".bmp");
                    StorageFile file = await openPicker.PickSingleFileAsync();
                    if (file != null)
                    {
                        var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        var stream_send = stream.CloneStream();
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        imgPhoto.Source = image;

                        //请求API
                        VisionServiceClient client = new VisionServiceClient(key);
                        var feature = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Faces, VisualFeature.Description, VisualFeature.Adult, VisualFeature.Categories };

                        var result = await client.AnalyzeImageAsync(stream_send.AsStream(), feature);
                        if (result != null)
                        {
                            DisplayData(result);
                        }
                    }
                }
                else  //粘贴URL
                {
                    var content = Clipboard.GetContent();
                    if (content != null && content.Contains(StandardDataFormats.Text))
                    {
                        var url = await content.GetTextAsync();
                        txtLocation.Text = url;
                        imgPhoto.Source = new BitmapImage(new Uri(txtLocation.Text));

                        //请求API
                        VisionServiceClient client = new VisionServiceClient(key);
                        var feature = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Faces, VisualFeature.Description, VisualFeature.Adult, VisualFeature.Categories };

                        var result = await client.AnalyzeImageAsync(url, feature);
                        if (result != null)
                        {
                            DisplayData(result);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 显示数据到界面
        /// </summary>
        /// <param name="result"></param>
        private void DisplayData(AnalysisResult result)
        {

        }
    }
}
