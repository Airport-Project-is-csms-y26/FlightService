#pragma warning disable CA1506

using FlightService.Application.BackgroundServices;
using FlightService.Application.Extensions;
using FlightService.Infrastructure.Persistence.Extensions;
using FlightService.Presentation.Grpc.Controllers;
using FlightService.Presentation.Grpc.Interceptors;
using FlightService.Presentation.Kafka.Extensions;
using FlightService.Presentation.Kafka.Models;
using PassengerService.Infrastructure.Persistence.Options;
using ServiceManagement.Presentation.Kafka.Configuration;
using Tasks.Kafka.Contracts;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();

// builder.Services.Configure<ClientOptions>(builder.Configuration.GetSection("Client:Configuration"));
builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Persistence:Postgres"));

builder.Services.Configure<ProducerOptions>("FlightCreation", builder.Configuration.GetSection("Kafka:Producers:FlightCreation"));
builder.Services.Configure<ProducerOptions>("PassengerNotifications", builder.Configuration.GetSection("Kafka:Producers:PassengerNotifications"));
builder.Services.Configure<ConsumerOptions>("TaskProcessing", builder.Configuration.GetSection("Kafka:Consumers:TaskProcessing"));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services
    .AddMigration()
    .AddInfrastructureDataAccess()
    .AddApplication();

builder.Services.AddHostedService<MigrationService>();

builder.Services.AddGrpc(o => o.Interceptors.Add<ExceptionInterceptor>());

builder.Services.AddHandlers();
builder.Services.AddKafkaConsumer<TaskProcessingKey, TaskProcessingValue>("TaskProcessing");
builder.Services.AddKafkaProducer<FlightCreationKey, FlightCreationValue>("FlightCreation");
builder.Services.AddKafkaProducer<PassengerNotificationsKey, PassengerNotificationsValue>("PassengerNotifications");
WebApplication app = builder.Build();

app.MapGrpcService<FlightController>();
app.Run();