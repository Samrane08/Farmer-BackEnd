using Microsoft.AspNetCore.Identity;
using Model; 
using Model.Admin;
using Repository.Entity;
using System.Net.Sockets;

namespace Service.Interface;

public interface IUserManagerService
{
    Task<UserReponseModel?> ApplicantAuthentication(WebServiceUserModel model);
    Task<List<ApplicationRole>> GetRoleList();
    Task<bool> CreateRoleAsync(string roleName,int deptId);
    Task<int> GetDeptIdByDeptName(string deptName);
    Task<bool> UpdateRoleAsync(ApplicationRole role);
    Task<List<EntityType>> GetEntityTypeList();
    Task<bool> CreateEntityAsync(string entityName, int Status);
    Task<bool> EntityRoleMapping(int EntityTypeId, string RoleId, int Status);
    Task<List<EntityRoleMapModel>> RoleMappingList();
    Task<List<int>> GetEntityRoleMappingId(string UserId,int? deptId);
    Task UserLoginSessionStore(string UserId, string SessionId);
    Task<long> CreateNumericId(string UserId);
    Task<int> GetDepartmentIdByRoleName(string roleName);
    Task<long> SaveloginDetails(logindetails model);
    Task<VerifiedStatusModel> Getlogindetails();
    Task<Applicantdetails> GetlogindetailsByUserId(long userId);

   // Task<logindetails> GetlogindetailsByAadharRefNo();
    Task<logindetails> GetUserNumericId(string useridentity);

    Task<logindetails> GetLogindetailByUserName(string username);

    Task<bool> UpdateAadharStatus(bool IsAadharVerified, long UserId);
    Task<List<SelectModel>> GetDeptList();

    Task<List<RolesSelectModel>> GetRolesBydept(int deptId);

    Task<int> GetDeptIdByRoleName(string roleName);

    DbtIntgrationModel CheckApplicantDataAvailbility(string academicYear);
    int? InsertApplicantPrefilledData(string aadharRefNo, string academicYear);

    Task<bool> ResetApplicantPassword(string userName,string Password);

    Task<bool> CreateScoolUsersAsync(SchoolLoginCreationDto model);

    Task<bool> CreateScrutinyLoginsAsync(ScrutinyLogins model);

    Task<bool> UpdateIsFirstLoginAsync();
    Task<bool> UpdateUserEmailAndMobileInfo(UpdateUserModel model);


}