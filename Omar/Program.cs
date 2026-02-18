using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Omar.Data;
using Omar.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var Jwt = builder.Configuration.GetSection("JWTSetting");

// Context
builder.Services.AddDbContext<AddDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        options =>
            options.EnableRetryOnFailure(
                maxRetryCount: 5, // ��� ���� ����� ��������
                maxRetryDelay: TimeSpan.FromSeconds(10), // ��� �������� ��� ���������
                errorNumbersToAdd: null // ������� ���� ��� ������� ���� (���� ����� null ����)
            )
    );
});
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(op =>
    {
        op.User.RequireUniqueEmail = true;
        op.Password.RequireLowercase = false;
        op.Password.RequireUppercase = false;
        op.Password.RequireNonAlphanumeric = false;
        op.Password.RequireDigit = false;
        op.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AddDbContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(2);
});

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = Jwt["ValidAudience"],
            ValidIssuer = Jwt["ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Jwt["securityKey"]!)
            ),

        };

    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
