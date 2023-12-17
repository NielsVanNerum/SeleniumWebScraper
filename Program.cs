using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V117.LayerTree;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

// Set up ChromeDriver that will navigate our webbrowser
IWebDriver driver = new ChromeDriver();

while (true) {
    // ask user what to do, based on that the specific webscraper will start
    Console.WriteLine("");
    Console.WriteLine("1. youtube\t2. job\t3. motorcycle");
    Console.Write("Select what to scrape: ");
    var userSelection = Console.ReadLine();
    // do different parts of the program based on selection
    // ======================================================================= YOUTUBE ================================================================================
    if (userSelection == "youtube") {
        // in data variable we store csv
        string[][] data = new string[][]{};
        // in jsonData variable we store json
        var jsonData = new
        {
            videos = Array.Empty<object>()
        };


        // Navigate to youtube
        driver.Navigate().GoToUrl("https://www.youtube.com/");

        //find cookies button and click it
        var cookies = driver.FindElement(By.XPath("//*[@id='content']/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]"));
        cookies.Click();

        // locate searchbar and fill in our topic
        IWebElement searchBar = driver.FindElement(By.Name("search_query"));
        Thread.Sleep(1000);
        searchBar.Clear();
        Console.WriteLine("");
        Console.Write("What topic?: ");
        searchBar.SendKeys(Console.ReadLine());
        searchBar.Submit();

        // locate button for search settings and click it
        Thread.Sleep(1000);
        IWebElement button = driver.FindElement(By.XPath("//*[@id='filter-button']/ytd-button-renderer/yt-button-shape/button"));
        button.Click();
        Thread.Sleep(1000);
        // locate button for filter by upload data and click it
        IWebElement filter = driver.FindElement(By.XPath("/html/body/ytd-app/ytd-popup-container/tp-yt-paper-dialog/ytd-search-filter-options-dialog-renderer/div[2]/ytd-search-filter-group-renderer[5]/ytd-search-filter-renderer[2]/a/div/yt-formatted-string"));
        filter.Click();
        Thread.Sleep(1000);

        // Get all videos from the page in a collection
        ReadOnlyCollection<IWebElement> videos = driver.FindElements(By.CssSelector("#contents > ytd-video-renderer"));

        Console.WriteLine("");

        // go over first 5 with index
        var i = 0;
        while (i < 5) {
            // get information about video for each element
            var title = videos[i].FindElement(By.CssSelector("h3"));
            var videoViewCount = videos[i].FindElement(By.CssSelector("#metadata-line > span:nth-child(3)"));
            var uploader = videos[i].FindElement(By.XPath("div[1]/div/div[2]/ytd-channel-name/div/div/yt-formatted-string/a"));
            var videoLink = videos[i].FindElement(By.CssSelector("#video-title"));

            // print the data we located add .Text because we can't print IWebElements
            Console.WriteLine(title.Text);
            Console.WriteLine(videoViewCount.Text);
            Console.WriteLine(uploader.Text);
            // get href attribute from the element we located
            Console.WriteLine(videoLink.GetAttribute("href"));
            Console.WriteLine("");

            i++;

            // code to add to csv
            string[] newData = { title.Text, videoViewCount.Text, uploader.Text, videoLink.GetAttribute("href") };
            // create new array that is 1 longer then existing data array
            string[][] newDataArray = new string[data.Length + 1][];
            // paste data in newDataArray
            Array.Copy(data, newDataArray, data.Length);
            // paste the new data in the last row of our new array
            newDataArray[data.Length] = newData;
            // update the recently expanded array to our data array
            data = newDataArray;

            // make new json structure with the data
            var newDataJson = new
            {
                Title = title.Text,
                videoViewCount = videoViewCount.Text,
                Uploader = uploader.Text,
                videoLink = videoLink.GetAttribute("href")
            };

            // concatenate the new json structure with json that holds everything
            jsonData = new
            {
                videos = ((object[])jsonData.videos).Concat(new[] { newDataJson }).ToArray()
            };
        }

        // =============================== CSV FILE WRITING ========================================
        // specify the name for the csv
        string filePath = "output-youtube.csv";

        // write data to the CSV file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // iterate through the rows of data
            foreach (string[] row in data)
            {
                // join the array elements into the file seperated by ,
                string csvLine = string.Join(",", row);
                // Write the line to the file
                writer.WriteLine(csvLine);
            }
        }
        Console.WriteLine($"CSV file created: {filePath}");

        // =============================== JSON FILE WRITING =======================================
        // Specify the file path for json
        string filePathJson = "output-youtube.json";
        // Create a JSON-formatted string with indentation true
        string jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        // Write the string to the file
        File.WriteAllText(filePathJson, jsonString);
        Console.WriteLine($"JSON file created: {filePathJson}");
    }
    // ======================================================================= JOB ================================================================================
    else if (userSelection == "job")
    {
        // once again initiate variable in which we will store csv data
        string[][] data = new string[][]{};
        // and json data
        var jsonData = new
        {
            jobs = Array.Empty<object>()
        };

        // go to jobsite
        driver.Navigate().GoToUrl("https://www.ictjob.be/nl/");

        // locate searchbar and fill in what user wants
        IWebElement searchBar = driver.FindElement(By.CssSelector("#keywords-input"));
        Thread.Sleep(1000);
        searchBar.Clear();
        Console.Write("what search term?: ");
        searchBar.SendKeys(Console.ReadLine());

        // click the search button
        Thread.Sleep(1000);
        IWebElement button = driver.FindElement(By.CssSelector("#main-search-button"));
        button.Click();
        
        // locate and click sort
        Thread.Sleep(3000);
        IWebElement sort = driver.FindElement(By.CssSelector("#sort-by-date"));
        sort.Click();
        
        // get all job offers in collection
        Thread.Sleep(3000);
        ReadOnlyCollection<IWebElement> jobs = driver.FindElements(By.CssSelector("#search-result-body > div > ul > li > span.job-info"));

        // go over first 5 from collection and get data
        var i = 0;
        while (i < 5)
        {
            // scrape the data
            Thread.Sleep(1000);
            var title = jobs[i].FindElement(By.CssSelector("a.job-title"));
            var company = jobs[i].FindElement(By.CssSelector("span.job-company"));
            var location = jobs[i].FindElement(By.CssSelector("span.job-location"));
            var link = jobs[i].FindElement(By.CssSelector("a.job-title"));

            // print the data
            Console.WriteLine(title.Text);
            Console.WriteLine(company.Text);
            Console.WriteLine(location.Text);
            // try to scrape the keywords, if there are none, we get error
            // then we print "no keywords"
            try
            {
                // get keywords
                var keywords = jobs[i].FindElement(By.CssSelector("span.job-keywords"));

                // add data to csv
                // same concept as described earlier
                // create array 1 bigger copy data from previous array to the bigger one
                // add new data to last row
                string[] newData = { title.Text, company.Text, location.Text, keywords.Text, link.GetAttribute("href") };
                string[][] newDataArray = new string[data.Length + 1][];
                Array.Copy(data, newDataArray, data.Length);
                newDataArray[data.Length] = newData;
                data = newDataArray;

                //print keywords
                Console.WriteLine(keywords.Text);

                // add to json
                var newDataJson = new
                {
                    Title = title.Text,
                    company = company.Text,
                    location = location.Text,
                    keywords = keywords.Text,
                    link = link.GetAttribute("href")
                };

                // add new json to old json
                jsonData = new
                {
                    jobs = ((object[])jsonData.jobs).Concat(new[] { newDataJson }).ToArray()
                };
            }
            // if there are no keywords and we got an error on locating the keywords we run this code everything the same
            // just we fill in "no keywords" in the csv and json and also print "no keywords"
            catch (NoSuchElementException)
            {
                Console.WriteLine("no keywords");

                // add data to csv with no keywords
                string[] newData = { title.Text, company.Text, location.Text, "no keywords", link.GetAttribute("href") };
                string[][] newDataArray = new string[data.Length + 1][];
                Array.Copy(data, newDataArray, data.Length);
                newDataArray[data.Length] = newData;
                data = newDataArray;

                // add data to json
                var newDataJson = new
                {
                    Title = title.Text,
                    company = company.Text,
                    location = location.Text,
                    keywords = "no keywords",
                    link = link.GetAttribute("href")
                };

                // add new json to old json
                jsonData = new
                {
                    jobs = ((object[])jsonData.jobs).Concat(new[] { newDataJson }).ToArray()
                };
            }
            // for link we get the href
            Console.WriteLine(link.GetAttribute("href"));

            Console.WriteLine("");
            i++;
        }

        // =============================== CSV FILE WRITING ========================================
        // specify path
        string filePath = "output-job.csv";
        // write data to the CSV file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // iterate through the rows of data
            foreach (string[] row in data)
            {
                // Join the array elements into a CSV-formatted line
                string csvLine = string.Join(",", row);
                // write the line to the file
                writer.WriteLine(csvLine);
            }
        }
        Console.WriteLine($"CSV file created: {filePath}");

        // =============================== JSON FILE WRITING =======================================
        // specify the file path
        string filePathJson = "output-job.json";
        // create a JSON-formatted string
        string jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        // write the string to the file
        File.WriteAllText(filePathJson, jsonString);
        Console.WriteLine($"JSON file created: {filePathJson}");
    }
    // ======================================================================= MOTORCYCLE ================================================================================
    else if (userSelection == "motorcycle")
    {
        // variable to keep itterating through the code as long as person wants to add extra motorcycles to compare
        var compareMotorcycle = "yes";
        // create a list of strings in which we will contain the specs of our selected motorcycles
        List<string> motorCycleCollection = new List<string>();

        // variables to store json and csv in
        string[][] data = new string[][] { };
        var jsonData = new
        {
            motorcycles = Array.Empty<object>()
        };

        // as long as we want to compare keep comparing
        while (compareMotorcycle == "yes") { 

            // navigate to the website
            driver.Navigate().GoToUrl("https://www.moto-data.net/");

            // create colletion of web elements for the brands on the homepage
            ReadOnlyCollection<IWebElement> brandNames = driver.FindElements(By.CssSelector("ul.grid li a"));
            Console.WriteLine("");

            // go over the brands and print them all with according index for selection
            var index = 0;
            foreach (var brandName in brandNames)
            {
                Console.WriteLine(index + ". " + brandName.Text);
                index++;
            }

            // ask the user to select a brand based on index
            Console.Write("\nChoose a motorcycle brand: ");
            int brandSelectionNumber = Convert.ToInt32(Console.ReadLine());
            // go to the page of the brand based on the href of the name
            driver.Navigate().GoToUrl(brandNames[brandSelectionNumber].GetAttribute("href"));
            Console.WriteLine("");

            // create collection of web elements for the years
            ReadOnlyCollection<IWebElement> years = driver.FindElements(By.CssSelector("span.color-bar"));

            // got over years but only print first 4 charachters (YYYY)
            foreach (var year in years)
            {
                Console.WriteLine(year.Text.Substring(0, 4));
            }

            // ask the user to fill in a year
            Console.Write("\nChoose a year: ");
            var yearSelectionNumber = Console.ReadLine();

            // go over the years again and check for a match with the first charachters
            foreach (var year in years)
            {
                // if there is a match
                if (yearSelectionNumber == year.Text.Substring(0, 4))
                {
                    // click the year
                    year.Click();

                    // make collection of motorcycles from that year
                    ReadOnlyCollection<IWebElement> motorcyclesByYear = driver.FindElements(By.CssSelector("section.wrapper li"));

                    // go over the motorcycles listing them with index
                    index = 0;
                    Console.WriteLine("");
                    foreach (var motorcycle in motorcyclesByYear)
                    {
                        Console.WriteLine(index + ". " + motorcycle.Text);
                        index++;
                    }

                    // allow the user to select by index
                    Console.Write("\nChoose a motorcycle: ");
                    int motorcycleSelectionNumber = Convert.ToInt32(Console.ReadLine());
                    motorcyclesByYear[motorcycleSelectionNumber].Click();
                    Console.WriteLine("");

                    // stop itterating over the years because we are no longer on that page
                    break;
                }
            }

            // make collection of specs
            ReadOnlyCollection<IWebElement> specs = driver.FindElements(By.CssSelector("table.data_details tr"));

            // make a string with specs and itterate over the specs and add them to string
            var placeholderString = "";
            foreach (var spec in specs)
            {
                placeholderString += spec.Text + "\n";
            }
            // add the motorcycle to the collection so we can print it once user has selected all motorcycles
            motorCycleCollection.Add(placeholderString);

            // split the string into lines on \n with removing empty entries 
            string inputData = placeholderString;
            string[] lines = inputData.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            // create a dictionary to store key-value pairs
            Dictionary<string, string> dataMotorcycles = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                // find the position of the colon
                int colonIndex = line.IndexOf(':');

                // ensure there is a colon and it's not at the beginning or end of the line
                if (colonIndex != -1 && colonIndex > 0 && colonIndex < line.Length - 1)
                {
                    // Extract key and value (trimming leading/trailing whitespaces)
                    string key = line.Substring(0, colonIndex).Trim();
                    string value = line.Substring(colonIndex + 1).Trim();

                    // add key-value pair to the dictionary
                    dataMotorcycles[key] = value;
                }
            }

            // add the dictoniary to motorcyclesList
            List<Dictionary<string, string>> motorcyclesList = new List<Dictionary<string, string>> { dataMotorcycles };

            // save this dictoniary in json format
            var jsonDataMoto = new
            {
                motorcycles = motorcyclesList
            };

            // empty the string for next time use
            placeholderString = "";

            // concatinate all the specs without \n
            foreach (var spec in specs)
            {
                placeholderString += spec.Text;
            }

            // add the concatinated string to csv
            string[] newData = { placeholderString };
            string[][] newDataArray = new string[data.Length + 1][];
            Array.Copy(data, newDataArray, data.Length);
            newDataArray[data.Length] = newData;
            data = newDataArray;

            // empty string for later use
            placeholderString = "";

            // ask if person wants to continue adding motorcycles to comparison
            Console.Write("Do you want to add another motorcycle to the comparison? (yes/no): ");
            compareMotorcycle = Console.ReadLine();
        }

        // if the person ended the comparison
        Console.WriteLine("");
        Console.WriteLine("----- COMPARISON -----");
        Console.WriteLine("");

        // go over all motorcycles in the comparison and print the specs
        foreach (var motorcycle in motorCycleCollection)
        {
            Console.WriteLine(motorcycle);
        }

        // =============================== CSV FILE WRITING ========================================
        // specify the file path
        string filePath = "output-motorcycle.csv";

        // write data to the CSV file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // iterate through the rows of data
            foreach (string[] row in data)
            {
                // join the array elements into a CSV-formatted line
                string csvLine = string.Join(",", row);
                // write the line to the file
                writer.WriteLine(csvLine);
            }
        }

        // =============================== JSON FILE WRITING =======================================
        Console.WriteLine($"CSV file created: {filePath}");
        // specify the file path
        string filePathJson = "output-motorcycle.json";
        // create a JSON-formatted string
        string jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        // write the string to the file
        File.WriteAllText(filePathJson, jsonString);
        Console.WriteLine($"JSON file created: {filePathJson}");
    }
}
// Close the browser
driver.Quit();
