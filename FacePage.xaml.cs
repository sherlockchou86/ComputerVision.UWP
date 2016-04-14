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
                        var faces_task = f_client.DetectAsync(url, true, true, requiedFaceAttributes);
                        var emotion_task = e_client.RecognizeAsync(url);

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
                    }
                }
            }
        }


        /// <summary>
        /// 显示Face数据到界面
        /// </summary>
        /// <param name="result"></param>
        private void DisplayFacesData(Face[] faces)
        {

        }
        /// <summary>
        /// 显示Emotions数据到界面
        /// </summary>
        /// <param name="emotions"></param>
        private void DisplayEmotionsData(Emotion[] emotions)
        {

        }
    }
}
