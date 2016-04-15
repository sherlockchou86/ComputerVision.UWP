using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
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
using Windows.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComputerVision.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FacePage : Page
    {
        public FacePage()
        {
            this.InitializeComponent();
        }
        string key_face = "33a1073f52b94e07882eab6f66cdb33a";
        string key_emotion = "0f583131728d430faa769ea1ed5e7e7a";

        Size size_image;
        Face[] faces;
        Emotion[] emotions;

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
                        var stream_send2 = stream.CloneStream();
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        imgPhoto.Source = image;
                        size_image = new Size(image.PixelWidth, image.PixelHeight);

                        ringLoading.IsActive = true;

                        //请求API
                        FaceServiceClient f_client = new FaceServiceClient(key_face);
                        EmotionServiceClient e_client = new EmotionServiceClient(key_emotion);

                        var requiedFaceAttributes = new FaceAttributeType[] {
                                FaceAttributeType.Age,
                                FaceAttributeType.Gender,
                                FaceAttributeType.Smile,
                                FaceAttributeType.FacialHair,
                                FaceAttributeType.HeadPose,
                                FaceAttributeType.Glasses
                                };
                        var faces_task = f_client.DetectAsync(stream_send.AsStream(), true, true, requiedFaceAttributes);
                        var emotion_task = e_client.RecognizeAsync(stream_send2.AsStream());

                        var faces = await faces_task;
                        var emotions = await emotion_task;

                        if (faces != null)
                        {
                            DisplayFacesData(faces);
                        }
                        if (emotions != null)
                        {
                            DisplayEmotionsData(emotions);
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
                        var stream_send2 = stream.CloneStream();
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        imgPhoto.Source = image;
                        size_image = new Size(image.PixelWidth, image.PixelHeight);

                        ringLoading.IsActive = true;
                        //请求API
                        FaceServiceClient f_client = new FaceServiceClient(key_face);
                        EmotionServiceClient e_client = new EmotionServiceClient(key_emotion);

                        var requiedFaceAttributes = new FaceAttributeType[] {
                                FaceAttributeType.Age,
                                FaceAttributeType.Gender,
                                FaceAttributeType.Smile,
                                FaceAttributeType.FacialHair,
                                FaceAttributeType.HeadPose,
                                FaceAttributeType.Glasses
                                };
                        var faces_task = f_client.DetectAsync(stream_send.AsStream(), true, true, requiedFaceAttributes);
                        var emotion_task = e_client.RecognizeAsync(stream_send2.AsStream());

                        var faces = await faces_task;
                        var emotions = await emotion_task;

                        if (faces != null)
                        {
                            DisplayFacesData(faces);
                        }
                        if (emotions != null)
                        {
                            DisplayEmotionsData(emotions);
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
        /// 显示Face数据到界面
        /// </summary>
        /// <param name="result"></param>
        private void DisplayFacesData(Face[] faces, bool init = true)
        {
            if (faces == null)
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
            if (faces != null)
            {
                //将face矩形显示到界面（如果有）
                foreach (var face in faces)
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

            var list_child = gridFaces.Children.ToList();  //移除之前Face数据
            list_child.ForEach((e) =>
            {
                if (e as TextBlock != null && (e as TextBlock).Tag != null)
                {
                    gridFaces.Children.Remove(e);
                }
            });

            list_child = gridEmotions.Children.ToList();  //移除之前Emotion数据
            list_child.ForEach((e) =>
            {
                if (e as TextBlock != null && (e as TextBlock).Tag != null)
                {
                    gridEmotions.Children.Remove(e);
                }
            });

            foreach (var face in faces)
            {

            }
        }
        /// <summary>
        /// 显示Emotions数据到界面
        /// </summary>
        /// <param name="emotions"></param>
        private void DisplayEmotionsData(Emotion[] emotions, bool init = true)
        {

        }
        /// <summary>
        /// 粘贴URL时 图片加载完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void imgPhoto_ImageOpened(object sender, RoutedEventArgs e)
        {
            size_image = new Size((imgPhoto.Source as BitmapImage).PixelWidth, (imgPhoto.Source as BitmapImage).PixelHeight);

            //请求API
            FaceServiceClient f_client = new FaceServiceClient(key_face);
            EmotionServiceClient e_client = new EmotionServiceClient(key_emotion);

            var requiedFaceAttributes = new FaceAttributeType[] {
                                FaceAttributeType.Age,
                                FaceAttributeType.Gender,
                                FaceAttributeType.Smile,
                                FaceAttributeType.FacialHair,
                                FaceAttributeType.HeadPose,
                                FaceAttributeType.Glasses
                                };
            var faces_task = f_client.DetectAsync(txtLocation.Text, true, true, requiedFaceAttributes);
            var emotion_task = e_client.RecognizeAsync(txtLocation.Text);

            var faces = await faces_task;
            var emotions = await emotion_task;

            if (faces != null)
            {
                DisplayFacesData(faces);
            }
            if (emotions != null)
            {
                DisplayEmotionsData(emotions);
            }
            ringLoading.IsActive = false;
        }
        /// <summary>
        /// 尺寸改变 重新绘制界面中Face 矩形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cvasMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DisplayFacesData(faces, false);
            DisplayEmotionsData(emotions, false);
        }
    }
}
