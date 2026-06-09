namespace OrderManagerSystem.Dto
{
    public class timebuckDto
    {
        //查詢timebucket的資訊
        public DateTime bucketDate { get; set; }//時間桶日期
        public int usedTime { get; set; }//已被使用的容量
        public int remainTime { get; set; }//時間桶餘額
        public string? whoIsWorkingOverNight { get; set; }//哪個訂單會在今天跨日加工到隔日，string?表示可以是null或字串

    }
}
