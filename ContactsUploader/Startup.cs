using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ContactsUploader.Startup))]
namespace ContactsUploader
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
