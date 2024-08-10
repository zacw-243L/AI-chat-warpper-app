using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.Reflection;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;



namespace AITESTING
{
    [DataContract]
    public class Message
    {
        [DataMember(Name = "role")]
        public string Role { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }
    }

    [DataContract]
    public class RequestBody
    {
        [DataMember(Name = "messages")]
        public Message[] Messages { get; set; }

        [DataMember(Name = "system_prompt")]
        public string SystemPrompt { get; set; }

        [DataMember(Name = "temperature")]
        public double Temperature { get; set; }

        [DataMember(Name = "top_k")]
        public int TopK { get; set; }

        [DataMember(Name = "top_p")]
        public double TopP { get; set; }

        [DataMember(Name = "max_tokens")]
        public int MaxTokens { get; set; }

        [DataMember(Name = "web_access")]
        public bool WebAccess { get; set; }
    }
    public sealed partial class MainPage : Page
    {
        private const int MAX_CHARACTERS = 400;
        private ObservableCollection<Message> Messages = new ObservableCollection<Message>();

        public MainPage()
        {
            this.InitializeComponent();
            SetupWindow();
        }
        private void AddM(String sender, String message)
        {
            TextBlock MBlock = new TextBlock();
            TextBlock Timestamp = new TextBlock();
            if (sender == "You")
            {
                MBlock.HorizontalAlignment = HorizontalAlignment.Right;
                Timestamp.HorizontalAlignment = HorizontalAlignment.Right;
                MBlock.Padding = new Thickness(50,0, 0, 0);
            }
            else
            {
                MBlock.HorizontalAlignment = HorizontalAlignment.Left;
                Timestamp.HorizontalAlignment = HorizontalAlignment.Left;
                MBlock.Padding = new Thickness(0, 0, 50, 0);
            }
            MBlock.Text = $"{sender}: {message}";
            MBlock.Margin = new Thickness(5, 5, 5, 0);
            MBlock.TextWrapping = TextWrapping.Wrap;
            MBlock.MaxWidth = 300;
            MBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.WhiteSmoke);
            ChatSP.Children.Add(MBlock);

            Timestamp.Text = DateTime.Now.ToString("HH:mm:ss");
            Timestamp.FontSize = 10;
            Timestamp.Margin = new Thickness(5, 0, 5, 5);
            Timestamp.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
            ChatSP.Children.Add(Timestamp);

            ChatS.ChangeView(null, ChatS.ScrollableHeight,null);
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            String M = UserInputTextBox.Text;
            if (!string.IsNullOrEmpty(M)) 
            {
                AddM("You", M);
                UserInputTextBox.Text = "";
                UserInputTextBox.Visibility = Visibility.Collapsed;
                ThinkingTextBlock.Visibility = Visibility.Visible;

                var client = new HttpClient();
                var requestBody = new RequestBody
                {
                    Messages = new[]
                    {
                    new Message { Role = "user", Content = M }
                },
                    SystemPrompt = "",
                    Temperature = 0.9,
                    TopK = 5,
                    TopP = 0.9,
                    MaxTokens = 256,
                    WebAccess = false
                };

                var memoryStream = new MemoryStream();
                var serializer = new DataContractJsonSerializer(typeof(RequestBody));
                serializer.WriteObject(memoryStream, requestBody);
                memoryStream.Position = 0;
                var content = new StringContent(Encoding.UTF8.GetString(memoryStream.ToArray()), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://chatgpt-42.p.rapidapi.com/chatgpt"),
                    Headers =
                {
                    { "x-rapidapi-key", "" },
                    { "x-rapidapi-host", "chatgpt-42.p.rapidapi.com" }
                },
                    Content = content
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    // Parse JSON response to extract the result field
                    var jsonResponse = JObject.Parse(body);
                    var result = jsonResponse["result"]?.ToString();

                    Debug.WriteLine(result);

                    string CBR = result ?? "AI Buddy has no response.";
                    AddM("AI Buddy", CBR);
                }
                UserInputTextBox.Visibility = Visibility.Visible;
                ThinkingTextBlock.Visibility = Visibility.Collapsed;
            } 
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (AssemblyTitleAttribute)assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true)[0];
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(attribute.Title);
            Clipboard.SetContent(dataPackage);
        }
        private void SetupWindow()
        {
            // Set the preferred launch view size and windowing mode
            ApplicationView.PreferredLaunchViewSize = new Size(800, 480);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            // Set the title bar properties
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Prevent the application window from being resized
            var view = ApplicationView.GetForCurrentView();
            var size = new Size(800, 480);
            view.VisibleBoundsChanged += (s, e) =>
            {
                if (view.VisibleBounds.Width != size.Width || view.VisibleBounds.Height != size.Height)
                {
                    view.TryResizeView(size);
                }
            };
            view.TryResizeView(size);
        }
    }
}
