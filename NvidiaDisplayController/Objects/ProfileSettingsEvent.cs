namespace NvidiaDisplayController.Objects;

public class ProfileSettingsEvent
{
    public ProfileSettingsEvent(bool isDirty)
    {
        IsDirty = isDirty;
    }

    public bool IsDirty { get; set; }
}