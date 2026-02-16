ef-add-migration:
	dotnet ef migrations add --project ../MyIndustry.Repository/MyIndustry.Repository.csproj --startup-project ../MyIndustry.Api/MyIndustry.Api.csproj --configuration Debug $(NAME) --output-dir Migrations

ef-remove-migration:
	dotnet ef migrations remove --project ./../Wallet.Core.Infrastructure/Wallet.Core.Infrastructure.csproj --startup-project Wallet.Core.Api.csproj --context Wallet.Core.Infrastructure.Persistence.EntityFrameworkCore.WalletDbContext --configuration Debug --force

ef-update-db: 
	dotnet ef database update