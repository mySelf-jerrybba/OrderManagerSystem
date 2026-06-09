using Microsoft.AspNetCore.Mvc;
using OrderManagerSystem.Dto;
using OrderManagerSystem.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OrderManagerSystem.Controllers
{
    public class addOrderController : Controller
    {
        public IActionResult addOrder()
        {
            return View();
        }

        //Constructor Injection（建構子注入）
        //取用charService裡的function
        //類似：
        //@Autowired
        //private charService service;
        private readonly chartService _chartService;
        private readonly addOrderService _addOrderService;
        
        public addOrderController(chartService chartService,addOrderService addOrderService)
        {
            _chartService = chartService;
            _addOrderService = addOrderService;
        }
        

        //自動載入圖表
        [HttpGet("/getScheduled")]//@spring裡GetMapping的意思
        public async Task<IActionResult> getScheduled(string monthStartId, string monthEndId)
        {
            //IActionResult=這個方法最後會回傳一個 HTTP 回應
            System.Console.WriteLine("開始執行getScheduled");
            try
            {
                var result = await _chartService.getScheduledInfo(monthStartId, monthEndId);
                System.Console.WriteLine("getScheduled執行結束");
                return Json(result);
            }
            catch (Exception e) {
                System.Console.WriteLine("getScheduled執行結束");
                System.Console.WriteLine("發生錯誤");
                System.Console.WriteLine("Message: " + e.Message);
                System.Console.WriteLine("StackTrace: " + e.StackTrace);
                return StatusCode(500, new
                {
                    error = e.Message
                });
            }

        }//end by getScheduled

        [Route("applyAddOrder")]
        [HttpPost]
        public async Task<IActionResult> applyAddOrder([FromBody] addOrderDto dto) {
            System.Console.WriteLine("開始執行applyAddOrder");
            try
            {
                //接收addOrderDto
                var result = await _addOrderService.ckeckApply(dto);
                //result=true(可新增排程)/false(重新選填)
                //js在判斷怎麼處理，controller只回傳結果
                if (result == "true")
                {
                    System.Console.WriteLine("applyAddOrder執行結束");
                    return Json("true");//回傳失敗訊息
                }
                else {
                    System.Console.WriteLine("applyAddOrder執行結束");
                    return Json(result);//回傳失敗訊息
                }
                
            }
            catch (Exception e)
            {
                System.Console.WriteLine("applyAddOrder執行結束");
                System.Console.WriteLine("發生錯誤");
                System.Console.WriteLine("Message: " + e.Message);
                System.Console.WriteLine("StackTrace: " + e.StackTrace);
                return StatusCode(500, new
                {
                    error = e.Message
                });
            }
        } //end by applyAddOrder
    }
}
