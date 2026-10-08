namespace PimpMyBakkie.Identification
{
    public readonly struct VehicleCandidate
    {
        public readonly string Make, Model, Variant;
        public readonly int Year;
        public readonly float Confidence;
        public VehicleCandidate(string make,string model,int year,string variant,float confidence)
        { Make=make; Model=model; Year=year; Variant=variant; Confidence=confidence; }
        public override string ToString() => $"{Year} {Make} {Model} {Variant}";
    }
}