# POI

## Setup

### Migrations
Create a new migration to update the database schema with your new changes:

```bash
dotnet ef migrations add "[New Migration Name]" --project POI.Persistence.EFCore.Npgsql --verbose
```

Apply all generated migrations on your database:

```bash
dotnet ef database update --project POI.Persistence.EFCore.Npgsql --verbose -- "[Connection String]"
```

Find all migrations in the `POI.Persistence.EFCore.Npgsql/Migrations` folder.

### Environment Variables

| Variable                       | Description                                                                                                                                        | Example                                                                             |
|--------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------|
| `ConnectionStrings:PostgreSQL` | Connection string for **PostgreSQL** database ([tutorial](https://www.commandprompt.com/education/how-to-create-a-postgresql-database-in-docker/)) | `Host=localhost;Port=5432;Database=poi_test_db;Username=postgres;Password=password` |
| `Discord:Prefix`               | Prefix for bot chat commands                                                                                                                       | `poi`                                                                               |
| `Discord:Token`                | Token for your Discord bot ([tutorial](https://www.writebots.com/discord-bot-token/))                                                              | `xxxx.xxxx.xxxx`                                                                    |
| `Paths:DataFolderPath`         | Path to folder where data files like image assets and logs can be stored                                                                           | `C:\your-path\POI\Data`                                                             |
