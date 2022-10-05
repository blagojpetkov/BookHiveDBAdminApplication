
using BookHiveDBAdminApplication.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BookHiveDBAdminApplication.Controllers
{
    public class UserController : Controller
    {

        //[HttpGet("[action]")]
        public IActionResult ImportUsers()
        {
            return View();
        }

        //import users from excel file using ExcelDataReader
        [HttpPost("[action]")]
        public IActionResult ImportUsers(IFormFile file)
        {

            //make a copy to upload the file in the desired directory folder, dva pati \ za escape
            string pathToUpload = $"{Directory.GetCurrentDirectory()}\\files\\{file.FileName}";

            //zapis na sodrzina od file vo taa pateka i flush posle
            using (FileStream fileStream = System.IO.File.Create(pathToUpload))
            {
                file.CopyTo(fileStream);

                fileStream.Flush();
            }

            //read data from uploaded file

            List<User> users = getUsersFromExcelFile(file.FileName);

            HttpClient client = new HttpClient();

            string URL = "https://localhost:44325/api/Admin/ImportAllUsers";


            HttpContent content = new StringContent(JsonConvert.SerializeObject(users), Encoding.UTF8, "application/json");


            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var result = response.Content.ReadAsAsync<bool>().Result;

            return Redirect("https://localhost:44325");
        }


        //od excel da se zeme lista na users
        private List<User> getUsersFromExcelFile(string fileName)
        {
            string pathToFile = $"{Directory.GetCurrentDirectory()}\\files\\{fileName}";

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            List<User> userList = new List<User>();

            using (var stream = System.IO.File.Open(pathToFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        userList.Add(new Models.User
                        {
                            Name = reader.GetValue(0).ToString(),
                            LastName = reader.GetValue(1).ToString(),
                            Address = reader.GetValue(2).ToString(),
                            Email = reader.GetValue(3).ToString(),
                            Password = reader.GetValue(4).ToString(),
                            ConfirmPassword = reader.GetValue(5).ToString(),
                        });
                    }
                }
            }
            return userList;

        }
    }
}