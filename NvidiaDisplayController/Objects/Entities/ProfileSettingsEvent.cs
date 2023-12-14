namespace NvidiaDisplayController.Objects.Entities;

public class ProfileSettingsEvent
{
    public ProfileSettingsEvent(bool isDirty)
    {
        IsDirty = isDirty;
    }

    public bool IsDirty { get; set; }
}