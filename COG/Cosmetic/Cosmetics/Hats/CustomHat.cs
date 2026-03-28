namespace COG.Cosmetics.Hats;

public sealed class CustomHat
{
    public CustomHat(string id, HatData hatData, HatViewData hatViewData, PreviewViewData previewData)
    {
        Id = id;
        HatData     = hatData;
        HatViewData = hatViewData;
        PreviewData = previewData;
    }

    public string Id { get; }
    public HatData HatData { get; }
    public HatViewData HatViewData { get; }
    public PreviewViewData PreviewData { get; }
}
