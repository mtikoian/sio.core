﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swastika.Cms.Lib.Repositories;
using Swastika.Cms.Lib.Services;
using Swastika.Cms.Lib.ViewModels.Account;
using Swastika.Domain.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Swastika.Cms.Lib.ViewModels.Api
{
    public class ApiInitCmsViewModel
    {
        #region Properties
        [JsonProperty("connectionString")]
        public string ConnectionString
        {
            get
            {
                return IsUseLocal
                    ? LocalDbConnectionString
                    : $"Server={DataBaseServer};Database={DataBaseName}" +
                    $";UID={DataBaseUser};Pwd={DataBasePassword};MultipleActiveResultSets=true;"
                    ;
            }
        }

        [JsonProperty("dataBaseServer")]
        public string DataBaseServer { get; set; }

        [JsonProperty("dataBaseName")]
        public string DataBaseName { get; set; }

        [JsonProperty("dataBaseUser")]
        public string DataBaseUser { get; set; }

        [JsonProperty("dataBasePassword")]
        public string DataBasePassword { get; set; }

        [JsonProperty("isUseLocal")]
        public bool IsUseLocal { get; set; }

        [JsonProperty("localDbConnectionString")]
        public string LocalDbConnectionString { get; set; } = $"Server=(localdb)\\mssqllocaldb;Database=aspnet-swastika.Cms.Db;Trusted_Connection=True;MultipleActiveResultSets=true";

        [JsonProperty("superAdminsuperAdmin")]
        public string SuperAdmin { get; set; }

        [JsonProperty("adminPassword")]
        public string AdminPassword { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }
        #endregion

        #region View

        
        #endregion

        public ApiInitCmsViewModel()
        {

        }

        
    }
}
