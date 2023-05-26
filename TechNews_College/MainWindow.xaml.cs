using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using System.Runtime.CompilerServices;
using System.IO;
using System.Security.Policy;
using System.Diagnostics;

namespace TechNews_College
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static readonly HttpClient client = new HttpClient();
        private static async Task<Newtonsoft.Json.Linq.JObject> APICall(string APILink, string APIKey, string Data)
        {

            //In the next using statement you will initiate the Get Request, use the await keyword so it will execute the using statement in order.
            var client = new HttpClient();
            var useragent = "my-user-agent"; // THIS WAS ASKED BY THE API
            client.DefaultRequestHeaders.Add("User-Agent", useragent);
            var response = await client.GetAsync(APILink + "?" + Data + "&apiKey=" + APIKey);
            var responseContent = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(responseContent);
            //Console.WriteLine(json.ToString());
            return json;
        }
        static string PostRequest(string url, StringContent json)
        {
            using (HttpClient client = new HttpClient())
            {
                var result = client.PostAsync(url + ":8080", json).Result;

                return result.Content.ReadAsStringAsync().Result;
            }
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // READING THE FILE TO GET THE IP FROM
            string fileName = "IP.txt";
            string defaultContent = "localhost";
            string fileContents = "";

            if (File.Exists(fileName))
            {
                fileContents = File.ReadAllText(fileName);
            }
            else
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.Write(defaultContent);
                }
                fileContents = defaultContent;
            }

            Console.WriteLine("File Contents: " + fileContents);
            bool isAuthenticated = false;
            if (fileContents.Contains("localhost"))
            {
                isAuthenticated = true;
            }
            else
            {
                // TODO: Authenticate the user
                JObject values = new JObject(
                    new JProperty("user", UsernameTextBox.Text),
                    new JProperty("pass", PasswordBox.Password.ToString())
                );
                string json = JsonConvert.SerializeObject(values);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                string Response = PostRequest(fileContents, content);
                try
                {
                    isAuthenticated = bool.Parse(Response);
                }
                catch (Exception)
                {
                    MessageBox.Show(Response);
                }

            }

            if (isAuthenticated)
            {
                // Show the news page and hide the login page
                MainGrid.Children.Remove(MainGrid.Children[0]);
                NewsGrid.Visibility = Visibility.Visible;
                LoadNews();
            }
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink)sender;
            string navigateUri = hyperlink.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }


        private void NewsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewsListBox.SelectedItem != null)
            {
                var selectedNews = (JToken)((ListBoxItem)NewsListBox.SelectedItem).Tag;
                var url = selectedNews["url"].ToString();

                // Create a new Hyperlink object with the URL as the NavigateUri
                var link = new Hyperlink();
                link.NavigateUri = new Uri(url);
                link.Inlines.Add(url);

                // Add the RequestNavigate event handler to the Hyperlink object
                link.RequestNavigate += Hyperlink_Click;

                // Set the Hyperlink to the TextBlock
                NewsContentTextBlock.Inlines.Clear();
                NewsContentTextBlock.Inlines.Add(new Run("Click "));
                NewsContentTextBlock.Inlines.Add(link);
            }
        }



        private void AddToNewsListBox(JToken newsItem)
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = newsItem["title"].ToString();
            listBoxItem.Tag = newsItem;
            NewsListBox.Items.Add(listBoxItem);
        }
        private async void LoadNews()
        {
            JObject response = await APICall("https://newsapi.org/v2/everything", "c8d5702f44ab45b7bb311d22fced51f8", "q=tech&pageSize=10&page=1");

            List<Dictionary<string, string>> newsList = new List<Dictionary<string, string>>();

            foreach (var item in response["articles"])
            {
                // Build the dictionary with news item data
                Dictionary<string, string> newsDict = new Dictionary<string, string>();

                if (item["title"] != null)
                    newsDict.Add("title", item["title"].ToString());

                if (item["url"] != null)
                    newsDict.Add("url", item["url"].ToString());

                // Add the news item to the list box
                AddToNewsListBox(item);
            }

           
        }
        private async void Search_click(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "" || SearchTextBox.Text == null)
            {
                MessageBox.Show("Please ensure you have entered something into the search box!");
            }
            else
            {
                NewsListBox.Items.Clear();
                JObject response = await APICall("https://newsapi.org/v2/everything", "c8d5702f44ab45b7bb311d22fced51f8", "q=" + SearchTextBox.Text + "&pageSize=30&page=1");

                List<Dictionary<string, string>> newsList = new List<Dictionary<string, string>>();

                if (response["articles"] != null && response["articles"].HasValues)
                {
                    foreach (var item in response["articles"])
                    {
                        // Build the dictionary with news item data
                        Dictionary<string, string> newsDict = new Dictionary<string, string>();

                        if (item["title"] != null)
                            newsDict.Add("title", item["title"].ToString());

                        if (item["url"] != null)
                            newsDict.Add("url", item["url"].ToString());

                        // Add the news item to the list box
                        AddToNewsListBox(item);
                    }
                }
                else
                {
                    MessageBox.Show("No news have been found!");
                }
            }

        }



        /*
        static async Task Main(string[] args)
        {
            Console.WriteLine("Choose options");
            Console.Write(@"
                    1 - Top 10 News
                    2 - iPhone News
                ");

            string U_input = Console.ReadLine();


            Newtonsoft.Json.Linq.JObject t;
            switch (U_input)
            {
                case "1":
                    t = await APICall("https://newsapi.org/v2/everything", "c8d5702f44ab45b7bb311d22fced51f8", "q=tech&pageSize=10&page=1");
                    foreach (var item in t["articles"])
                    {
                        Console.WriteLine(item["title"] + " -- by " + item["author"]);
                        Console.WriteLine("Follow here in this link - " + item["url"]);

                        Console.WriteLine("\n");

                    }
                    break;
                case "2":
                    t = await APICall("https://newsapi.org/v2/everything", "c8d5702f44ab45b7bb311d22fced51f8", "q=iphone&pageSize=10&page=1");
                    foreach (var item in t["articles"])
                    {
                        Console.WriteLine(item["title"] + " -- by " + item["author"]);
                        Console.WriteLine("Follow here in this link - " + item["url"]);

                        Console.WriteLine("\n");

                    }
                    break;

            }


        }
        */
    }
}
