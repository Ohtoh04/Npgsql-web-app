using Courses.Api.Schema.Queries;
using Courses.Api.Schema.Subscriptions;
using Courses.Api.Schema.Types;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<CourseType>()
    .AddSubscriptionType<SolutionSubscription>()
    .AddInMemorySubscriptions();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseWebSockets();
app.MapGraphQL();

app.Run();
