using OrderManagerSystem.Controllers;
using OrderManagerSystem.Dao;
using OrderManagerSystem.Dto;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderManagerSystem.Service
{
    public class addOrderService
    {
        private readonly selectDao _selectDao;
        private readonly insertDao _insertDao;
        private readonly updateDao _updateDao;
        public addOrderService(selectDao selectDao, insertDao insertDao, updateDao updateDao)
        {
            _selectDao = selectDao;
            _insertDao=insertDao;
            _updateDao = updateDao;
        }

        //addOrderDto dto 表這是一路從js傳過來的資料
        public async Task<string> ckeckApply(addOrderDto dto) {//addOrderDto dto
            //運作邏輯：
            //加工不可中斷
            //人會自行判斷要使用多長時間區間
            //預估工時本身就包含預留時間不由系統判斷

            System.Console.WriteLine("開始執行ckeckApply");
            DateTime estimateStartDate = DateTime.Parse(dto.estimateStartDate);
            DateTime estimateEndDate = DateTime.Parse(dto.estimateEndDate);
            var estimateWorkingTime = dto.estimateWorkingTime;

            System.Console.WriteLine("estimateStartDate="+ estimateStartDate);
            System.Console.WriteLine("estimateEndDate=" + estimateEndDate);
            System.Console.WriteLine("estimateWorkingTime=" + estimateWorkingTime);

            //先調出整個區間的時間桶資訊
            var bucketInfo =await _selectDao.getTimeBucketTime(estimateStartDate, estimateEndDate);
            //把釣出來的資料(obect型態)，整理程可以用bucketTime查詢的格式
            var bucketMap = bucketInfo.ToDictionary(x => x.bucketDate.Date);

            

            //第一關：區間內時間桶總餘額是否足夠
            int totalTime = 0;
            foreach (var item in bucketInfo)
            {
                totalTime += item.remainTime;
            }
            System.Console.WriteLine("totalTime(區間內時間桶總餘額)=" + totalTime);

            if(totalTime< estimateWorkingTime) { //時間桶總餘額<預估工時
                System.Console.WriteLine("錯誤：區間內時間桶總餘額不足");
                System.Console.WriteLine("ckeckApply執行結束");
                return "false";//重新選填
            }

            //第二關分類是哪一種情況
            if (totalTime == estimateWorkingTime)//時間桶總餘額=預估工時
            {
                //普通
                if (estimateStartDate == estimateEndDate)
                {
                    //第一種：當日加工
                    //既然餘額充足，又不需要跨日加工，那自然是通過檢查
                    System.Console.WriteLine("成功：當日加工不跨日");
                    //step1.新增訂單資訊
                    System.Console.WriteLine("step1.新增訂單資訊");
                    await _insertDao.insertIntoOrder(dto);
                    //step2.新增排程資訊
                    System.Console.WriteLine("step2.新增排程資訊");
                    int workingHour1 = totalTime;//餘額夠用且單日加工，因此workingHour直接等於預估工時
                    await _insertDao.insertTimebucketscheduled(dto,workingHour1, estimateStartDate,"false");
                    //step3.更新時間桶資訊
                    System.Console.WriteLine("step3.更新時間桶資訊");
                    await _updateDao.updateTimebucketscheduled(dto, workingHour1, estimateStartDate,"false");

                    System.Console.WriteLine("ckeckApply執行結束");
                    return "true";
                }
                else if (estimateStartDate.AddDays(1) == estimateEndDate)
                {
                    DateTime currentDate1 = estimateStartDate;
                    //第二種：連續加工一天
                    //檢查有沒有其他訂單占用跨日加工名額
                    while (currentDate1 < estimateEndDate)
                    {
                        var beUsed = bucketMap[currentDate1.Date].whoIsWorkingOverNight;
                        if (!string.IsNullOrEmpty(beUsed))
                        {
                            System.Console.WriteLine("錯誤：" + currentDate1 + "已被" + beUsed + "占用跨日加工名額");
                            System.Console.WriteLine("ckeckApply執行結束");
                            return "錯誤：" + currentDate1 + "已被" + beUsed + "占用跨日加工名額";//重新選填
                        }
                        currentDate1 = currentDate1.AddDays(1);
                    }
                    System.Console.WriteLine("成功：跨一日加工");
                    //step1.新增訂單資訊
                    System.Console.WriteLine("step1.新增訂單資訊");
                    await _insertDao.insertIntoOrder(dto);

                    //step2.新增排程資訊
                    //因為時間桶總餘額=預估工時，所以每天時間桶的餘額等於我每日安排的加工時數
                    //but，究竟是4+4=8還是2+6=8這件事還是要用bucketMap(之前select的時間桶資料)做確認

                    //只需新增兩次，就頭尾兩天
                    System.Console.WriteLine("step2.新增排程資訊");
                    await _insertDao.insertTimebucketscheduled(dto, Convert.ToInt32(bucketMap[estimateStartDate].remainTime), estimateStartDate,"true");
                    await _insertDao.insertTimebucketscheduled(dto, Convert.ToInt32(bucketMap[estimateEndDate].remainTime), estimateEndDate,"fasle");

                    //step3.更新時間桶資訊
                    System.Console.WriteLine("step3.更新時間桶資訊");
                    await _updateDao.updateTimebucketscheduled(dto, Convert.ToInt32(bucketMap[estimateStartDate].remainTime), estimateStartDate,dto.orderId.ToString());
                    await _updateDao.updateTimebucketscheduled(dto, Convert.ToInt32(bucketMap[estimateEndDate].remainTime), estimateEndDate, "false");

                    System.Console.WriteLine("ckeckApply執行結束");
                    return "true";
                }
                else
                {
                    //第三種：連續加工多天
                    //此刻對系統來說：
                    //時間桶總餘額=預估工時
                    //預計使用區間>2天
                    //例如：預估工時=30h，預計安排20260608~20260610
                    //理想情況是：3+24+3，連續跨日加工20260608、20260609兩天
                    //但是，有可能情況是10+10+10，中間還被占用跨日加工的名額

                    //檢查除了最後一天，有沒有其他訂單占用跨日加工名額
                    Dictionary<DateTime, string> ErrorList = new Dictionary<DateTime, string>();
                    DateTime currentDate2 = estimateStartDate;
                    while (currentDate2 < estimateEndDate)
                    {
                        //whoIsWorkingOverNight若沒人使用，在mysql預設是null
                        var beUsed = bucketMap[currentDate2.Date].whoIsWorkingOverNight;
                        if (!string.IsNullOrEmpty(beUsed))
                        {
                            ErrorList[currentDate2.Date] = "被" + beUsed + "占用";
                        }
                        currentDate2 = currentDate2.AddDays(1);
                    }

                    //既然都沒有人占用跨日名額，那想必除了頭跟尾，中間的天數都是24小時
                    DateTime currentDate3 = estimateStartDate.AddDays(1);
                    while (currentDate3 < estimateEndDate)
                    {
                        var overworkingDays = bucketMap[currentDate3.Date].remainTime;
                        if (overworkingDays != 24)
                        {
                            ErrorList[currentDate3.Date] = "低於24小時";
                        }
                        currentDate3 = currentDate3.AddDays(1);
                    }
                    if (ErrorList.Count > 0)
                    {
                        //印錯誤訊息
                        string errMsg = "";
                        foreach (var item in ErrorList)
                        {
                            errMsg += $"錯誤：{item.Key}，{item.Value}。";
                            System.Console.WriteLine($"錯誤：{item.Key}，{item.Value}");
                        }
                        System.Console.WriteLine("ckeckApply執行結束");
                        return errMsg;//重新選填
                    }
                    System.Console.WriteLine("成功：連續跨日加工");
                    //step1.新增訂單資訊
                    System.Console.WriteLine("step1.新增訂單資訊");
                    await _insertDao.insertIntoOrder(dto);
                    
                    //因為時間桶餘額等於預估工時
                    //所以每日時間桶的餘額，就是該訂單的每日工時
                    DateTime currentDate3_1 = estimateStartDate;
                    int workingHour2 = 0;
                    //dao只執行一次insert和update
                    while (currentDate3_1 <= estimateEndDate)
                    {
                        workingHour2 = bucketMap[currentDate3_1].remainTime;
                        //step2.新增排程資訊
                        System.Console.WriteLine("step2.新增排程資訊，且同步step3.更新時間桶資訊");
                        if (currentDate3_1 == estimateEndDate)
                        {
                            //最後一天不需要跨日加工
                            await _insertDao.insertTimebucketscheduled(dto, workingHour2, currentDate3_1,"false");
                            //step3.更新時間桶資訊
                            await _updateDao.updateTimebucketscheduled(dto, workingHour2, currentDate3_1, "false");
                        }
                        else { 
                            await _insertDao.insertTimebucketscheduled(dto, workingHour2, currentDate3_1,"true");
                            //step3.更新時間桶資訊
                            await _updateDao.updateTimebucketscheduled(dto, workingHour2, currentDate3_1, dto.orderId.ToString());
                        }

                        currentDate3_1 = currentDate3_1.AddDays(1);
                    }

                    System.Console.WriteLine("ckeckApply執行結束");
                    return "true";
                }//end by else
            }//end by if (totalTime == estimateWorkingTime
            else {
                //totalTime != estimateWorkingTime

                if (totalTime >= estimateWorkingTime)//時間桶總餘額>=預估工時
                {
                    if (estimateStartDate == estimateEndDate)
                    {
                        //第四種：
                        //某天空檔(24h)排當日加工
                        //既然餘額充足，又不需要跨日加工，那自然是通過檢查
                        System.Console.WriteLine("成功：當日加工不跨日");

                        //step1.新增訂單資訊
                        System.Console.WriteLine("step1.新增訂單資訊");
                        await _insertDao.insertIntoOrder(dto);
                        //step2.新增排程資訊
                        //因為時間桶總餘額>=預估工時
                        //所以必須取用預估工時
                        System.Console.WriteLine("step2.新增排程資訊");
                        int workingHour4 = estimateWorkingTime;
                        await _insertDao.insertTimebucketscheduled(dto, workingHour4, estimateStartDate,"false");
                        //step3.更新時間桶資訊
                        System.Console.WriteLine("step3.更新時間桶資訊");
                        await _updateDao.updateTimebucketscheduled(dto, workingHour4, estimateStartDate,"false");

                        System.Console.WriteLine("ckeckApply執行結束");
                        return "true";
                    }
                    else
                    {
                        //第五種：區間過大
                        //假設目前時間桶：
                        //6 / 8，可用3h
                        //6 / 9，可用24h
                        //6 / 10，可用24h
                        //6 / 11，可用3h
                        //總餘額是54小時

                        //有一筆訂單需要加工30小時
                        //我預期user的選擇會是：
                        //6 / 8、6 / 9、6 / 10這3天
                        //6 / 8，3h
                        //6 / 9，24h
                        //6 / 10，3h
                        //然而user卻選擇6 / 8、6 / 9、6 / 10、6 / 11共4天，總餘額=54

                        //有一筆訂單需要加工24小時
                        //我預期user的選擇會是：
                        //6 / 8、6 / 9、6 / 10這2天，3+21
                        //或是挑選剛好某天餘額是24小時的日期
                        //然而user卻選擇6 / 8、6 / 9、6 /10共3天，總餘額=51

                        //有一筆訂單需要加工8小時
                        //我預期user的選擇會是：
                        //6 / 8、6 / 9、6 / 10這2天，3+8
                        //或找一天能容納8小時餘額的日期
                        //然而user卻選擇6 / 9、6 /10共2天，總餘額=48
                        //或6 / 10、6 / 10共2天，總餘額=27


                        System.Console.WriteLine("totalTime(區間內時間桶總餘額)=" + totalTime);
                        System.Console.WriteLine("estimateWorkingTime=" + estimateWorkingTime);

                        System.Console.WriteLine("錯誤：區間過大（雖仍可排程)");
                        return "錯誤：區間過大（雖仍可排程)";//重新選填
                    }
                }
            }
            return "異常：系統的判斷式沒能攔截";//略過條件判斷的異常
        }//end by ckeckApply

       
    }//end by public class addOrderService
}
