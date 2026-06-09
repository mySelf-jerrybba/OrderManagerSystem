using OrderManagerSystem.Dao;
using OrderManagerSystem.Dto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderManagerSystem.Service
{
    public class chartService
    {
        //啟用selectDao的function
        private readonly selectDao dao;
        public chartService(selectDao selectDao)
        {
            dao = selectDao;
        }

        public async Task<List<ScheduleDto>> getScheduledInfo(string monthStartId, string monthEndId)
        {
            System.Console.WriteLine("開始執行getScheduledInfo");
            var perdayData = await dao.setChart(monthStartId, monthEndId);
            //perdayData會包含以下的資料：
            //例如：
            //日期 = 2026 / 6 / 1 上午 12:00:00
            //排成日期 = 2026 / 6 / 1 上午 12:00:00
            //時間桶容量 = 24
            //訂單編號 = 2026060001
            //預估工時 = 8
            //是否跨日加工 = false
            //時間桶餘額


            System.Console.WriteLine("getScheduledInfo執行結束");

            return perdayData;
        }

        
    }
}
    
