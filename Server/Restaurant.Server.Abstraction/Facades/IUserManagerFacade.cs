﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Restaurant.Server.Models;

namespace Restaurant.Server.Abstraction.Facades
{
	public interface IUserManagerFacade
	{
		Task<IdentityResult> Create(User user, string password);

		Task<User> GetAsync(ClaimsPrincipal principal);

		Task<List<User>> GetAllUsers();
	}
}