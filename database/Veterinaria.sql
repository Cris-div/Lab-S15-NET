-- SQL Server. Base independiente del laboratorio S12. No elimina bases existentes.
USE master;
GO
IF DB_ID(N'VeterinariaS14DB') IS NULL CREATE DATABASE VeterinariaS14DB;
GO
USE VeterinariaS14DB;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.Propietarios', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.Mascotas', N'U') IS NOT NULL
    THROW 50001, 'Las tablas ya existen. No se sobrescriben datos.', 1;

CREATE TABLE dbo.Propietarios (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Propietarios PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    Documento varchar(8) NOT NULL,
    Telefono varchar(15) NOT NULL,
    CONSTRAINT UQ_Propietarios_Documento UNIQUE (Documento),
    CONSTRAINT CK_Propietarios_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
    CONSTRAINT CK_Propietarios_Documento CHECK (LEN(Documento) = 8 AND Documento NOT LIKE '%[^0-9]%')
);
CREATE TABLE dbo.Mascotas (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mascotas PRIMARY KEY,
    Nombre nvarchar(80) NOT NULL,
    Especie nvarchar(40) NOT NULL,
    Edad int NOT NULL,
    Peso decimal(7,2) NOT NULL,
    PropietarioId int NOT NULL,
    CONSTRAINT FK_Mascotas_Propietarios FOREIGN KEY (PropietarioId) REFERENCES dbo.Propietarios(Id),
    CONSTRAINT CK_Mascotas_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
    CONSTRAINT CK_Mascotas_Especie CHECK (LEN(LTRIM(RTRIM(Especie))) > 0),
    CONSTRAINT CK_Mascotas_Edad CHECK (Edad BETWEEN 0 AND 50),
    CONSTRAINT CK_Mascotas_Peso CHECK (Peso BETWEEN 0.01 AND 1000)
);
CREATE INDEX IX_Mascotas_PropietarioId ON dbo.Mascotas(PropietarioId);
INSERT dbo.Propietarios (Nombre, Documento, Telefono) VALUES
(N'Ana Torres', '12345678', '987654321'),
(N'Luis Ramos', '87654321', '912345678'),
(N'Carla Flores', '45678912', '956123456');
INSERT dbo.Mascotas (Nombre, Especie, Edad, Peso, PropietarioId) VALUES
(N'Luna', N'Perro', 3, 12.50, 1),
(N'Milo', N'Gato', 2, 4.20, 1),
(N'Rocky', N'Perro', 5, 22.80, 2);
COMMIT;
GO
