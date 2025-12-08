using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Model.Admin;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Entity;
using System;
using System.Data;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Service.Configuration;

public static class ApplicationSeedingConfig
{
    public async static Task AddSeed(this IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Database.IsSqlServer())
        {
            context.Database.Migrate();
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();


        await ApplicationDbContextSeed.SeedDefaultUserAsync(userManager, roleManager, context);
        //  await ApplicationDbContextSeed.SeedDefaultApplicantAsync(userManager, roleManager, context);
        // await ApplicationDbContextSeed.SeedDefaultWardenAsync(userManager, roleManager, context);
        // await ApplicationDbContextSeed.SeedAllWardenAsync(userManager, roleManager, context);
        //await ApplicationDbContextSeed.SeedDefaultDeptUserAsync(userManager, roleManager, context);
    }
}
public static class ApplicationDbContextSeed
{
    public static async Task SeedDefaultUserAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        List<string> roles = new List<string> { "New_student" };

        foreach (string role in roles)
        {
            var administratorRole = new ApplicationRole(role, 1);

            if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await roleManager.CreateAsync(administratorRole);
            }
        }
        //string roleName = "DDO";
        //string deptName = "Social Justice & Special Assistance Department (SJSA)";
        //int departmentId = await context.departments.Where(d => d.DepartmentName == deptName).Select(d => d.DepartmentID).FirstOrDefaultAsync();
        //if (departmentId > 0)
        //{
        //    var administratorRole = new ApplicationRole(roleName, departmentId);

        //    if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
        //    {
        //        await roleManager.CreateAsync(administratorRole);
        //    }
        //    var administrator = new ApplicationUser { Name = "Desk User 2", UserName = "DDO", Email = "deskuser2@gmail.com", Status = Repository.Enums.Status.Active };

        //    if (userManager.Users.All(u => u.UserName != administrator.UserName))
        //    {
        //        var result = await userManager.CreateAsync(administrator, "Pass@123");
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(administrator,administratorRole.Name );
        //            await context.AspNetUserNumericIdentity.AddAsync(new UserNumericIdentity { UserId = administrator.Id });
        //            await context.SaveChangesAsync();
        //        }
        //    }
        //}
    }
    public static async Task SeedDefaultApplicantAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        string roleName = "DDO";
        string deptName = "Social Justice & Special Assistance Department (SJSA)";
        int departmentId = await context.departments.Where(d => d.DepartmentName == deptName).Select(d => d.DepartmentID).FirstOrDefaultAsync();
        if (departmentId > 0)
        {
            var applicantRole = new ApplicationRole(roleName, departmentId);

            if (roleManager.Roles.All(r => r.Name != applicantRole.Name))
            {
                await roleManager.CreateAsync(applicantRole);
            }
            var applicant = new ApplicationUser { Name = "DDO", UserName = "DDO_2024", Email = "user1@gmail.com", Status = Repository.Enums.Status.Active };

            if (userManager.Users.All(u => u.UserName != applicant.UserName))
            {
                var result = await userManager.CreateAsync(applicant, "Password@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(applicant, applicantRole.Name);
                    await context.AspNetUserNumericIdentity.AddAsync(new UserNumericIdentity { UserId = applicant.Id });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
    public static async Task SeedDefaultWardenAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        string roleName = "Scrutiny";
        string deptName = "Social Justice & Special Assistance Department (SJSA)";
        int departmentId = await context.departments.Where(d => d.DepartmentName == deptName).Select(d => d.DepartmentID).FirstOrDefaultAsync();
        if (departmentId > 0)
        {
            var WardenRole = new ApplicationRole(roleName, departmentId);



            if (roleManager.Roles.All(r => r.Name != WardenRole.Name))
            {
                await roleManager.CreateAsync(WardenRole);
            }

            List<string> user = new List<string>()
        {
            "AsstCommAhmednagar",
            "AsstCommAkola",
            "AsstCommAmravati",
            "AsstCommAurangabad",
            "AsstCommBeed",
            "AsstCommBhandara",
            "AsstCommBuldana",
            "AsstCommChandrapur",
            "AsstCommDhule",
            "AsstCommGadchiroli",
            "AsstCommGondiya",
            "AsstCommHingoli",
            "AsstCommJalgaon",
            "AsstCommJalna",
            "AsstCommKolhapur",
            "AsstCommLatur",
            "AsstCommMumbaiCity",
            "AsstCommMumbaiSuburban",
            "AsstCommNagpur",
            "AsstCommNanded",
            "AsstCommNandurbar",
            "AsstCommNashik",
            "AsstCommOsmanabad",
            "AsstCommPalghar",
            "AsstCommParbhani",
            "AsstCommPune",
            "AsstCommRaigarh",
            "AsstCommRatnagiri",
            "AsstCommSangli",
            "AsstCommSatara",
            "AsstCommSindhudurg",
            "AsstCommSolapur",
            "AsstCommThane",
            "AsstCommWardha",
            "AsstCommWashim",
            "AsstCommYavatmal",
        };

            foreach (var item in user)
            {
                var warden = new ApplicationUser { Name = item, UserName = item, Email = item + "@gmail.com", Status = Repository.Enums.Status.Active };

                if (userManager.Users.All(u => u.UserName != warden.UserName))
                {
                    var result = await userManager.CreateAsync(warden, "Password@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(warden, WardenRole.Name);
                        await context.AspNetUserNumericIdentity.AddAsync(new UserNumericIdentity { UserId = warden.Id });
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
    public static async Task SeedDefaultDeptUserAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {

        string roleName = "Scrutiny";
        string deptName = "Social Justice & Special Assistance Department (SJSA)";
        int departmentId = await context.departments.Where(d => d.DepartmentName == deptName).Select(d => d.DepartmentID).FirstOrDefaultAsync();
        if (departmentId > 0)
        {
            var deptRole = new ApplicationRole(roleName, departmentId);



            if (roleManager.Roles.All(r => r.Name != deptRole.Name))
            {
                await roleManager.CreateAsync(deptRole);
            }


            //var dept = new ApplicationUser { Name = item, UserName = item, Email = $"{item}@gmail.com", Status = Repository.Enums.Status.Active };

            //if (userManager.Users.All(u => u.UserName != dept.UserName))
            //{
            //    var result = await userManager.CreateAsync(dept, "Password@123");
            //    if (result.Succeeded)
            //    {
            //        await userManager.AddToRoleAsync(dept, new[] { deptRole.Name });
            //        await context.AspNetUserNumericIdentity.AddAsync(new UserNumericIdentity { UserId = deptRole.Id });
            //        await context.SaveChangesAsync();
            //    }
            //}
        }

    }
    public static async Task SeedAllWardenAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        try
        {
            string deptName = "Social Justice & Special Assistance Department (SJSA)";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7062/master-service/api/Hostel/List");
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                var Data = JsonSerializer.Deserialize<List<HostelModel>>(res);

                if (Data != null && Data.Count > 0)
                {

                    string roleName = "Scrutiny";
                    int departmentId = await context.departments.Where(d => d.DepartmentName == deptName).Select(d => d.DepartmentID).FirstOrDefaultAsync();
                    if (departmentId > 0)
                    {
                        var WardenRole = new ApplicationRole(roleName, departmentId);



                        if (roleManager.Roles.All(r => r.Name != WardenRole.Name))
                        {
                            await roleManager.CreateAsync(WardenRole);
                        }
                        //var intlist = new int[]{ 92, 97, 104, 142, 194, 410, 430 };
                        //Data = Data.Where(x => intlist.Contains(x.HostelID)).ToList();

                        foreach (var item in Data)
                        {
                            var warden = new ApplicationUser { Name = item.Name, UserName = item.DDOCode.ToString(), Status = Repository.Enums.Status.Active };

                            if (!userManager.Users.Any(u => u.UserName == warden.UserName))
                            {
                                var result = await userManager.CreateAsync(warden, "Password@123");
                                if (result.Succeeded)
                                {
                                    await userManager.AddToRoleAsync(warden, WardenRole.Name);
                                    await context.AspNetUserNumericIdentity.AddAsync(new UserNumericIdentity { UserId = warden.Id });
                                    await context.SaveChangesAsync();

                                    var request2 = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7231/hostel-service/api/Warden/warden-map");


                                    string content = JsonSerializer.Serialize(new { HostelId = item.HostelID, UserIdentity = warden.Id, Name = item.Name, Mobile = item.Mobile });
                                    request2.Content = new StringContent(content, null, "application/json");
                                    HttpClient httpClient2 = new HttpClient();
                                    var response2 = await httpClient2.SendAsync(request2);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {

            throw;
        }

    }
}