namespace Repository.Interface;

public interface ICurrentUserService
{
    string UserId { get; }
    string UserNumericId { get; }
    string SessionId { get; }

     string deptId { get; }
}
