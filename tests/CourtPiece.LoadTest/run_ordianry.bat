dotnet build ..\..\src\CourtPiece.WebApi\CourtPiece.WebApi.csproj
START "" dotnet run --no-build  --project ..\..\src\CourtPiece.WebApi\CourtPiece.WebApi.csproj
set /p stuff=What stuff do you want to do?:
k6 run -e ApiVersion=1 .\main.js
ren report.html report_ordinary.html