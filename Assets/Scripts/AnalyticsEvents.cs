public class LevelCompletedEvent : Unity.Services.Analytics.Event
{
    public LevelCompletedEvent() : base("levelCompleted")
    {
    }

    public string LevelName { set { SetParameter("levelName", value); } }
    public float ClearTimeSeconds { set { SetParameter("clearTimeSeconds", value); } }
}
