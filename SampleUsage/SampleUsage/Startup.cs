using System.Collections.Generic;
using AspnetCore.ServiceCollection.MultiTenancy;
using AspnetCore.ServiceCollection.MultiTenancy.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleUsage.Services;
using SampleUsage.Services.Contracts;

namespace SampleUsage
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var tenants = new List<ITenant>
			{
				new Tenant(1),
				new Tenant(2)
			};

			services.AddControllers();
			services.AddMultiTenancy(tenants);

			Register(services, tenants);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();
			app.UseTenantMiddleware();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private void Register(IServiceCollection serviceCollection, IList<ITenant> tenants)
		{
			foreach (var tenant in tenants)
			{
				if (tenant.Id == 1)
					serviceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(DefaultFooService), ServiceLifetime.Scoped);
				else
					serviceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(SpecialFooService), ServiceLifetime.Scoped);

				//this singleton is tenant scoped.
				//there will be a singleton for each tenant
				serviceCollection.AddTenantScoped(tenant, typeof(IBarService), typeof(BarService), ServiceLifetime.Singleton);
			}

			serviceCollection.AddSingleton<ITenantProvider, DefaultTenantProvider>();

			serviceCollection.AddSingleton<IBazService, BazService>();
		}
	}
}
