using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using EasyDesk.SampleApp.Application.ExternalEventHandlers;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using System;

namespace EasyDesk.SampleApp.Web.DependencyInjection
{
    public class PipelineInstaller : PipelineInstallerBase
    {
        protected override Type ApplicationAssemblyMarker => typeof(CoupleGotMarried);

        protected override Type InfrastructureAssemblyMarker => typeof(SampleAppContext);

        protected override Type WebAssemblyMarker => typeof(Startup);

        protected override bool UsesPublisher => true;

        protected override bool UsesConsumer => true;
    }
}
