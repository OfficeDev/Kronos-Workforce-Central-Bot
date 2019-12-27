/// <summary>
/// 
/// </summary>

namespace Microsoft.Teams.App.KronosWfc.Configurator.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Configurator.Models;
    using Configurator.Providers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TenantConfigController : ControllerBase
    {
        private readonly ITenantMappingProvider tenantMappingProvider;
        private readonly IPaycodeMappingProvider paycodeMappingProvider;
        private readonly IKeyVaultHelper keyVaultHelper;
        private readonly IIdentityService identityService;
        private readonly IConfiguration configuration;
        public TenantConfigController(ITenantMappingProvider tenantMappingProvider, IPaycodeMappingProvider paycodeMappingProvider, IKeyVaultHelper keyVaultHelper, IIdentityService identityService, IConfiguration configuration)
        {
            this.tenantMappingProvider = tenantMappingProvider;
            this.paycodeMappingProvider = paycodeMappingProvider;
            this.keyVaultHelper = keyVaultHelper;
            this.identityService = identityService;
            this.configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> GetPaycodeMapping()
        {
            if (!this.identityService.GetMail().Equals(this.configuration["UPN"]))
            {
                return this.Unauthorized();
            }
            try
            {
                var paycodes = await this.paycodeMappingProvider.RetrieveAllAsync();
                return this.Ok(paycodes);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> GetTenantInfo()
        {
            if (!this.identityService.GetMail().Equals(this.configuration["UPN"]))
            {
                return this.Unauthorized();
            }
            try
            {
                var tenant = await this.tenantMappingProvider.GetAsync();
                return this.Ok(tenant.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SetTenantInfo([FromBody]TenantMappingEntities tenantMappingEntity)
        {
            if (!this.identityService.GetMail().Equals(this.configuration["UPN"]))
            {
                return this.Unauthorized();
            }
            try
            {
                var deletionResult = await this.tenantMappingProvider.DeleteAllAsync();
                if (deletionResult)
                {
                    var setResult = await this.tenantMappingProvider.SetAsync(tenantMappingEntity);
                    if (setResult)
                        return this.Ok();
                    else
                        return this.BadRequest();
                }
                else
                {
                    return this.BadRequest();
                }
                
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SetPaycodeMapping([FromBody] List<PaycodeMappingEntities> paycodeList)
        {
            if (!this.identityService.GetMail().Equals(this.configuration["UPN"]))
            {
                return this.Unauthorized();
            }
            try
            {
                var deletionResult = await this.paycodeMappingProvider.DeleteAllAsync();
                if (deletionResult)
                {
                    var insertionResult = await this.paycodeMappingProvider.AddBatchAsync(paycodeList);
                    if (insertionResult)
                    {
                        return this.Ok();
                    }
                }

                return this.BadRequest();
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult<SuperuserConfig> GetSuperuserConfig()
        {
            if (!this.identityService.GetMail().Equals(this.configuration["UPN"]))
            {
                return this.Unauthorized();
            }
            try
            {
                var superuserConfigDetails = new SuperuserConfig { SuperUsername = this.keyVaultHelper.GetSecret("KronosWfcSuperUserName"), SuperUserPassword = this.keyVaultHelper.GetSecret("KronosWfcSuperUserPwd") };
                return this.Ok(superuserConfigDetails);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public ActionResult SetSuperuserConfig([FromBody]SuperuserConfig superuserConfig)
        {
            if (!this.identityService.GetMail().Equals(this.configuration["UPN"]))
            {
                return this.Unauthorized();
            }
            try
            {
                this.keyVaultHelper.SetSecretAsync("KronosWfcSuperUserName", superuserConfig.SuperUsername);
                this.keyVaultHelper.SetSecretAsync("KronosWfcSuperUserPwd", superuserConfig.SuperUserPassword);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message.ToString());
            }
        }


    }
}
