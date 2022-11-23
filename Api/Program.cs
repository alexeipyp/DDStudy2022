using Api.Services;
using Api.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Api.Middleware;
using Api.Mapper;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var authSection = builder.Configuration.GetSection(AuthConfig.Position);
            var authConfig = authSection.Get<AuthConfig>();

            builder.Services.Configure<AuthConfig>(authSection);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "Insert Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme,
                            },
                            Scheme = "oauth2",
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddDbContext<DAL.DataContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"), sql => { });
            });

            builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddTransient<AttachService>();
            builder.Services.AddScoped<PostService>();
            builder.Services.AddScoped<LinkGeneratorService>();
            builder.Services.AddScoped<SubscribeService>();
            builder.Services.AddScoped<AccessManagementService>();
            builder.Services.AddScoped<BlackListService>();
            builder.Services.AddScoped<MuteListService>();

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authConfig.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = authConfig.SymmetricSecuriryKey(),
                    ClockSkew = TimeSpan.Zero,
                };
            });

            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("ValidAccessToken", p =>
                {
                    p.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    p.RequireAuthenticatedUser();
                });
            });

            var app = builder.Build();

            using (var serviceScope = ((IApplicationBuilder)app).ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                if (serviceScope != null)
                {
                    var context = serviceScope.ServiceProvider.GetRequiredService<DAL.DataContext>();
                    context.Database.Migrate();
                }

            }

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseTokenValidator();
            app.UseCustomExceptionHandler();
            app.MapControllers();

            app.Run();
        }
    }
}