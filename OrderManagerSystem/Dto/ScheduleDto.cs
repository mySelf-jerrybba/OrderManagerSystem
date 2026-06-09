namespace OrderManagerSystem.Dto
{
    public class ScheduleDto
    {
        //接收selectDao的setChart查詢的資料
        //timebucketscheduled(時間桶的排程資訊) right join timebucket(時間桶資訊)
        public string bucketDate { get; set; }//排成日期

        public string workStation { get; set; }//工站名稱

        public string orderId { get; set; }//訂單編號

        public string workingHours { get; set; }//預估工時

        public string isWorkingOverNight { get; set; }//是否跨日加工

        public int remainTime { get; set; }//時間桶餘額
    }
}
