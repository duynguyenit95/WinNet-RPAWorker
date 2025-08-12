namespace SCCWorker.Model
{
    public class SCC_DOInfo
    {
        public string DOId { get; set; }
        public string DOCode { get; set; }
        public string DOApprovalStatus { get; set; }
        public string DOCloseStatus { get; set; }
        public string DOCooperateStatus { get; set; }
        public int TotalElements { get; set; } = -1;

    }
}
