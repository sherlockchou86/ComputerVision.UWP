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
using Windows.UI;
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
        string key = "84cd5c6bd46f445ba8d75225d8711882";  //API key
        Size size_image;  //当前图片实际size
        AnalysisResult thisresult;  //当前分析结果
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
                cvasMain.Children.Clear();
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
                        size_image = new Size(image.PixelWidth, image.PixelHeight);

                        ringLoading.IsActive = true;

                        //请求API
                        VisionServiceClient client = new VisionServiceClient(key);
                        var feature = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Faces, VisualFeature.Description, VisualFeature.Adult, VisualFeature.Categories };

                        var result = await client.AnalyzeImageAsync(stream_send.AsStream(), feature);
                        thisresult = result;
                        if (result != null)
                        {
                            DisplayData(result);
                        }
                        ringLoading.IsActive = false;
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
                        size_image = new Size(image.PixelWidth, image.PixelHeight);

                        ringLoading.IsActive = true;
                        //请求API
                        VisionServiceClient client = new VisionServiceClient(key);
                        var feature = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Faces, VisualFeature.Description, VisualFeature.Adult, VisualFeature.Categories };

                        var result = await client.AnalyzeImageAsync(stream_send.AsStream(), feature);
                        thisresult = result;
                        if (result != null)
                        {
                            DisplayData(result);
                        }
                        ringLoading.IsActive = false;
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
                        ringLoading.IsActive = true;
                    }
                }
            }
        }

        /// <summary>
        /// 显示数据到界面
        /// </summary>
        /// <param name="result"></param>
        private void DisplayData(AnalysisResult result, bool init = true)
        {
            if (result == null)
                return;

            cvasMain.Children.Clear();
            var offset_h = 0.0; var offset_w = 0.0;
            var p = 0.0;
            var d = cvasMain.ActualHeight / cvasMain.ActualWidth;
            var d2 = size_image.Height / size_image.Width;
            if (d < d2)
            {
                offset_h = 0;
                offset_w = (cvasMain.ActualWidth - cvasMain.ActualHeight / d2) / 2;
                p = cvasMain.ActualHeight / size_image.Height;
            }
            else
            {
                offset_w = 0;
                offset_h = (cvasMain.ActualHeight - cvasMain.ActualWidth / d2) / 2;
                p = cvasMain.ActualWidth / size_image.Width;
            }
            if (result.Faces != null)
            {
                //将face矩形显示到界面（如果有）
                foreach (var face in result.Faces)
                {
                    Windows.UI.Xaml.Shapes.Rectangle rect = new Windows.UI.Xaml.Shapes.Rectangle();
                    rect.Width = face.FaceRectangle.Width * p;
                    rect.Height = face.FaceRectangle.Height * p;
                    Canvas.SetLeft(rect, face.FaceRectangle.Left * p + offset_w);
                    Canvas.SetTop(rect, face.FaceRectangle.Top * p + offset_h);
                    rect.Stroke = new SolidColorBrush(Colors.Blue);
                    rect.StrokeThickness = 3;

                    cvasMain.Children.Add(rect);
                }
            }
            if (!init)
                return;

            //将其他数据显示到表格
            if (result.Description != null && result.Description.Captions != null) //描述
            {
                txtDesc.Text = result.Description.Captions[0].Text;
                txtDesc_Score.Text = Math.Round(result.Description.Captions[0].Confidence, 3).ToString();
            }
            if (result.Adult != null)  //是否成人内容
            {
                txtAdult.Text = result.Adult.IsAdultContent.ToString();
                txtAdult_Score.Text = Math.Round(result.Adult.AdultScore, 3).ToString();

                txtRacy.Text = result.Adult.IsRacyContent.ToString();
                txtRacy_Score.Text = Math.Round(result.Adult.RacyScore, 3).ToString();
            }

            var list_child = gridTags.Children.ToList();  //移除之前Tag数据
            list_child.ForEach((e) => 
                {
                    if (e as TextBlock != null && (e as TextBlock).Tag != null)
                    {
                        gridTags.Children.Remove(e);
                    }
                });

            list_child = gridFaces.Children.ToList();  //移除之前Face数据
            list_child.ForEach((e) =>
                {
                    if (e as TextBlock != null && (e as TextBlock).Tag != null)
                    {
                        gridFaces.Children.Remove(e);
                    }
                });

            if (result.Tags != null)  //Tag
            {
                int index = 1;
                foreach (var tag in result.Tags)
                {

                    TextBlock txt0 = new TextBlock();  //#
                    txt0.Text = "0" + index;
                    txt0.Padding = new Thickness(0);
                    Grid.SetRow(txt0, index + 1);
                    Grid.SetColumn(txt0, 0);
                    txt0.Tag = true;

                    TextBlock txt1 = new TextBlock();  //Tag Name
                    txt1.Text = tag.Name;
                    txt1.Padding = new Thickness(1);
                    Grid.SetRow(txt1, index + 1);
                    Grid.SetColumn(txt1, 1);
                    txt1.Tag = true;

                    TextBlock txt2 = new TextBlock();  //Tag Confidence
                    txt2.Text = Math.Round(tag.Confidence, 3).ToString();
                    txt2.Padding = new Thickness(1);
                    Grid.SetRow(txt2, index + 1);
                    Grid.SetColumn(txt2, 2);
                    txt2.Tag = true;

                    index++;

                    gridTags.Children.Add(txt0);
                    gridTags.Children.Add(txt1);
                    gridTags.Children.Add(txt2);
                }
            }

            if (result.Faces != null)  //faces
            {
                int index = 1;
                foreach (var face in result.Faces)
                {
                    TextBlock txt0 = new TextBlock();  //#
                    txt0.Text = "0" + index;
                    txt0.Padding = new Thickness(0);
                    Grid.SetRow(txt0, index + 1);
                    Grid.SetColumn(txt0, 0);
                    txt0.Tag = true;

                    TextBlock txt1 = new TextBlock();  //Age
                    txt1.Text = face.Age.ToString();
                    txt1.Padding = new Thickness(1);
                    Grid.SetRow(txt1, index + 1);
                    Grid.SetColumn(txt1, 1);
                    txt1.Tag = true;

                    TextBlock txt2 = new TextBlock();  //Sex
                    txt2.Text = face.Gender;
                    txt2.Padding = new Thickness(1);
                    Grid.SetRow(txt2, index + 1);
                    Grid.SetColumn(txt2, 2);
                    txt2.Tag = true;

                    index++;

                    gridFaces.Children.Add(txt0);
                    gridFaces.Children.Add(txt1);
                    gridFaces.Children.Add(txt2);
                }
            }

        }
        /// <summary>
        /// 尺寸改变时，重新绘制Canvas中的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cvasMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DisplayData(thisresult, false);
        }
        /// <summary>
        /// 粘贴URL时  图片加载完毕后保存图片尺寸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void imgPhoto_ImageOpened(object sender, RoutedEventArgs e)
        {
            size_image = new Size((imgPhoto.Source as BitmapImage).PixelWidth, (imgPhoto.Source as BitmapImage).PixelHeight);

            //请求API
            VisionServiceClient client = new VisionServiceClient(key);
            var feature = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Faces, VisualFeature.Description, VisualFeature.Adult, VisualFeature.Categories };

            var result = await client.AnalyzeImageAsync(txtLocation.Text, feature);
            thisresult = result;
            if (result != null)
            {
                DisplayData(result);
            }
            ringLoading.IsActive = false;
        }
    }
}
