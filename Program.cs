using Azure.Identity;
using LinkUp.Interfaces;
using LinkUp.Models.Auth;
using LinkUp.Data;
using LinkUp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace LinkUp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        /* On ajoute nos services ci-dessous */
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        /* On configure Swagger (uniquement pour le développement de notre application */
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "LinkUp API", 
                Version = "v1" 
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        /* Créer une variable qui contient l'URL à notre Keyvault */
        var keyVaultURl = new Uri(builder.Configuration.GetSection("KeyVaultURL").Value!);
        var azureCredential = new DefaultAzureCredential();
        builder.Configuration.AddAzureKeyVault(keyVaultURl, azureCredential);


        /* On fais la même chose pour notre DBConnectionString, qui se situe dans notre keyvault */
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetSection("DbConnectionString").Value));


        
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        /* On gère notre JWT, les valeurs sont dans notre keyvault */
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration.GetSection("JwtIssuer").Value,
                ValidAudience = builder.Configuration.GetSection("JWTAudience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtKey").Value))
            };
        });

        /* On note ici nos services Post, Media et Comment */
        builder.Services.AddScoped<IPostService, PostService>();
        builder.Services.AddScoped<IMediaService, MediaService>();
        builder.Services.AddScoped<ICommentService, CommentService>();
        

        var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LinkUp API v1");
            });

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}

