using System.Reflection;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using robot_controller_api.Persistence;
using robot_controller_api.Authentication;
using robot_controller_api.Contexts;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// enable generation of OAS file
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Robot Controller API",
        Version = "v1.0.0",
        Description = "New backend service that provides resources for the Moon robot simulator.",
        Contact = new OpenApiContact
        {
            Name = "Charlie Horn",
            Email = "s223356097@deakin.edu.au"
        }
    });

    options.AddServer(new OpenApiServer
    {
        Url = "http://localhost:5152", 
        Description = "Development HTTP"
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        xmlFilename));
});


// enable persistence layer

// ADO
// builder.Services.AddScoped<IRobotCommandDataAccess, RobotCommandADO>();
// builder.Services.AddScoped<IMapDataAccess, MapADO>();
// builder.Services.AddScoped<IUserDataAccess, UserADO>();

// Repository
// builder.Services.AddScoped<IRobotCommandDataAccess, RobotCommandRepository>();
// builder.Services.AddScoped<IMapDataAccess, MapRepository>();
// builder.Services.AddScoped<IUserDataAccess, UserRepository>();

// Entity Framework
builder.Services.AddScoped<IRobotCommandDataAccess, RobotCommandEF>();
builder.Services.AddScoped<IMapDataAccess, MapEF>();
builder.Services.AddScoped<IUserDataAccess, UserEF>();
builder.Services.AddScoped<RobotContext>();


// provide authentication details
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
    ("BasicAuthentication", default);

// enable authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin"));
    options.AddPolicy("UserOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin", "User"));
    options.AddPolicy("SelfOrAdmin", policy =>
        policy.RequireAssertion(context => 
        {
            // allow if user is admin
            if (context.User.IsInRole("Admin")) 
                return true;
                
            // get the requested user ID from route data (if it exists)
            RouteData routeData = (context.Resource as HttpContext).GetRouteData();
            
            if (routeData?.Values["id"] != null)
            {
                // get the ID from the route
                var requestedUserId = int.Parse(routeData.Values["id"].ToString());
                
                // get the ID from the user's claims
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                
                if (userIdClaim != null && int.Parse(userIdClaim.Value) == requestedUserId)
                    return true;
            }
            
            return false;
        }));
});


var app = builder.Build();

// enable authentication
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.UseStaticFiles();

// use OpenAPI specification file and UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setup => setup.InjectStylesheet("/styles/theme-muted.css"));
}

app.Run();
