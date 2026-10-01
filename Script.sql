IF DB_ID('BibliotecaDB') IS NULL
    CREATE DATABASE BibliotecaDB;
GO

USE BibliotecaDB;
GO

IF OBJECT_ID('dbo.DetallePrestamo', 'U') IS NOT NULL DROP TABLE dbo.DetallePrestamo;
IF OBJECT_ID('dbo.Prestamos',       'U') IS NOT NULL DROP TABLE dbo.Prestamos;
IF OBJECT_ID('dbo.Libros',          'U') IS NOT NULL DROP TABLE dbo.Libros;
IF OBJECT_ID('dbo.Socios',          'U') IS NOT NULL DROP TABLE dbo.Socios;
IF OBJECT_ID('dbo.Autores',         'U') IS NOT NULL DROP TABLE dbo.Autores;
GO

/* ====================== TABLAS ====================== */

CREATE TABLE dbo.Autores (
    AutorId       INT IDENTITY(1,1) NOT NULL,
    Nombre        NVARCHAR(100)     NOT NULL,
    Nacionalidad  NVARCHAR(50)      NULL,
    Activo        BIT               NOT NULL CONSTRAINT DF_Autores_Activo DEFAULT 1,
    CONSTRAINT PK_Autores PRIMARY KEY (AutorId)
);
GO

CREATE TABLE dbo.Libros (
    LibroId     INT IDENTITY(1,1) NOT NULL,
    Titulo      NVARCHAR(200)     NOT NULL,
    ISBN        VARCHAR(20)       NOT NULL,
    AutorId     INT               NOT NULL,
    Ejemplares  INT               NOT NULL CONSTRAINT DF_Libros_Ejemplares DEFAULT 1,
    Activo      BIT               NOT NULL CONSTRAINT DF_Libros_Activo DEFAULT 1,
    CONSTRAINT PK_Libros        PRIMARY KEY (LibroId),
    CONSTRAINT UQ_Libros_ISBN   UNIQUE (ISBN),
    CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES dbo.Autores (AutorId),
    CONSTRAINT CK_Libros_Ejemplares CHECK (Ejemplares >= 0)
);
GO

CREATE TABLE dbo.Socios (
    SocioId  INT IDENTITY(1,1) NOT NULL,
    DNI      CHAR(8)           NOT NULL,
    Nombre   NVARCHAR(100)     NOT NULL,
    Email    NVARCHAR(100)     NULL,
    Activo   BIT               NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT 1,
    CONSTRAINT PK_Socios     PRIMARY KEY (SocioId),
    CONSTRAINT UQ_Socios_DNI UNIQUE (DNI)
);
GO

CREATE TABLE dbo.Prestamos (
    PrestamoId     INT IDENTITY(1,1) NOT NULL,
    SocioId        INT               NOT NULL,
    FechaPrestamo  DATE              NOT NULL,
    FechaLimite    DATE              NOT NULL,
    Estado         VARCHAR(20)       NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT 'Pendiente',
    CONSTRAINT PK_Prestamos        PRIMARY KEY (PrestamoId),
    CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES dbo.Socios (SocioId),
    CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN ('Pendiente', 'Devuelto')),
    CONSTRAINT CK_Prestamos_Fechas CHECK (FechaLimite >= FechaPrestamo)
);
GO

CREATE TABLE dbo.DetallePrestamo (
    PrestamoId       INT  NOT NULL,
    LibroId          INT  NOT NULL,
    FechaDevolucion  DATE NULL,  
    CONSTRAINT PK_DetallePrestamo          PRIMARY KEY (PrestamoId, LibroId),
    CONSTRAINT FK_DetallePrestamo_Prestamo FOREIGN KEY (PrestamoId) REFERENCES dbo.Prestamos (PrestamoId),
    CONSTRAINT FK_DetallePrestamo_Libro    FOREIGN KEY (LibroId)    REFERENCES dbo.Libros (LibroId)
);
GO

/* ====================== DATOS DE PRUEBA ====================== */

/* ---- 8 autores ---- */
INSERT INTO dbo.Autores (Nombre, Nacionalidad) VALUES
(N'Gabriel García Márquez', N'Colombiana'),   -- 1
(N'Mario Vargas Llosa',     N'Peruana'),      -- 2
(N'Isabel Allende',         N'Chilena'),      -- 3
(N'Julio Cortázar',         N'Argentina'),    -- 4
(N'Jorge Luis Borges',      N'Argentina'),    -- 5
(N'Ricardo Palma',          N'Peruana'),      -- 6
(N'César Vallejo',          N'Peruana'),      -- 7
(N'Laura Esquivel',         N'Mexicana');     -- 8
GO

/* ---- 20 libros ---- */
INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares) VALUES
(N'Cien años de soledad',                 '9780000000011', 1, 3),  -- 1
(N'El amor en los tiempos del cólera',    '9780000000028', 1, 2),  -- 2
(N'Crónica de una muerte anunciada',      '9780000000035', 1, 2),  -- 3
(N'La ciudad y los perros',               '9780000000042', 2, 3),  -- 4
(N'Conversación en La Catedral',          '9780000000059', 2, 2),  -- 5
(N'La fiesta del Chivo',                  '9780000000066', 2, 3),  -- 6
(N'La casa de los espíritus',             '9780000000073', 3, 4),  -- 7
(N'Paula',                                '9780000000080', 3, 2),  -- 8
(N'Rayuela',                              '9780000000097', 4, 2),  -- 9
(N'Bestiario',                            '9780000000103', 4, 3),  -- 10
(N'Ficciones',                            '9780000000110', 5, 4),  -- 11
(N'El Aleph',                             '9780000000127', 5, 3),  -- 12
(N'Tradiciones peruanas',                 '9780000000134', 6, 5),  -- 13
(N'Los heraldos negros',                  '9780000000141', 7, 2),  -- 14
(N'Trilce',                               '9780000000158', 7, 3),  -- 15
(N'Como agua para chocolate',             '9780000000165', 8, 4),  -- 16
(N'Eva Luna',                             '9780000000172', 3, 2),  -- 17
(N'Los cachorros',                        '9780000000189', 2, 3),  -- 18
(N'El libro de arena',                    '9780000000196', 5, 2),  -- 19
(N'Del amor y otros demonios',            '9780000000202', 1, 0);  -- 20 (sin ejemplares: prueba de regla)
GO

/* ---- 10 socios ---- */
INSERT INTO dbo.Socios (DNI, Nombre, Email) VALUES
('70123456', N'Carlos Mendoza Rojas',     'carlos.mendoza@correo.com'),   -- 1 (3 libros pendientes)
('71234567', N'María Fernanda Quispe',    'maria.quispe@correo.com'),     -- 2
('72345678', N'Luis Alberto Torres',      'luis.torres@correo.com'),      -- 3
('73456789', N'Ana Lucía Paredes',        'ana.paredes@correo.com'),      -- 4
('74567890', N'Jorge Ramírez Soto',       'jorge.ramirez@correo.com'),    -- 5
('75678901', N'Patricia Huamán Díaz',     'patricia.huaman@correo.com'),  -- 6
('76789012', N'Diego Salazar Vega',       'diego.salazar@correo.com'),    -- 7
('77890123', N'Rosa Elena Campos',        'rosa.campos@correo.com'),      -- 8
('78901234', N'Miguel Ángel Flores',      'miguel.flores@correo.com'),    -- 9
('79012345', N'Sofía Cárdenas Luna',      'sofia.cardenas@correo.com');   -- 10
GO

/* ---- 5 préstamos ---- */
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES
(1, '20260920', '20260927', 'Pendiente'),  -- 1: Carlos, 3 libros pendientes (vencido: sirve para probar multa)
(2, '20260905', '20260912', 'Pendiente'),  -- 2: María, 1 devuelto y 1 pendiente
(3, '20260901', '20260908', 'Devuelto'),   -- 3: Luis, devuelto
(4, '20260910', '20260917', 'Devuelto'),   -- 4: Ana, 2 libros devueltos
(5, '20260928', '20261005', 'Pendiente');  -- 5: Jorge, 1 pendiente (aún en plazo)
GO

/* ---- Detalle de préstamos (FechaDevolucion NULL = pendiente) ---- */
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(1, 1, NULL),          -- Cien años de soledad
(1, 2, NULL),          -- El amor en los tiempos del cólera
(1, 3, NULL),          -- Crónica de una muerte anunciada
(2, 4, '20260910'),    -- La ciudad y los perros (devuelto)
(2, 5, NULL),          -- Conversación en La Catedral (pendiente)
(3, 6, '20260907'),    -- La fiesta del Chivo (devuelto a tiempo)
(4, 7, '20260915'),    -- La casa de los espíritus (devuelto)
(4, 8, '20260919'),    -- Paula (devuelto con 2 días de retraso)
(5, 9, NULL);          -- Rayuela
GO

/* ---- Descontar del stock los ejemplares que siguen prestados ----
   (los libros ya devueltos volvieron al stock, por eso no se descuentan) */
UPDATE l
SET    l.Ejemplares = l.Ejemplares - p.Pendientes
FROM   dbo.Libros AS l
INNER JOIN (
        SELECT LibroId, COUNT(*) AS Pendientes
        FROM   dbo.DetallePrestamo
        WHERE  FechaDevolucion IS NULL
        GROUP BY LibroId
) AS p ON p.LibroId = l.LibroId;
GO

/* ====================== VERIFICACIÓN ====================== */

-- Conteo de registros (esperado: 8, 20, 10, 5, 9 filas de detalle)
SELECT 'Autores' AS Tabla, COUNT(*) AS Total FROM dbo.Autores
UNION ALL SELECT 'Libros',          COUNT(*) FROM dbo.Libros
UNION ALL SELECT 'Socios',          COUNT(*) FROM dbo.Socios
UNION ALL SELECT 'Prestamos',       COUNT(*) FROM dbo.Prestamos
UNION ALL SELECT 'DetallePrestamo', COUNT(*) FROM dbo.DetallePrestamo;
GO

-- Libros pendientes por socio (Carlos Mendoza debe tener 3)
SELECT s.SocioId, s.Nombre, COUNT(*) AS LibrosPendientes
FROM   dbo.Socios s
INNER JOIN dbo.Prestamos p        ON p.SocioId = s.SocioId
INNER JOIN dbo.DetallePrestamo d  ON d.PrestamoId = p.PrestamoId
WHERE  d.FechaDevolucion IS NULL
GROUP BY s.SocioId, s.Nombre
ORDER BY LibrosPendientes DESC;
GO

-- Reporte de préstamos por rango de fechas (INNER JOIN de las 4 tablas)
SELECT p.PrestamoId, s.Nombre AS Socio, l.Titulo AS Libro,
       p.FechaPrestamo, p.FechaLimite, p.Estado
FROM   dbo.Prestamos p
INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
INNER JOIN dbo.Libros l          ON l.LibroId    = d.LibroId
INNER JOIN dbo.Socios s          ON s.SocioId    = p.SocioId
WHERE  p.FechaPrestamo BETWEEN '20260901' AND '20260930'
ORDER BY p.FechaPrestamo, p.PrestamoId;

GO