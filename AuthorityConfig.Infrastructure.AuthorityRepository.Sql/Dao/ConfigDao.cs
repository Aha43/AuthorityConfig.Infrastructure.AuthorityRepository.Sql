using IdentityServer4.Models;
using System.Collections.Generic;

namespace AuthorityConfig.Infrastructure.AuthorityRepository.Sql.Dao
{
    public class ConfigDao
    {
        public IEnumerable<Client> Clients { get; set; } = new Client[] { };
        public IEnumerable<ApiScope> Apis { get; set; } = new ApiScope[] { };
        public IEnumerable<IdentityResource> IdentityResources { get; set; } = new IdentityResource[] { };
        public IEnumerable<IdProviderOptionsDao> IdProviders { get; set; } = new IdProviderOptionsDao[] { };

        public bool EnableLocalLogin { get; set; }
        public bool EnableWindowsAuthentication { get; set; }

        public bool ShowNewPasswordLink { get; set; } = true;
        public bool NoUserPasswordButton { get; set; } = false;
        public bool MobileFirst { get; set; } = false;
    }
}
