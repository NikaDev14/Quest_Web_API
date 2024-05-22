using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using quest_web.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using quest_web.Utils;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace quest_web
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
            /*
            var connectionString = this.Configuration.GetConnectionString("QuestWebDatabase");
            services.AddDbContext<APIDbContext>(options =>
            {
                var dbContextOptionsBuilder = options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            */
            var connectionString = "Server=127.0.0.1;port=3307;Database=quest_web;Uid=application;Pwd=password;";
            services.AddMvc();
            //services.Configure<JwtTokenUtil>(Configuration.GetSection("JwtTokenUtil"));
            services.AddScoped<JwtTokenUtil>();
            services.AddDbContext<APIDbContext>(
                DbContextOptions => 
                    DbContextOptions.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                );
            services.AddControllers().AddJsonOptions(x =>
                                                      x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "quest_web", Version = "v1" });
            });

            var JwtTokenUtil = Configuration.GetSection("JwtTokenUtil").Get<JwtTokenUtil>();
            services.AddSingleton(JwtTokenUtil);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.RequireHttpsMetadata = false;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtTokenUtil:Key"])),
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5 * 360)
                };
            });
            //services.AddDefaultIdentity<IdentityUser>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "quest_web v1"));
            }
            var context = serviceProvider.GetService<APIDbContext>();
            context.Database.EnsureCreated();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
