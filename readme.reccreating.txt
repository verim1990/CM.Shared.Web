1.1) Prerequisites

install-package Autofac.Extensions.DependencyInjection
install-package FluentValidation
install-package FluentValidation.AspNetCore
install-package MediatR
install-package MediatR.Extensions.Microsoft.DependencyInjection
install-package Microsoft.AspNetCore.App -version 2.1.1
install-package Microsoft.Extensions.Http.Polly
install-package Npgsql.EntityFrameworkCore.PostgreSQL
install-package Polly
Install-Package Sieve
install-package Swashbuckle.AspNetCore.SwaggerUI
install-package Swashbuckle.AspNetCore.SwaggerGen

1.2) References

CM.Shared.Kernel

2) Recreate microservice

2.1) dotnet new classlib -o CM.Shared.Kernel -f netcoreapp2.1
2.2) Move
	- all folder and files
	
3) Publishing library
3.1) dotnet pack
3.2) dotnet nuget push bin/Debug/CM.Shared.Web.1.0.0.nupkg -k 96de8bc7-fbf0-3ca9-9587-e57e07d18441 -s https://nexus.wojciechdudek.pl/repository/nuget-hosted/