﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using User.Api.Data;
using User.Api.Model;

namespace User.Api.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UserController : BaseController
    {
        UserContext _userContext;
        private ICapPublisher _capPublisher;
        public UserController(UserContext userContext, ILogger<UserController> logger, ICapPublisher capPublisher)
        {
            _userContext = userContext;
            _capPublisher = capPublisher;
        }

        /// <summary>
        /// //发布用户变更消息
        /// </summary>
        /// <param name="appUser"></param>
        private void RaiseUserprofileChanagedEvent(AppUser appUser)
        {
            if (_userContext.Entry(appUser).Property(nameof(appUser.Name)).IsModified ||
                _userContext.Entry(appUser).Property(nameof(appUser.Title)).IsModified ||
                _userContext.Entry(appUser).Property(nameof(appUser.Company)).IsModified ||
                _userContext.Entry(appUser).Property(nameof(appUser.Avatar)).IsModified
                )
            {
                _capPublisher.Publish("finbook.userapi.userprofilechanged", new Dtos.UserIdentity()
                {
                    UserId = appUser.Id,
                    Name=appUser.Name,
                    Title=appUser.Title,
                    Company=appUser.Company,
                    Avatar=appUser.Avatar
                }); ;
            }
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users.AsNoTracking().Include(u => u.Properties).SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
            if (user == null)
                //return NotFound();
                throw new UserOperationException("错误的用户上下文ID");
            return Json(user);

        }
        [Route("")]
        [HttpPatch]

        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<Model.AppUser> patch)
        {
            /*
                       {
                           "op":"replace",
                           "path":"/Company",
                           "value":"adminA"
                       } 
             */
            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
            patch.ApplyTo(user);


            foreach (var property in user?.Properties)
            {
                _userContext.Entry(property).State = EntityState.Detached;
            }

            var originProperties = await _userContext.UserProperty.AsNoTracking().Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync();
            //合并，去重
            var allProperties = originProperties.Union(user.Properties).Distinct();
            //这里的意思是strList1中哪些是strList2中没有的,并将获得的差值存放在strList3(即: strList1中有, strList2中没有)
            var removedProperties = originProperties.Except(user.Properties);
            var newProperties = allProperties.Except(originProperties);

            foreach (var property in removedProperties)
            {
                _userContext.Remove(property);
            }
            foreach (var item in newProperties)
            {
                _userContext.Add(item);
            }
            using (var transaction = _userContext.Database.BeginTransaction())
            {
                _userContext.Users.Update(user);
                await _userContext.SaveChangesAsync();
                //发布用户变更消息
                RaiseUserprofileChanagedEvent(user);
                transaction.Commit();
            }
            return Json(user);
        }



        [Route("check-orcreate")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate([FromForm]string phone)
        {
            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Phone == phone);
            //if (!await _userContext.Users.AsNoTracking().Include(u => u.Properties).AnyAsync(u => u.Phone == phone))
            if (user == null)
            {
                user = new Model.AppUser() { Phone = phone };
                _userContext.Users.Add(user);
                await _userContext.SaveChangesAsync();
            }
            return Ok(new
            {
                userID = user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }

        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            var ret = await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync();
            return Ok(ret);
        }
        [HttpPost]
        [Route("Search")]
        public async Task<IActionResult> Search(string phone)
        {
            var ret = await _userContext.Users.Include(u => u.Properties).SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
            return Ok(ret);
        }

        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateUsetTas([FromBodyAttribute]List<string> tags)
        {
            var originTags = await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync();
            var newTags = tags.Except(originTags.Select(a => a.Tag));
            await _userContext.UserTags.AddRangeAsync(newTags.Select(a => new Model.UserTag()
            {
                CreatedTime = DateTime.Now,
                UserId = UserIdentity.UserId,
                Tag = a
            }));
            await _userContext.SaveChangesAsync();
            return Ok();
        }


        [HttpGet]
        [Route("baseinfo/{userId}")]
        public async Task<IActionResult> GetBaseInfo(int userId)
        {
            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                userId = user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }


    }
}
