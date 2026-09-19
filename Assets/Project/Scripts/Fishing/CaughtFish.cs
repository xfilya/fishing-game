public sealed class CaughtFish
{
    public FishDefinition Definition { get; }
    public float Weight { get; }
    public bool IsNewSpecies { get; private set; }

    public CaughtFish(FishDefinition definition, float weight)
    {
        Definition = definition;
        Weight = weight;
    }

    public void MarkAsNewSpecies()
    {
        IsNewSpecies = true;
    }
}
