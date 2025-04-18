﻿using LMS.Enums;
using LMS.Models;
using Microsoft.AspNetCore.Identity;

//adapted from https://codewithmukesh.com/blog/user-management-in-aspnet-core-mvc/

namespace LMS.Areas.Identity.Data;

public static class SeedRoles
{
    public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        //Seed Roles
        await roleManager.CreateAsync(new IdentityRole(Roles.Administrator.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Roles.Professor.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Roles.Student.ToString()));
    }
}