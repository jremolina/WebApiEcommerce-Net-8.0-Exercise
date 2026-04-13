using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// añadiendo cache.
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024;
    options.UseCaseSensitivePaths = true;
});


builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


//implementacion de automapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
});


// implementacion de Autenticacion/autorizacion, permite que solo los usuarios autenticados accedan a los endpoints
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("Secret key no esta configurada");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; //desactivar uso de https, en produccion debe estar en true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Add services to the container.

builder.Services.AddControllers(option =>// creacion de perfiles de cache para usar en controladores, ver ejemplo categorias
{
    // option.CacheProfiles.Add("Default10", new CacheProfile() { Duration = 10 });
    // option.CacheProfiles.Add("Default20", new CacheProfile() { Duration = 20 });

    option.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.Profile10);// usando las constantes definidas
    option.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.Profile20);

});





// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(
  options =>
  {
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
          Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                      "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                      "Ejemplo: \"12345abcdef\"",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.Http,
          Scheme = "Bearer"
      });
      options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });


      // doucmentacion de api
      options.SwaggerDoc("v2", new OpenApiInfo
      {
          Version = "v2",
          Title = "Api Ecommerce V2",
          Description = "Api para gestion de productos y usuarios",
          TermsOfService = new Uri("https://www.example.com/terms"),
          Contact = new OpenApiContact
          {
              Name = "Devtalles",
              Url = new Uri("https://www.google.com")
          },
          License = new OpenApiLicense
          {
              Name = "Licencia de Uso",
              Url = new Uri("https://www.example.com/license")
          }
      });
      options.SwaggerDoc("v1", new OpenApiInfo
      {
          Version = "v1",
          Title = "Api Ecommerce",
          Description = "Api para gestion de productos y usuarios",
          TermsOfService = new Uri("https://www.example.com/terms"),
          Contact = new OpenApiContact
          {
              Name = "Devtalles",
              Url = new Uri("https://www.google.com")
          },
          License = new OpenApiLicense
          {
              Name = "Licencia de Uso",
              Url = new Uri("https://www.example.com/license")
          }
      });
  }
);


//versionnamiento de endpoints - ver controladores
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    //options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));
});

apiVersioningBuilder.AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV";
    option.SubstituteApiVersionInUrl = true; //api/v(version)/products
});



builder.Services.AddCors(options =>
{
    options.AddPolicy(PolicyNames.AllowSpecificOrigin, builder =>
    {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    }
    );
}
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
        }
    );
}

app.UseHttpsRedirection();
//app.UseCors("AllowSpecificOrigin");
app.UseCors(PolicyNames.AllowSpecificOrigin);

// añadiendo cache, importante, debe ir debajo de app.usecors
app.UseResponseCaching();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
