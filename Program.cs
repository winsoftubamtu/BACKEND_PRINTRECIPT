using Microsoft.EntityFrameworkCore;
using finalhotelAPI.Data;
using finalhotelAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace finalhotelAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false, // ? No expiry
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowIonicApp",
                    policy =>
                    {
                        //policy.WithOrigins("http://localhost:8100") // Ionic dev server
                        //      .AllowAnyHeader()
                        //      .AllowAnyMethod();

                        policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();

                    });
            });
            //builder.Services.AddAuthentication(); // if using JWT or Identity
            //builder.Services.AddAuthorization();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            // builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "finalhotelAPI",
                    Version = "v1"
                });

                // ?? Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token.Example: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI..."
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });


            builder.Services.AddDbContext<WebDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


            var app = builder.Build();

           

            // Configure the HTTP request pipeline.
           // if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //he if chi condition local la run karayla aani production la pn run karayla
            if (!app.Environment.IsDevelopment()) {
                app.UseHttpsRedirection();
            }
               


            // Enable CORS before authentication/authorization
            app.UseCors("AllowIonicApp");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.Run("http://localhost:5101");
            //app.Run();

        }
    }
}
