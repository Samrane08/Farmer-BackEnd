namespace Model;

public class WebServiceUserModel
{
    public int ID { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? EmailID { get; set; }
    public string? MobileNo { get; set; }
    public string? FullName { get; set; }
    public string? FullNameInMarathi { get; set; }
    public string? Gender { get; set; }
    public string? DateofBirth { get; set; }
    public string? UIDNO { get; set; }
    public string? PANNO { get; set; }
    public string? TrackID { get; set; }
    public string? UserID { get; set; }
    public long? NumericID { get; set; }
    public string? UserIdentity { get; set; }
    public string? Domain { get; set; }
    public bool? IsEmailVerified { get; set; }
    public bool? IsMobileVerified { get; set; }
}

public class HostelServiceUserModel
{
    public int? HostelID { get; set; }
    public string? Hostel { get; set; }
    public int? District { get; set; }
    public int? Taluka { get; set; }
    public string? Address { get; set; }
    public string? Mobile { get; set; }
    public string? Landline { get; set; }
    public string? Email { get; set; }
    public int? Capacity { get; set; }
    public int? BuildingCapacity { get; set; }
    public int? HostelType { get; set; }
    public bool IsFirstLogin { get; set; }
}
public class DepartmentProfileModel
{
    public int? DistrictId { get; set; }
    public string? UserIdentity { get; set; }
    public int? WorkFlowId { get; set; }
    public bool? IsFirstLogin { get; set; }
}
