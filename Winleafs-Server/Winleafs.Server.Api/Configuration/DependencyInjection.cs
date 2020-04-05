using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Data;

namespace Template.Api.Configuration
{
    public static class DependencyInjection
    {
        /// <summary>
        ///     Adds the patters to the patterns code to the services so the repositories and services work.
        /// </summary>
        /// <param name="services">The current service collection.</param>
        /// <returns>The <paramref name="services" /> enriched with the patterns objects.</returns>
        public static IServiceCollection AddPatterns(this IServiceCollection services)
        {
            services.AddScoped<DbContext, ApplicationContext>();

            return services;
        }

        /// <summary>
        ///     Configures the injection for the services and repositories.
        /// </summary>
        /// <param name="services">The current service collection.</param>
        /// <returns>The <paramref name="services" /> enriched with the services and repositories.</returns>
        public static IServiceCollection AddServicesAndRepositories(this IServiceCollection services)
        {
            // Add new services and repositories here.
            // Done like:
            // services.AddScoped<IUserService, UserService>();
            // services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}