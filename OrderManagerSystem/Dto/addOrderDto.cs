namespace OrderManagerSystem.Dto
{
    public class addOrderDto
    {
        //addOrderController接收js傳過來的資料
        //其實就是order(訂單明細)的所有欄位
        public string orderId { get; set; }
        public string applyDate { get; set; }
        public string deadline { get; set; }
        public string estimateStartDate { get; set; }
        public string estimateEndDate { get; set; }
        public int amount { get; set; }
        public string location { get; set; }
        public string mainReceiver { get; set; }
        public int estimateWorkingTime { get; set; }
    }
}
