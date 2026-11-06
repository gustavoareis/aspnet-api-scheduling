using aspnet_api_scheduling.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace aspnet_api_scheduling.Data;

public sealed class DatabaseSchemaInitializer(AppDbContext dbContext)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await TableExistsAsync("Clientes", cancellationToken))
        {
            await UpgradeExistingDatabaseAsync(cancellationToken);
        }
    }

    private async Task UpgradeExistingDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (!await TableExistsAsync("Barbearias", cancellationToken))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE Barbearias (
                    Id INTEGER NOT NULL CONSTRAINT PK_Barbearias PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    Slug TEXT NOT NULL,
                    Ativa INTEGER NOT NULL
                );
                INSERT INTO Barbearias (Id, Nome, Slug, Ativa)
                VALUES (1, 'Barbearia de demonstração', 'barbearia-demonstracao', 1);
                """,
                cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        }

        foreach (var table in new[] { "Clientes", "Barbeiros", "Servicos", "Agendamentos" })
        {
            await AddColumnIfMissingAsync(table, "BarbeariaId", "INTEGER NOT NULL DEFAULT 1", cancellationToken);
        }

        await AddColumnIfMissingAsync(
            "Agendamentos", "ServicoNome", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await AddColumnIfMissingAsync(
            "Agendamentos", "Preco", "TEXT NOT NULL DEFAULT '0'", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            UPDATE Agendamentos
            SET ServicoNome = (SELECT Nome FROM Servicos WHERE Servicos.Id = Agendamentos.ServicoId),
                Preco = (SELECT Preco FROM Servicos WHERE Servicos.Id = Agendamentos.ServicoId)
            WHERE ServicoNome = '';

            DROP INDEX IF EXISTS IX_Clientes_Email;
            CREATE UNIQUE INDEX IF NOT EXISTS IX_Clientes_BarbeariaId_Email
                ON Clientes (BarbeariaId, Email COLLATE NOCASE);
            CREATE INDEX IF NOT EXISTS IX_Barbeiros_BarbeariaId_Ativo_Nome
                ON Barbeiros (BarbeariaId, Ativo, Nome);
            CREATE INDEX IF NOT EXISTS IX_Servicos_BarbeariaId_Ativo_Nome
                ON Servicos (BarbeariaId, Ativo, Nome);
            CREATE INDEX IF NOT EXISTS IX_Agendamentos_BarbeariaId_BarbeiroId_DataHoraInicio
                ON Agendamentos (BarbeariaId, BarbeiroId, DataHoraInicio);
            CREATE TABLE IF NOT EXISTS Produtos (
                Id INTEGER NOT NULL CONSTRAINT PK_Produtos PRIMARY KEY AUTOINCREMENT,
                BarbeariaId INTEGER NOT NULL,
                Nome TEXT NOT NULL,
                Descricao TEXT NULL,
                Preco TEXT NOT NULL,
                Ativo INTEGER NOT NULL,
                CONSTRAINT FK_Produtos_Barbearias_BarbeariaId
                    FOREIGN KEY (BarbeariaId) REFERENCES Barbearias (Id) ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS IX_Produtos_BarbeariaId_Ativo_Nome
                ON Produtos (BarbeariaId, Ativo, Nome);
            CREATE TABLE IF NOT EXISTS UsuariosBarbearias (
                UsuarioId TEXT NOT NULL,
                BarbeariaId INTEGER NOT NULL,
                Papel TEXT NOT NULL,
                CONSTRAINT PK_UsuariosBarbearias PRIMARY KEY (UsuarioId, BarbeariaId),
                CONSTRAINT FK_UsuariosBarbearias_AspNetUsers_UsuarioId
                    FOREIGN KEY (UsuarioId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
                CONSTRAINT FK_UsuariosBarbearias_Barbearias_BarbeariaId
                    FOREIGN KEY (BarbeariaId) REFERENCES Barbearias (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_UsuariosBarbearias_BarbeariaId
                ON UsuariosBarbearias (BarbeariaId);
            CREATE TABLE IF NOT EXISTS AspNetRoles (
                Id TEXT NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY,
                Name TEXT NULL,
                NormalizedName TEXT NULL,
                ConcurrencyStamp TEXT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS RoleNameIndex ON AspNetRoles (NormalizedName);
            CREATE TABLE IF NOT EXISTS AspNetUsers (
                Id TEXT NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY,
                UserName TEXT NULL,
                NormalizedUserName TEXT NULL,
                Email TEXT NULL,
                NormalizedEmail TEXT NULL,
                EmailConfirmed INTEGER NOT NULL,
                PasswordHash TEXT NULL,
                SecurityStamp TEXT NULL,
                ConcurrencyStamp TEXT NULL,
                PhoneNumber TEXT NULL,
                PhoneNumberConfirmed INTEGER NOT NULL,
                TwoFactorEnabled INTEGER NOT NULL,
                LockoutEnd TEXT NULL,
                LockoutEnabled INTEGER NOT NULL,
                AccessFailedCount INTEGER NOT NULL
            );
            CREATE INDEX IF NOT EXISTS EmailIndex ON AspNetUsers (NormalizedEmail);
            CREATE UNIQUE INDEX IF NOT EXISTS UserNameIndex ON AspNetUsers (NormalizedUserName);
            CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
                Id INTEGER NOT NULL CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY AUTOINCREMENT,
                RoleId TEXT NOT NULL,
                ClaimType TEXT NULL,
                ClaimValue TEXT NULL,
                CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId
                    FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId);
            CREATE TABLE IF NOT EXISTS AspNetUserClaims (
                Id INTEGER NOT NULL CONSTRAINT PK_AspNetUserClaims PRIMARY KEY AUTOINCREMENT,
                UserId TEXT NOT NULL,
                ClaimType TEXT NULL,
                ClaimValue TEXT NULL,
                CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId
                    FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId);
            CREATE TABLE IF NOT EXISTS AspNetUserLogins (
                LoginProvider TEXT NOT NULL,
                ProviderKey TEXT NOT NULL,
                ProviderDisplayName TEXT NULL,
                UserId TEXT NOT NULL,
                CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
                CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId
                    FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId);
            CREATE TABLE IF NOT EXISTS AspNetUserRoles (
                UserId TEXT NOT NULL,
                RoleId TEXT NOT NULL,
                CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
                CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId
                    FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
                CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId
                    FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId);
            CREATE TABLE IF NOT EXISTS AspNetUserTokens (
                UserId TEXT NOT NULL,
                LoginProvider TEXT NOT NULL,
                Name TEXT NOT NULL,
                Value TEXT NULL,
                CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
                CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId
                    FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_Barbearias_Slug ON Barbearias (Slug);
            """,
            cancellationToken);
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        var result = await dbContext.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'table' AND name = {0}",
                tableName)
            .SingleAsync(cancellationToken);
        return result > 0;
    }

    private async Task AddColumnIfMissingAsync(
        string tableName,
        string columnName,
        string columnDefinition,
        CancellationToken cancellationToken)
    {
        if (!new[] { "Clientes", "Barbeiros", "Servicos", "Agendamentos" }.Contains(tableName, StringComparer.Ordinal) ||
            !new[] { "BarbeariaId", "ServicoNome", "Preco" }.Contains(columnName, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Tentativa de atualizar uma coluna fora do esquema permitido.");
        }

        var columns = await dbContext.Database.SqlQuery<string>(
                $"SELECT name AS Value FROM pragma_table_info({tableName})")
            .ToListAsync(cancellationToken);
        if (columns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var connection = dbContext.Database.GetDbConnection();
        var closeConnection = connection.State != ConnectionState.Open;
        if (closeConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{columnName}\" {columnDefinition}";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (closeConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}
