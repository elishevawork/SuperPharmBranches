namespace SuperPharmWebAPI.Models
{
    public class BranchesReadDto
    {
        public List<Branch>? Branches { get; set; }
    }

    public class Branch
    {
        public string? BranchName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public bool IsOpen24Hours { get; set; }
        public double LatitudeY { get; set; }
        public double LongitudeX { get; set; }
        public double xITM { get; set; }
        public double yITM { get; set; }

    }
}
