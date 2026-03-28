namespace COG.Cosmetics.Nameplates;

public sealed class CustomNamePlate
{
    public CustomNamePlate(string id, NamePlateData namePlateData, NamePlateViewData viewData, PreviewViewData previewData)
    {
        Id               = id;
        NamePlateData    = namePlateData;
        NamePlateViewData = viewData;
        PreviewData      = previewData;
    }

    public string            Id                { get; }
    public NamePlateData     NamePlateData     { get; }
    public NamePlateViewData NamePlateViewData  { get; }
    public PreviewViewData   PreviewData        { get; }
}
