using DotNetEnv;

// Env.Load();

string? myVar = Environment.GetEnvironmentVariable("GREETINGS");

Console.WriteLine(myVar);
