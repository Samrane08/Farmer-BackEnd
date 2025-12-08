using Dapper;
using Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model;
using Model.Admin;
using Repository.Data;
using Repository.Entity;
using Repository.Implementation;
using Repository.Interface;
using Service.Interface;
using System.Data;

namespace Service.Implementation
{
    public class UserManagerService : IUserManagerService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> roleManager;

        private readonly ApplicationDbContext context;
        private readonly ILogger<UserManagerService> logger;
        private readonly ICurrentUserService currentUserService;
        private readonly IDapper dapper;

        public UserManagerService(UserManager<ApplicationUser> userManager,
                                  RoleManager<ApplicationRole> roleManager,
                                  ApplicationDbContext context,
                                  ILogger<UserManagerService> logger, ICurrentUserService currentUserService, IDapper dapper)
        {
            _userManager = userManager;
            this.roleManager = roleManager;
            this.context = context;
            this.logger = logger;
            this.currentUserService = currentUserService;
            this.dapper = dapper;

        }
        public async Task<UserReponseModel?> ApplicantAuthentication(WebServiceUserModel model)
        {
            try
            {
                var responseModel = new UserReponseModel();

                if (!_userManager.Users.Any(u => u.UserName != model.UserName))
                {

                    var applicant = new ApplicationUser
                    {
                        Name = model.FullName,
                        UserName = model.UserName,
                        Email = !string.IsNullOrWhiteSpace(model.EmailID) ? model.EmailID : model.UserName + "@gmail.com",
                        Password = model.Password,
                        UserId = model.UserID,
                        PhoneNumber = model.MobileNo,
                        Status = Repository.Enums.Status.Active
                    };

                    try
                    {
                        var user = await _userManager.CreateAsync(applicant, "Pw1@" + model.Password);
                        if (user.Succeeded)
                        {
                            var applicantRole = roleManager.FindByNameAsync("Applicant").Result;

                            await _userManager.AddToRolesAsync(applicant, new[] { applicantRole.Name });
                            var userNumericIdentity = new UserNumericIdentity { UserId = applicant.Id };
                            await context.AspNetUserNumericIdentity.AddAsync(userNumericIdentity);
                            await context.SaveChangesAsync();
                            responseModel.Id = applicant.Id;
                            responseModel.NumericId = userNumericIdentity.Id;
                            responseModel.UserName = applicant.UserName;
                            responseModel.Name = applicant.Name;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
                    return responseModel;
                }
                else
                {
                    var user = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == model.UserName);
                    if (user != null)
                    {
                        if (!string.IsNullOrWhiteSpace(model.Password) && (user.Password != model.Password))
                        {
                            user.Password = model.Password;
                            await _userManager.UpdateAsync(user);
                        }
                    }
                    var numericId = await context.AspNetUserNumericIdentity.Where(x => x.UserId == user.Id).Select(x => x.Id).FirstOrDefaultAsync();
                    return new UserReponseModel { Id = user.Id, NumericId = numericId, Name = user.Name, UserName = user.UserName };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }
        public async Task<List<ApplicationRole>> GetRoleList()
        {
            return await roleManager.Roles.ToListAsync();
        }
        public async Task<bool> CreateRoleAsync(string roleName, int deptId)
        {
            var role = new ApplicationRole(roleName, deptId);

            if (!roleManager.Roles.Any(r => r.Name == role.Name))
            {
                await roleManager.CreateAsync(role);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<int> GetDeptIdByDeptName(string deptName)
        {
            try
            {
                var data = await context.departments.Where(d => d.DepartmentName == deptName).SingleOrDefaultAsync();
                return data.DepartmentID;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> GetDepartmentIdByRoleName(string roleName)
        {
            try
            {
                var data = await context.Roles.Where(r => r.Name.ToLower() == roleName.ToLower()).SingleOrDefaultAsync();
                return data?.DepartmentId ?? 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<bool> UpdateRoleAsync(ApplicationRole role)
        {
            var existingRole = await roleManager.FindByIdAsync(role.Id);
            if (existingRole == null)
            {
                return false;
            }
            existingRole.Name = role.Name;
            existingRole.DepartmentId = role.DepartmentId;
            var result = await roleManager.UpdateAsync(existingRole);
            return result.Succeeded;
        }
        public async Task<List<EntityType>> GetEntityTypeList()
        {
            return await context.EntityType.ToListAsync();
        }
        public async Task<bool> CreateEntityAsync(string entityName, int Status)
        {
            try
            {
                var result = await context.EntityType.Where(x => x.EntityTypeName == entityName)
                                                     .FirstOrDefaultAsync();

                if (result != null)
                {
                    result.Status = (Repository.Enums.Status)Status;
                }
                else
                {
                    var entity = new EntityType()
                    {
                        EntityTypeName = entityName,
                        Status = (Repository.Enums.Status)Status
                    };
                    await context.EntityType.AddAsync(entity);
                }
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> EntityRoleMapping(int EntityTypeId, string RoleId, int Status)
        {
            try
            {
                var result = await context.EntityRoleMapping.Where(x => x.EntityTypeId == EntityTypeId && x.RoleId == RoleId)
                                                             .FirstOrDefaultAsync();
                if (result != null)
                {
                    result.Status = (Repository.Enums.Status)Status;
                }
                else
                {
                    var entity = new EntityRoleMapping
                    {
                        RoleId = RoleId,
                        EntityTypeId = EntityTypeId,
                        Status = (Repository.Enums.Status)Status
                    };
                    await context.AddAsync(entity);
                }
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<EntityRoleMapModel>> RoleMappingList()
        {
            try
            {
                var roleList = await GetRoleList();
                var entitylist = await context.EntityType.ToListAsync();
                var rolemappinglist = await context.EntityRoleMapping.ToListAsync();

                if (rolemappinglist.Count > 0)
                {
                    var rolemaplist = rolemappinglist.Select(x => new EntityRoleMapModel
                    {
                        Id = x.Id,
                        EntityName = entitylist.Where(y => y.Id == x.EntityTypeId).Select(y => y.EntityTypeName).FirstOrDefault(),
                        RoleName = roleList.Where(y => y.Id == x.RoleId).Select(y => y.Name).FirstOrDefault(),
                        Status = (int)x.Status,
                    }).ToList();

                    return rolemaplist;
                }
                else
                {
                    return new List<EntityRoleMapModel>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<int>> GetEntityRoleMappingId(string UserId, int? deptId)
        {
            try
            {

                List<string> roleids = null;
                List<string> userRole = new List<string>().ToList();
                int? departmentId = 0;
                var user = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == UserId);



                if (user != null)
                {

                    var roles = await _userManager.GetRolesAsync(user);
                    //var roledata = roleManager.FindByNameAsync(roles.FirstOrDefault().ToLower()).Result;
                    var roledata = GetDepartmentIdByRoleName(roles.FirstOrDefault()).Result;
                    if (deptId > 0)
                        departmentId = deptId;
                    else
                        departmentId = roledata;



                    if (departmentId == 1 && roles.Contains("Applicant"))
                    {
                        userRole.Add("applicant");
                        roleids = await roleManager.Roles.Where(x => userRole.Contains(x.Name.ToLower()) && x.DepartmentId == departmentId)
                                                          .Select(x => x.Id)
                                                          .ToListAsync();
                    }
                    else if (departmentId == 2 && roles.Contains("Applicant"))
                    {
                        userRole.Add("vjnt_applicant");
                        roleids = await roleManager.Roles.Where(x => userRole.Contains(x.Name.ToLower()) && x.DepartmentId == departmentId)
                                                          .Select(x => x.Id)
                                                          .ToListAsync();
                    }
                    else
                    {
                        roleids = await roleManager.Roles.Where(x => roles.Contains(x.Name) && x.DepartmentId == departmentId)
                                                      .Select(x => x.Id)
                                                      .ToListAsync();

                    }


                    return await context.EntityRoleMapping.Where(x => roleids.Contains(x.RoleId))
                                                           .Select(x => x.Id)
                                                           .ToListAsync();
                }
                else
                    return new List<int>();
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }
        public async Task UserLoginSessionStore(string UserId, string SessionId)
        {
            try
            {
                var result = await context.AspNetUserNumericIdentity.Where(x => x.UserId == UserId).FirstOrDefaultAsync();
                if (result != null)
                {
                    result.LoginAt = DateTime.Now;
                    result.SessionId = SessionId;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

            }
        }
        public async Task<long> CreateNumericId(string UserId)
        {
            try
            {
                var userNumericIdentity = new UserNumericIdentity { UserId = UserId };
                await context.AspNetUserNumericIdentity.AddAsync(userNumericIdentity);
                await context.SaveChangesAsync();
                return userNumericIdentity.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<long> SaveloginDetails(logindetails model)
        {
            try
            {

                await context.logindetails.AddAsync(model);
                await context.SaveChangesAsync();
                return model.ID;
            }
            catch (Exception)
            {

                return 0;
            }


        }


        public async Task<logindetails> GetUserNumericId(string useridentity)
        {
            try
            {
                var data = await context.logindetails.Where(x => x.UserIdentity == useridentity).FirstOrDefaultAsync();
                return data;
            }
            catch (Exception)
            {
                return new logindetails();
            }
        }

        public async Task<bool> UpdateAadharStatus(bool IsAadharVerified, long UserId)
        {
            try
            {
                if (UserId > 0)
                {
                    var data = await context.logindetails.Where(x => x.ID == UserId).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        data.IsAadharVerified = IsAadharVerified;
                        await context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<VerifiedStatusModel> Getlogindetails()
        {
            try
            {
                var data = await context.logindetails
     .Where(x => x.ID == Convert.ToInt64(currentUserService.UserNumericId))
     .Select(s => new VerifiedStatusModel
     {
         IsAadharVerified = s.IsAadharVerified ?? false,
         IsEmailVerified = s.IsEmailVerified ?? false,
         IsMobileVerified = s.IsMobileVerified ?? false,
         IsFirstLogin = s.IsFirstLogin ?? false
     })
     .FirstOrDefaultAsync();
                if (data != null)
                    return data;
                else
                    return new VerifiedStatusModel();


            }
            catch (Exception ex)
            {

                return new VerifiedStatusModel();
            }
        }

        public async Task<Applicantdetails> GetlogindetailsByUserId(long userId)
        {
            try
            {
                var data = await context.logindetails.Where(x => x.ID == userId).Select(s => new Applicantdetails
                {
                    EmailId = s.EmailId,
                    UserName = s.UserName
                }).FirstOrDefaultAsync();
                if (data != null)
                    return data;
                else
                    return new Applicantdetails();


            }
            catch (Exception ex)
            {

                return new Applicantdetails();
            }
        }

        public async Task<List<SelectModel>> GetDeptList()
        {
            try
            {

                var data = await context.departments.Select(x => new SelectModel
                {
                    value = x.DepartmentID,
                    Text = x.DepartmentName
                }).ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async Task<List<RolesSelectModel>> GetRolesBydept(int deptId)
        {
            try
            {
                var data = await context.Roles.Where(x => x.DepartmentId == deptId)
                    .Select(x => new RolesSelectModel { value = x.Id, Text = x.Name }).ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> GetDeptIdByRoleName(string roleName)
        {
            try
            {
                var data = await context.Roles.Where(x => x.Name == roleName)
                    .FirstOrDefaultAsync();
                return (int)data.DepartmentId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DbtIntgrationModel CheckApplicantDataAvailbility(string academicYear)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("p_UserId", currentUserService.UserNumericId, DbType.Int64);
                //param.Add("p_UserId", 123, DbType.Int64);
                param.Add("p_ay", academicYear, DbType.String);
                var result = dapper.Get<DbtIntgrationModel>("usp_CheckApplicationPreFilledData", param, commandType: CommandType.StoredProcedure);

                return result;

            }
            catch (Exception ex)
            {
                ExceptionLogging.LogException(Convert.ToString(ex));
                return null;
            }
        }
        public int? InsertApplicantPrefilledData(string ResultData, string academicYear)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("p_UserId", currentUserService.UserNumericId, DbType.Int64);
                // param.Add("p_UserId", 123, DbType.Int64);
                param.Add("p_ay", academicYear, DbType.String);
                param.Add("p_applicantData", ResultData, DbType.String);
                param.Add("p_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                int result = dapper.Get<int>("usp_InsertDbtApplicantData", param, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                ExceptionLogging.LogException(Convert.ToString(ex));
                return -1;
            }
        }

        public async Task<bool> ResetApplicantPassword(string userName, string Password)
        {
            try
            {
                var user = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == userName);
                if (user != null)
                {
                    if (!string.IsNullOrWhiteSpace(Password) && (user.Password != Password))
                    {
                        user.Password = Password;
                        await _userManager.UpdateAsync(user);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CreateScoolUsersAsync(SchoolLoginCreationDto model)
        {
            try
            {
                foreach (var username in model.SchoolUDISCCode)
                {
                    string roleName = "Principle";
                    string deptName = "Namankit";
                    int departmentId = await context.departments.Where(d => d.DepartmentName == deptName).Select(d => d.DepartmentID).FirstOrDefaultAsync();
                    if (departmentId > 0)
                    {
                        var administratorRole = new ApplicationRole(roleName, departmentId);

                        if (roleManager.Roles.Any(r => r.Name.ToLower() == administratorRole.Name.ToLower()))
                        {


                            var administrator = new ApplicationUser { Name = username, UserName = username, Email = "deskuser2@gmail.com", Status = Repository.Enums.Status.Active };

                            if (!_userManager.Users.Any(u => u.UserName == administrator.UserName))
                            {
                                var result = await _userManager.CreateAsync(administrator, "Pass@123");
                                if (result.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(administrator, administratorRole.Name);
                                    await context.AspNetUserNumericIdentity.AddAsync(new UserNumericIdentity { UserId = administrator.Id });
                                    await context.SaveChangesAsync();
                                    var logindetails = new logindetails
                                    {
                                        UserIdentity = administrator.Id,
                                        //Password = model.Password,
                                        //  EmailId = model.Email,
                                        // MobileNo = model.Mobile,
                                        UserName = username,
                                        Domain = "NM",
                                        IsAadharVerified = false,
                                        IsEmailVerified = false,
                                        IsMobileVerified = false,
                                        IsFirstLogin = true,
                                        CreatedOn = DateTime.Now,
                                        CreatedBy = "admin"
                                    };
                                    await SaveloginDetails(logindetails);

                                }
                            }
                        }
                    }


                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateIsFirstLoginAsync()
        {
            try
            {
                if (Convert.ToInt64(currentUserService.UserNumericId) > 0)
                {
                    var data = await context.logindetails.Where(x => x.ID == Convert.ToInt64(currentUserService.UserNumericId)).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        data.IsFirstLogin = false;
                        await context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserEmailAndMobileInfo(UpdateUserModel m)
        {
            try
            {
                if (Convert.ToInt64(currentUserService.UserNumericId) > 0)
                {
                    var data = await context.logindetails.Where(x => x.ID == Convert.ToInt64(currentUserService.UserNumericId)).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        data.EmailId = m.Email;
                        data.MobileNo = m.PhoneNumber;
                        data.IsMobileVerified = true;
                        data.IsEmailVerified = true;
                        await context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CreateScrutinyLoginsAsync(ScrutinyLogins model)
        {
            try
            {
                string roleName = string.Empty;
                if (model.WorkflowId == 1)
                    roleName = "SS_WorkflowOne";
                else if (model.WorkflowId == 2)
                    roleName = "SS_Inspector";
                else
                    roleName = "SS_AssitComm";

                string deptName = "Namankit";
                int departmentId = await context.departments.Where(d => d.DepartmentName == deptName).Select(d => d.DepartmentID).FirstOrDefaultAsync();
                if (departmentId > 0)
                {
                    var administratorRole = new ApplicationRole(roleName, departmentId);

                    if (roleManager.Roles.Any(r => r.Name.ToLower() == administratorRole.Name.ToLower()))
                    {


                        var administrator = new ApplicationUser { Name = model.UserName, UserName = model.UserName, Email = "deskuser2@gmail.com", Status = Repository.Enums.Status.Active };

                        if (!_userManager.Users.Any(u => u.UserName == administrator.UserName))
                        {
                            var result = await _userManager.CreateAsync(administrator, "Pass@123");
                            if (result.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(administrator, administratorRole.Name);
                                await context.AspNetUserNumericIdentity.AddAsync(new UserNumericIdentity { UserId = administrator.Id });
                                await context.SaveChangesAsync();
                                var logindetails = new logindetails
                                {
                                    UserIdentity = administrator.Id,
                                    DistrictId = model.DistrictId,
                                    WorkflowId = model.WorkflowId,
                                    UserName = model.UserName,
                                    Domain = "NM",
                                    IsAadharVerified = false,
                                    IsEmailVerified = false,
                                    IsMobileVerified = false,
                                    IsFirstLogin = true,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = "admin"
                                };
                                await SaveloginDetails(logindetails);

                            }
                        }
                    }
                }



                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<logindetails> GetLogindetailByUserName(string username)
        {
            var data = await context.logindetails.Where(x => x.UserName.ToLower() == username.ToLower()).FirstOrDefaultAsync();
            return data;
        }
    }






}
