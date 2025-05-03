namespace healthmate_backend.Models.Request;

public class SaveAvailableSlotsRequest
{
    public List<SlotData> Slots { get; set; }
}

public class SlotData
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
}
