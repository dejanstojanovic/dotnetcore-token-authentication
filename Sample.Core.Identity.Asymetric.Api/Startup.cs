using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sample.Core.Common.Extensions;
using Sample.Core.Common.Helpers;
using Sample.Core.Identity.Data.DbContexts;
using Sample.Core.Identity.Data.Enities;

namespace Sample.Core.Identity.Asymetric.Api
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
            #region Add CORS
            services.AddCors(options => options.AddPolicy("Cors", builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));
            #endregion

            #region Add Entity Framework and Identity Framework

            services.AddDbContext<ApplicationUserDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DatabaseConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationUserDbContext>();

            #endregion

            #region Add Authentication

            RSA publicRsa = RsaHelper.PublicKeyFromPemFile(Path.Combine(Directory.GetCurrentDirectory(),
                                "Keys",
                                 this.Configuration.GetValue<String>("Tokens:PublicKey")
                                 ));
            
                
                RsaSecurityKey signingKey = new RsaSecurityKey(publicRsa);


                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(config =>
                {
                    config.RequireHttpsMetadata = false;
                    config.SaveToken = true;
                    config.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = signingKey,
                        ValidateAudience = true,
                        ValidAudience = this.Configuration["Tokens:Audience"],
                        ValidateIssuer = true,
                        ValidIssuer = this.Configuration["Tokens:Issuer"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });
            
            #endregion

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("Cors");
            app.UseAuthentication();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

        }
    }
}
