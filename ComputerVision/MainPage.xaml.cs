using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using FileAttributes = System.IO.FileAttributes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ComputerVision
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

        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            StorageFile file = await picker.PickSingleFileAsync();
            string path = file.Path;
            BitmapImage bitmapImage = new BitmapImage();
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            await bitmapImage.SetSourceAsync(stream);
            loaded_image.Source = bitmapImage;
            if (file != null)
            {
                FaceDetectionAsync(file);
            }
            
        }

        public async void FaceDetectionAsync(StorageFile file)
        {
            HttpResponseMessage response;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", "43418dba990e4d13a6513ae5851c7060");
            var requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                                    "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                                    "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            var uri = "https://cvisionapi.cognitiveservices.azure.com/face/v1.0/detect" + "?" + requestParameters;
            var byteData = await Task.Run(() => GetImageAsByteArray(file));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                var contentString = await response.Content.ReadAsStringAsync();
                jsonTxt.Text = JsonPrettyPrint(contentString);
            }
        }

        private static async Task<byte[]> GetImageAsByteArray(StorageFile file)
        {
            var fStream = await file.OpenAsync(FileAccessMode.Read);
            var binaryReader = new BinaryReader(fStream.AsStream());
            return binaryReader.ReadBytes((int)fStream.Size);
        }

        static string JsonPrettyPrint(string json)
        {
            string result=null;
            var face = JsonConvert.DeserializeObject<List<RootObject>>(json);
            foreach (var f in face)
            {
                result = string.Concat("Gender: " + f.faceAttributes.gender + "\n"
                                       +"Age: " + f.faceAttributes.age + "\n" + "Glasses: " 
                                       + f.faceAttributes.glasses + "\n" + "Bald: " + f.faceAttributes.hair.bald);
            }

            return result;
        }
    }
}
