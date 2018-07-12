﻿// Licensed to the Swastika I/O Foundation under one or more agreements.
// The Swastika I/O Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Swastika.Cms.Lib;
using Swastika.Cms.Lib.Repositories;
using Swastika.Cms.Lib.Services;
using Swastika.Cms.Lib.ViewModels;
using Swastika.Cms.Lib.ViewModels.Account;
using Swastika.Cms.Lib.ViewModels.Api;
using Swastika.Domain.Core.ViewModels;
using Swastika.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swastka.Cms.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/portal")]
    public class ApiPortalController :
        BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ApiPortalController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        #region Get

        // GET api/category/id
        [HttpGet]
        [Route("{culture}/settings")]
        [Route("settings")]
        public RepositoryResponse<SiteSettingsViewModel> Settings()
        {
            SiteSettingsViewModel settings = new SiteSettingsViewModel()
            {
                Lang = _lang,
                ThemeId = GlobalConfigurationService.GetLocalInt(SWCmsConstants.ConfigurationKeyword.ThemeId, _lang),
                Cultures = GlobalLanguageService.ListSupportedCulture,
                PageTypes = Enum.GetNames(typeof(SWCmsConstants.CateType)).ToList()
            };
            return new RepositoryResponse<SiteSettingsViewModel>()
            {
                IsSucceed = true,
                Data = settings
            };
        }

        [HttpGet]
        [Route("init-settings")]
        public RepositoryResponse<SiteSettingsViewModel> InitSettings()
        {
            SiteSettingsViewModel settings = new SiteSettingsViewModel()
            {
                Lang = _lang,
                ThemeId = 1,
                Cultures = new List<Swastika.Domain.Core.Models.SupportedCulture>()
                {
                    new Swastika.Domain.Core.Models.SupportedCulture()
                    {
                        Specificulture = "en-us",
                        FullName = "United States",
                        Description = "United States",
                        Icon = "flag-icon-us",
                        Alias = "US"
                    },
                    new Swastika.Domain.Core.Models.SupportedCulture()
                    {
                        Specificulture = "vi-vn",
                        FullName = "Vietnam",
                        Description = "Việt Nam",
                        Icon = "flag-icon-vn",
                        Alias = "VN"
                    }

                },
                PageTypes = Enum.GetNames(typeof(SWCmsConstants.CateType)).ToList()
            };
            return new RepositoryResponse<SiteSettingsViewModel>()
            {
                IsSucceed = true,
                Data = settings
            };
        }

        // GET api/category/id
        [HttpGet]
        [Route("dashboard")]
        public RepositoryResponse<DashboardViewModel> Dashboard(int id)
        {
            return new RepositoryResponse<DashboardViewModel>()
            {
                IsSucceed = true,
                Data = new DashboardViewModel()
            };
        }

        #endregion Get

        #region Post

        // POST api/category
        [HttpPost, HttpOptions]
        [Route("init-cms")]
        public async Task<RepositoryResponse<bool>> Post([FromBody]ApiInitCmsViewModel model)
        {
            if (model != null)
            {
                var result = await InitCmsAsync(model).ConfigureAwait(false);
                return result;
            }
            return new RepositoryResponse<bool>();
        }

        #endregion Post

        #region Helpers
        public async Task<RepositoryResponse<bool>> InitCmsAsync(ApiInitCmsViewModel model)
        {
            var result = new RepositoryResponse<bool>();

            var settings = FileRepository.Instance.GetFile("appsettings", ".json", string.Empty, true, "{}");
            if (settings != null)
            {
                JObject jsonSettings = JObject.Parse(settings.Content);
                jsonSettings["ConnectionStrings"][SWCmsConstants.CONST_DEFAULT_CONNECTION] = model.ConnectionString;
                // Set connection string for identity ApplicationDbContext
                jsonSettings["ConnectionStrings"]["AccountConnection"] = model.ConnectionString;
                jsonSettings["isSqlite"] = model.IsSqlite;
                settings.Content = jsonSettings.ToString();
                FileRepository.Instance.SaveFile(settings);
            }
            
            GlobalConfigurationService.Instance.IsSqlite = model.IsSqlite;
            if (model.IsSqlite)
            {
                GlobalConfigurationService.Instance.ConnectionString = model.SqliteDbConnectionString;
            }
            else
            {
                GlobalConfigurationService.Instance.ConnectionString = model.ConnectionString;
            }
            var initResult = await GlobalConfigurationService.Instance.InitSWCms(
                _userManager, _roleManager);
            if (initResult.IsSucceed)
            {
                await InitRolesAsync();
                SWCmsConstants.Default.Specificulture = model.Lang;
                result.IsSucceed = true;
            }
            else
            {
                settings = FileRepository.Instance.GetFile("appsettings", ".json", string.Empty);
                JObject jsonSettings = JObject.Parse(settings.Content);
                jsonSettings["ConnectionStrings"][SWCmsConstants.CONST_DEFAULT_CONNECTION] = null;
                jsonSettings["ConnectionStrings"]["AccountConnection"] = null;
                jsonSettings["isSqlite"] = false;
                settings.Content = jsonSettings.ToString();
                FileRepository.Instance.SaveFile(settings);
                if (initResult.Exception != null)
                {
                    result.Errors.Add(initResult.Exception.Message);
                }
                foreach (var item in initResult.Errors)
                {
                    result.Errors.Add(item);
                }
            }
            return result;
        }

        private async Task<bool> InitRolesAsync()
        {
            bool isSucceed = true;
            var getRoles = await RoleViewModel.Repository.GetModelListAsync();
            if (getRoles.IsSucceed && getRoles.Data.Count == 0)
            {
                var saveResult = await _roleManager.CreateAsync(new IdentityRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "SuperAdmin"
                });
                isSucceed = saveResult.Succeeded;
            }
            return isSucceed;
        }

        #endregion
    }
}
