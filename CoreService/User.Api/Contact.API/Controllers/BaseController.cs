﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Contact.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : Controller
    {
        protected UserIdentity UserIdentity => new UserIdentity() {
            UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "sub").Value),
            Name= User.Claims.FirstOrDefault(c => c.Type == "name").Value,
            Company = User.Claims.FirstOrDefault(c => c.Type == "company").Value,
            Title = User.Claims.FirstOrDefault(c => c.Type == "title").Value,
            Avatar= User.Claims.FirstOrDefault(c => c.Type == "avatar").Value

        };


    }
}
