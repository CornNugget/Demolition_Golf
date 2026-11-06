// Custom analytics events. Each name and parameter must match its definition in the
// Unity dashboard's Event Manager exactly, including the parameter type.

public class LevelStartedEvent : Unity.Services.Analytics.Event
{
    public LevelStartedEvent() : base("levelStarted")
    {
    }

    public string LevelName { set { SetParameter("levelName", value); } }
}

public class LevelCompletedEvent : Unity.Services.Analytics.Event
{
    public LevelCompletedEvent() : base("levelCompleted")
    {
    }

    public string LevelName { set { SetParameter("levelName", value); } }
    public float ClearTimeSecondsF { set { SetParameter("clearTimeSecondsF", value); } }
    public int ShotsTaken { set { SetParameter("shotsTaken", value); } }
}

public class SwitchActivatedEvent : Unity.Services.Analytics.Event
{
    public SwitchActivatedEvent() : base("switchActivated")
    {
    }

    public string LevelName { set { SetParameter("levelName", value); } }
    public string SwitchName { set { SetParameter("switchName", value); } }
    public int SwitchOrder { set { SetParameter("switchOrder", value); } }
    public float TimeSeconds { set { SetParameter("timeSeconds", value); } }
}

public class MaterialUnlockedEvent : Unity.Services.Analytics.Event
{
    public MaterialUnlockedEvent() : base("materialUnlocked")
    {
    }

    public string LevelName { set { SetParameter("levelName", value); } }
    public string Material { set { SetParameter("material", value); } }
    public float TimeSeconds { set { SetParameter("timeSeconds", value); } }
}
