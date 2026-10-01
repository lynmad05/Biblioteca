# Biblioteca
# Sistema de Gestión de Biblioteca

Este repositorio contiene la solución en C# (.NET) para la gestión académica de una biblioteca. La aplicación implementa una arquitectura en capas, persistencia de datos mediante ADO.NET y SQL Server, e interfaz de usuario desarrollada en Windows Presentation Foundation (WPF).

## Integrantes

* **Medina Mallqui, Ailyn**
* **Ochoa Marin, Yamile**

## Características del Sistema

* **Gestión de Libros:** Registro, actualización, búsqueda por texto o ISBN y eliminación lógica de ejemplares.
* **Gestión de Socios:** Control de registros con validación de DNI y correo electrónico, estado activo/inactivo e historial de préstamos.
* **Préstamos y Devoluciones:** Procesamiento transaccional de préstamos múltiples con límite de libros pendientes y cálculo de multas por días de retraso.
* **Reportes:** Generación de listados detallados de préstamos y reportes por rango de fechas.

## Arquitectura de la Solución

El proyecto sigue una estructura desacoplada mediante una arquitectura en capas:

```text
Biblioteca/
├── Biblioteca.Entidades/   # Modelos de datos (Libro, Socio, Prestamo, Autor, etc.)
├── Biblioteca.Datos/       # Capa de acceso a datos mediante ADO.NET y SQL Server
├── Biblioteca.Negocio/     # Lógica de negocio, reglas del sistema y validaciones
└── Biblioteca.WPF/         # Interfaz gráfica de usuario y controladores XAML
```
## Reglas de Negocio Implementadas

1. **Límite de préstamos:** Un socio solo puede tener un máximo de 3 libros pendientes de devolución.
2. **Plazo de devolución:** Periodo estándar de 7 días naturales por préstamo.
3. **Multas:** Cobro automático por días de mora tras vencer la fecha límite.
4. **Eliminación Lógica:** Bloqueo de bajas para socios o libros con préstamos activos pendientes.

## Configuración e Instalación

### Base de Datos y Aplicación

1. **Base de Datos:** Ejecute el archivo script SQL incluido en este repositorio en su servidor de Microsoft SQL Server para crear la base de datos `BibliotecaDB` con sus respectivas tablas y registros.
2. **Cadena de Conexión:** Abra el archivo `App.config` (o la clase de configuración correspondiente en la capa de datos) y actualice la cadena de conexión (*ConnectionString*) indicando su servidor local de SQL Server.

## Información Académica

* **Docente:** Edwin William Arevalo Sermeño
* **Curso:** Desarrollo de Aplicaciones Empresariales Avanzado
