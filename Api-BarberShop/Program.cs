using Api_BarberShop.Context;
using Api_BarberShop.Servicios.IServices;
using Api_BarberShop.Servicios.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserServices, UserServices>();

var sendGridApiKey = builder.Configuration["SendGrid:ApiKey"];
builder.Services.AddScoped<IEmailServices, EmailServices>(sp => new EmailServices(
    builder.Configuration["Smtp:Server"],
    int.Parse(builder.Configuration["Smtp:Port"]),
    builder.Configuration["Smtp:Username"],
    builder.Configuration["Smtp:Password"]
    ));

var key = Encoding.ASCII.GetBytes("EstaEsUnaClaveDe32BytesJustos1234");

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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var token = context.SecurityToken as JwtSecurityToken;

                if (token != null)
                {
                    var isRevoked = await dbContext.RevokedTokens.AnyAsync(rt => rt.Token == token.RawData);
                    if (isRevoked)
                    {
                        context.Fail("Token revocado");
                    }
                }
            }
        };

    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
