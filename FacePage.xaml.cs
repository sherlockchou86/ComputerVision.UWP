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

                        faces = await faces_task;
                        emotions = await emotion_task;

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

                        faces = await faces_task;
                        emotions = await emotion_task;

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
                int count = 1;
                //将face矩形显示到界面（如果有）
                foreach (var face in faces)
                {
                    Windows.UI.Xaml.Shapes.Rectangle rect = new Windows.UI.Xaml.Shapes.Rectangle();
                    rect.Width = face.FaceRectangle.Width * p;
                    rect.Height = face.FaceRectangle.Height * p;
                    Canvas.SetLeft(rect, face.FaceRectangle.Left * p + offset_w);
                    Canvas.SetTop(rect, face.FaceRectangle.Top * p + offset_h);
                    rect.Stroke = new SolidColorBrush(Colors.Orange);
                    rect.StrokeThickness = 3;

                    cvasMain.Children.Add(rect);

                    TextBlock txt = new TextBlock();
                    txt.Foreground = new SolidColorBrush(Colors.Orange);
                    txt.Text = "#" + count;
                    Canvas.SetLeft(txt, face.FaceRectangle.Left * p + offset_w);
                    Canvas.SetTop(txt, face.FaceRectangle.Top * p + offset_h - 20);
                    cvasMain.Children.Add(txt);
                    count++;
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

            int index = 1;
            foreach (var face in faces)
            {
                TextBlock txt0 = new TextBlock();
                txt0.Text = "0" + index;
                txt0.Padding = new Thickness(1);
                Grid.SetRow(txt0, index + 1);
                Grid.SetColumn(txt0, 0);
                txt0.Tag = true;

                TextBlock txt1 = new TextBlock();
                txt1.Text = Math.Round(face.FaceAttributes.Age, 2).ToString();
                txt1.Padding = new Thickness(1);
                Grid.SetRow(txt1, index + 1);
                Grid.SetColumn(txt1, 1);
                txt1.Tag = true;

                TextBlock txt2 = new TextBlock();
                txt2.Text = face.FaceAttributes.Gender;
                txt2.Padding = new Thickness(1);
                Grid.SetRow(txt2, index + 1);
                Grid.SetColumn(txt2, 2);
                txt2.Tag = true;

                TextBlock txt3 = new TextBlock();
                txt3.Text = Math.Round(face.FaceAttributes.Smile, 2).ToString();
                txt3.Padding = new Thickness(1);
                Grid.SetRow(txt3, index + 1);
                Grid.SetColumn(txt3, 3);
                txt3.Tag = true;

                TextBlock txt4 = new TextBlock();
                txt4.Text = face.FaceAttributes.Glasses.ToString();
                txt4.Padding = new Thickness(1);
                Grid.SetRow(txt4, index + 1);
                Grid.SetColumn(txt4, 4);
                txt4.Tag = true;

                index++;
                gridFaces.Children.Add(txt0);
                gridFaces.Children.Add(txt1);
                gridFaces.Children.Add(txt2);
                gridFaces.Children.Add(txt3);
                gridFaces.Children.Add(txt4);
            }
        }
        /// <summary>
        /// 显示Emotions数据到界面
        /// </summary>
        /// <param name="emotions"></param>
        private void DisplayEmotionsData(Emotion[] emotions, bool init = true)
        {
            if (emotions == null)
                return;
            if (!init)
                return;

            var list_child = gridEmotions.Children.ToList();  //移除之前Emotion数据
            list_child.ForEach((e) =>
            {
                if (e as TextBlock != null && (e as TextBlock).Tag != null)
                {
                    gridEmotions.Children.Remove(e);
                }
            });

            int index = 1;
            foreach (var emotion in emotions)
            {
                TextBlock txt0 = new TextBlock();
                txt0.Padding = new Thickness(1);
                txt0.FontSize = 11;
                txt0.Text = "#" + index;
                Grid.SetRow(txt0, index + 1);
                Grid.SetColumn(txt0, 0);
                txt0.Tag = true;

                TextBlock txt1 = new TextBlock();
                txt1.Padding = new Thickness(1);
                txt1.FontSize = 11;
                txt1.Text = Math.Round(emotion.Scores.Anger, 2).ToString();
                Grid.SetRow(txt1, index + 1);
                Grid.SetColumn(txt1, 1);
                txt1.Tag = true;

                TextBlock txt2 = new TextBlock();
                txt2.Padding = new Thickness(1);
                txt2.FontSize = 11;
                txt2.Text = Math.Round(emotion.Scores.Contempt, 2).ToString();
                Grid.SetRow(txt2, index + 1);
                Grid.SetColumn(txt2, 2);
                txt2.Tag = true;

                TextBlock txt3 = new TextBlock();
                txt3.Padding = new Thickness(1);
                txt3.FontSize = 11;
                txt3.Text = Math.Round(emotion.Scores.Disgust, 2).ToString();
                Grid.SetRow(txt3, index + 1);
                Grid.SetColumn(txt3, 3);
                txt3.Tag = true;

                TextBlock txt4 = new TextBlock();
                txt4.Padding = new Thickness(1);
                txt4.FontSize = 11;
                txt4.Text = Math.Round(emotion.Scores.Fear, 2).ToString();
                Grid.SetRow(txt4, index + 1);
                Grid.SetColumn(txt4, 4);
                txt4.Tag = true;

                TextBlock txt5 = new TextBlock();
                txt5.Padding = new Thickness(1);
                txt5.FontSize = 11;
                txt5.Text = Math.Round(emotion.Scores.Happiness, 2).ToString();
                Grid.SetRow(txt5, index + 1);
                Grid.SetColumn(txt5, 5);
                txt5.Tag = true;

                TextBlock txt6 = new TextBlock();
                txt6.Padding = new Thickness(1);
                txt6.FontSize = 11;
                txt6.Text = Math.Round(emotion.Scores.Neutral, 2).ToString();
                Grid.SetRow(txt6, index + 1);
                Grid.SetColumn(txt6, 6);
                txt6.Tag = true;

                TextBlock txt7 = new TextBlock();
                txt7.Padding = new Thickness(1);
                txt7.FontSize = 11;
                txt7.Text = Math.Round(emotion.Scores.Sadness, 2).ToString();
                Grid.SetRow(txt7, index + 1);
                Grid.SetColumn(txt7, 7);
                txt7.Tag = true;

                TextBlock txt8 = new TextBlock();
                txt8.Padding = new Thickness(1);
                txt8.FontSize = 11;
                txt8.Text = Math.Round(emotion.Scores.Surprise, 2).ToString();
                Grid.SetRow(txt8, index + 1);
                Grid.SetColumn(txt8, 8);
                txt8.Tag = true;

                index++;
                gridEmotions.Children.Add(txt0);
                gridEmotions.Children.Add(txt1);
                gridEmotions.Children.Add(txt2);
                gridEmotions.Children.Add(txt3);
                gridEmotions.Children.Add(txt4);
                gridEmotions.Children.Add(txt5);
                gridEmotions.Children.Add(txt6);
                gridEmotions.Children.Add(txt7);
                gridEmotions.Children.Add(txt8);
            }
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

            faces = await faces_task;
            emotions = await emotion_task;

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
        /// <summary>
        /// 点击face 显示emotion数据折线图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cvasMain_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (emotions != null)
            {
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
                foreach (var emotion in emotions)
                {
                    Rect rect = new Rect();
                    rect.Width = emotion.FaceRectangle.Width * p;
                    rect.Height = emotion.FaceRectangle.Height * p;

                    rect.X = emotion.FaceRectangle.Left * p + offset_w;
                    rect.Y = emotion.FaceRectangle.Top * p + offset_h;

                    Point point = e.GetPosition(cvasMain);
                    if (rect.Contains(point))
                    {
                        EmotionDataControl edc = new EmotionDataControl();
                        var dic = new Dictionary<string, double>
                        {
                            {"Anger",emotion.Scores.Anger },
                            {"Contempt",emotion.Scores.Contempt },
                            {"Disgust",emotion.Scores.Disgust },
                            {"Fear",emotion.Scores.Fear },
                            {"Happiness",emotion.Scores.Happiness },
                            {"Neutral",emotion.Scores.Neutral },
                            {"Sadness",emotion.Scores.Sadness },
                            {"Surprise",emotion.Scores.Surprise },
                        };
                        edc.Data = dic;
                        edc.Width = cvasMain.ActualWidth * 3 / 4;
                        edc.Height = cvasMain.ActualHeight / 3;

                        emotionData.Child = edc;
                        emotionData.VerticalOffset = point.Y;
                        emotionData.HorizontalOffset = cvasMain.ActualWidth / 8;

                        emotionData.IsOpen = true;
                        
                        break;
                    }
                }
            }
        }
    }
}
