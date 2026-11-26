USE VeterinariaS14DB;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Correo varchar(254) NOT NULL,
        ClaveHash varchar(100) NOT NULL,
        Rol varchar(30) NOT NULL,
        CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo),
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('Veterinario', 'Recepcionista'))
    );
END;

-- Cuentas de demostración: ambas claves están almacenadas con BCrypt.
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo = 'veterinario@vet.local')
    INSERT dbo.Usuarios (Correo, ClaveHash, Rol)
    VALUES ('veterinario@vet.local', '$2a$10$5ntsPXp9e/HHpKM8RYG1CuBJE.kaVgj0oGZbfXecW8Y1.Pe3UnGn6', 'Veterinario');

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo = 'recepcion@vet.local')
    INSERT dbo.Usuarios (Correo, ClaveHash, Rol)
    VALUES ('recepcion@vet.local', '$2a$10$iSnrXB9vsCKVpd7RfaFcf.wa4zPv1KhXsJPPfRC49i1RnN094HXmK', 'Recepcionista');

COMMIT;
GO
