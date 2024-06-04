using Newtonsoft.Json;
using System.Drawing;
using System.Net.Http;
using System.Text;

internal class kvPair
{
    public string name { get; set; }
    public object value { get; set; }
}

internal class Program
{
    // This C# console program will submit a report with one image attached to it, for user #482 in MyCoast
    // This is not really intended to be a public API and so there isn't any type checking - please
    // make sure dates, times etc are valid in the formats below.

    private static readonly string myCoastUserId = "123"; // MyCoast userId
    private static readonly string myCoastuserHash = "123456789abcdef"; // MyCoast userId password hash

    private static async Task Main(string[] args)
    {
        using (HttpClient client = new HttpClient())
        {
            string imageId = Guid.NewGuid().ToString();
            using (MultipartFormDataContent formData = new MultipartFormDataContent())
            {
                using (FileStream stream = new FileStream(@"C:\Photo_Hudson.jpg", FileMode.Open, FileAccess.Read))
                {
                    formData.Add(new StreamContent(stream), "upfile", "upfile");
                    HttpResponseMessage fResponse = await client.PostAsync($"https://mycoast.org/blueurchin-js/appthings.php?apiv=5&user={myCoastUserId}&hash={myCoastuserHash}/&upload=image&id={imageId}&purpose=0", formData);
                    string fResponseString = await fResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(fResponseString);
                }
            }

            kvPair[] myReport = new kvPair[] {
                new kvPair { name = "_gform-form-id", value = 72 }, // the internal form ID for NJ High Water
                new kvPair { name = "from_device", value = "Chris' bulk uploader" }, // please set this to be whatever you're calling your uploader (it's just for logging)
                new kvPair { name = "photo_date", value = "2023-10-03" }, // date format is important. All dates and times are local for the report
                new kvPair { name = "photo_time", value = "10:26 am" }, // time format is important (and yes, I know it's a terrible one)
                new kvPair { name = "image1", value = new string[] { imageId } }, // array of IDs of uploaded images above
                new kvPair { name = "guessSource", value = new string[] { "Ocean", "Bay" } }, // These can be: "Ocean", "Bay", "River/Stream"
                new kvPair { name = "guessCause", value = new string[] { "High Tide", "Storm Surge" } }, // These can be: "High Tide", "Storm Surge", "Rain"
                new kvPair { name = "post_comment", value = "My post comment" },
                new kvPair { name = "location_longitude", value = -122.3181386 },
                new kvPair { name = "location_latitude", value = 47.6226932 },
            };

            string content = JsonConvert.SerializeObject(myReport);
            StringContent sContent = new StringContent(content, Encoding.UTF8, "application/json");
            string reportId = Guid.NewGuid().ToString();
            HttpResponseMessage rResponse = await client.PostAsync($"https://mycoast.org/blueurchin-js/appthings.php?apiv=5&user={myCoastUserId}&hash={myCoastuserHash}/&upload=report&id={reportId}", sContent);
            string rResponseString = await rResponse.Content.ReadAsStringAsync();
            Console.WriteLine(rResponseString);
        }
    }
}