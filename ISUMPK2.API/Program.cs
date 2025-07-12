using ISUMPK2.Application.Auth;
using ISUMPK2.Application.Services;
using ISUMPK2.Application.Services.Implementations;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using ISUMPK2.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Web.Services;
using ISUMPK2.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. ������� ������������
builder.Services.AddControllers();

// 2. SignalR
builder.Services.AddSignalR();

// 3. Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ISUMPK2 API",
        Version = "v1",
        Description = "API ��� ������� ������.\n� ������ �������� ��������� �: houtuy96@gmail.com",
        Contact = new OpenApiContact
        {
            Email = "houtuy96@gmail.com"
        }
    });

    // ��������� JWT � Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", builder =>
    {
        builder.WithOrigins("https://localhost:7062", "http://localhost:7062") // ��������� HTTP ������
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin =>
               {
                   Console.WriteLine($"CORS ������ ��: {origin}");
                   return origin.StartsWith("https://localhost:7062") ||
                          origin.StartsWith("http://localhost:7062");
               });
    });
});

// 5. ������������ ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 6. ���������� HttpContextAccessor (����� ��� UserService)
builder.Services.AddHttpContextAccessor();

// 7. ����������� �������� ������������
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IMaterialCategoryRepository, MaterialCategoryRepository>();
builder.Services.AddScoped<ISubTaskRepository, SubTaskRepository>();
builder.Services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();
builder.Services.AddScoped<ITaskMaterialRepository, TaskMaterialRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// 8. ����������� ����� ����������
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISubTaskService, SubTaskService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskMaterialService, TaskMaterialService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISUMPK2.Application.Services.INotificationService, ISUMPK2.Application.Services.Implementations.NotificationService>();
builder.Services.AddScoped<ISUMPK2.Application.Services.IMaterialService, MaterialService>();
builder.Services.AddScoped<IChatService, ChatService>();

// 9. ����������� ����� ��������������
builder.Services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// 10. ��������� JWT ��������������
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
        ClockSkew = TimeSpan.Zero // ������� ������ �� �������
    };

    // ��������� ������� JWT
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/notificationHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },

        // ��������� �������
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT �������������� �� �������: {context.Exception.Message}");
            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            Console.WriteLine($"JWT ����� ����������� ��� ������������: {context.Principal.Identity.Name}");
            foreach (var claim in context.Principal.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            Console.WriteLine($"JWT Challenge: {context.Error} - {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// 11. ��������� �����������
builder.Services.AddAuthorization();

var app = builder.Build();

// === �������� ��������� �������� ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISUMPK2 API v1");
        c.RoutePrefix = string.Empty;
        c.DocExpansion(DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseStaticFiles();
app.UseCors("AllowBlazorClient");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ����������� ����� SignalR
app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();