namespace MyIndustry.ApplicationService.Dto;

public class AdminStatsDto
{
    // User Stats
    public int TotalUsers { get; set; }
    public int TotalBuyers { get; set; }
    public int TotalSellers { get; set; }
    public int NewUsersThisMonth { get; set; }
    
    // Listing Stats
    public int TotalListings { get; set; }
    public int PendingListings { get; set; }
    public int ApprovedListings { get; set; }
    public int RejectedListings { get; set; }
    public int NewListingsThisMonth { get; set; }
    
    // Message Stats
    public int TotalMessages { get; set; }
    public int MessagesThisMonth { get; set; }
    
    // Category Stats
    public int TotalCategories { get; set; }
    
    // Recent Activity
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    
    // Charts Data
    public List<ChartDataPoint> UserRegistrationChart { get; set; } = new();
    public List<ChartDataPoint> ListingChart { get; set; } = new();
}

public class RecentActivityDto
{
    public string Type { get; set; } // "user_registered", "listing_created", "listing_approved", etc.
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public Guid? EntityId { get; set; }
}

public class ChartDataPoint
{
    public string Label { get; set; }
    public int Value { get; set; }
}
