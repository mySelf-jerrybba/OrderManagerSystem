using Microsoft.AspNetCore.Mvc;
using OrderManagerSystem.Models;
using System.Diagnostics;

namespace OrderManagerSystem.Controllers
{
    public class HomeController : Controller//XXX Controller是特殊命名規則
    {
        //Progam.cs在controller=Home的路由對應
        public IActionResult Index()//cshtml的檔案名稱要一樣才會自動對應
        {
            //預設的規則路徑，不想擺Views這個目錄也可以額外設定
            //會去Views的資料尋找有無Home資料夾
            //並根據Index再去尋找有無叫做Index的cshtml檔案
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
