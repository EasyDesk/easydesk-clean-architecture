using EasyDesk.CleanArchitecture.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace EasyDesk.SampleApp.Web
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment) : base(configuration, environment)
        {
        }

        protected override bool UseAuthentication => false;

        protected override bool UseAuthorization => false;

        protected override bool UseSwagger => true;
    }
}
