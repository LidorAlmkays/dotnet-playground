var builder = DistributedApplication.CreateBuilder(args);
var projectDir = AppContext.BaseDirectory;
var postgresVolumeDir = Path.GetFullPath(Path.Combine(projectDir, "..", "..", "..", "..", "..", "volumes", "postgres"));
var redisVolumeDir = Path.GetFullPath(Path.Combine(projectDir, "..", "..", "..", "..", "..", "volumes", "redis"));

var postgres = builder.AddPostgres(
    name: "postgres",
    userName: builder.AddParameter("postgres-user", "postgres"),
    password: builder.AddParameter("postgres-password", "postgres"),
    port: 5432
).WithBindMount(source: postgresVolumeDir, target: "/var/lib/postgresql/data");

var cache = builder.AddRedis(
    name: "redis",
    port: 6379,
    password: builder.AddParameter("redis-password", "redis_password")
    ).WithBindMount(source: redisVolumeDir, target: "/data");

var authService = builder.AddProject<Projects.AuthService>("AuthService")
                .WithReference(postgres)
                .WithReference(cache)
                .WaitFor(postgres)
                .WaitFor(cache);

builder.AddProject<Projects.Gateway>("reverseproxy")
    .WithReference(authService);

builder.Build().Run();
