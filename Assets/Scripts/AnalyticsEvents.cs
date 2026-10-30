public class LevelCompletedEvent : Unity.Services.Analytics.Event
{
    public LevelCompletedEvent() : base("levelCompleted")
    {
    }

    public string LevelName { set { SetParameter("levelName", value); } }
    public float ClearTimeSecondsF { set { SetParameter("clearTimeSeconds", value); } }
}
