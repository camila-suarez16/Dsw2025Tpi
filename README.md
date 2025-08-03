# Trabajo Práctico Integrador
## Desarrollo de Software
### Backend

## Introducción
Se desea desarrollar una plataforma de comercio electrónico (E-commerce). 
En esta primera etapa el objetivo es construir el módulo de Órdenes, permitiendo la gestión completa de éstas.

## Visión General del Producto
Del relevamiento preliminar se identificaron los siguientes requisitos:
- Los visitantes pueden consultar los productos sin necesidad de estar registrados o iniciar sesión.
- Para realizar un pedido se requiere el inicio de sesión.
- Una orden, para ser aceptada, debe incluir la información básica del cliente, envío y facturación.
- Antes de registrar la orden se debe verificar la disponibilidad de stock (o existencias) de los productos.
- Si la orden es exitosa hay que actualizar el stock de cada producto.
- Se deben poder consultar órdenes individuales o listar varias con posibilidad de filtrado.
- Será necesario el cambio de estado de una orden a medida que avanza en su ciclo de vida.
- Los administradores solo pueden gestionar los productos (alta, modificación y baja) y actualizar el estado de la orden.
- Los clientes pueden crear y consultar órdenes.

[Documento completo](https://frtutneduar.sharepoint.com/:b:/s/DSW2025/ETueAd4rTe1Gilj_Yfi64RYB5oz9s2dOamxKSfMFPREbiA?e=azZcwg) 

## Alcance para el Primer Parcial
> [!IMPORTANT]
> Del apartado `IMPLEMENTACIÓN` (Pag. 7), completo hasta el punto `6` (inclusive)


### Características de la Solución

- Lenguaje: C# 12.0
- Plataforma: .NET 8

# Desarrollo de Software TPI 
Integrante: Camila Rosario Suarez – 58143 - 3K3

# Requisitos
•	.NET SDK 8 
•	SQL Server LocalDB o SQL Server Developer
•	Git

# Configuración 
1.	Clonar el repo: https://github.com/camila-suarez16/Dsw2025Tpi.git
   
2.	(opcional) Verificar la cadena de conexión
"ConnectionStrings": {
  "Dsw2025TpiEntities": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Dsw2025TpiDb;Integrated Security=True;"
}
Si se usa LocalDB → No es necesario modificar nada.
Si se usa otra instancia/servidor → Reemplazar Data Source y/o Initial Catalog con la cadena propia.

3.	Restaurar dependencias
•	Si se trabaja desde Visual Studio, el IDE ejecuta “Restore” automáticamente.
•	Sino:
-	Abrir una terminal (PowerShell, CMD o Bash).
-	Posicionarse en la carpeta raíz del repositorio, donde está el archivo de solución Dsw2025Tpi.sln.
-	Ejecutar: dotnet restore
Esta herramienta lee la solución y cada proyecto, descarga los paquetes NuGet que falten y los guarda en la caché global.

4.	Crear/actualizar la base de datos con EF Core
El proyecto usa dos DbContext: Dsw2025TpiContext (dominio) y AuthenticateContext (Identity).
Ejecutar (desde la raíz de la solución):
#Contexto de dominio
dotnet ef database update -p Dsw2025Tpi.Data -s Dsw2025Tpi.Api -c Dsw2025TpiContext
#Contexto de Identity
dotnet ef database update -p Dsw2025Tpi.Data -s Dsw2025Tpi.Api -c AuthenticateContext

  En el arranque, la app:
- Crea roles Admin y Customer si no existen.
- Carga Products y Customers desde Sources/products.json y Sources/customers.json (si las tablas están vacías).
  
# Endpoints principales
Auth:
1.	POST /api/auth/register – Registrar usuario
{
  "username": "camila",
  "email": "camila@gmail.com",
  "password": "Camila123!",
  "role": "Customer" o "Admin"
}
•	200 OK: “Usuario registrado correctamente.”
•	400 BadRequest: datos inválidos / contraseña no cumple reglas.

2.	POST /api/auth/login – Login y obtención de token
{
  "username": "camila",
  "password": "Camila123!"
}
•	200 OK:
{ "token": "eyJhbGciOi..." }
•	401 Unauthorized: credenciales inválidas.

Products:
1.	POST /api/products (Admin) – Crear producto
{
  "sku": "SKU-1001",
  "internalCode": "INT-01",
  "name": "Galletas oreo",
  "description": "Paquete de galletas oreos de 118 g",
  "currentUnitPrice": 1800.00,
  "stockQuantity": 10
}
•	201 Created + Location /api/products/{id}
•	400 datos inválidos | 409 SKU duplicado

2.	GET /api/products (Anónimo) – Listar productos activos
•	200 OK: [] o lista de productos
•	204 No Content: sin productos

3.	GET /api/products/{id} (Anónimo) – Producto por ID
•	200 OK | 404 NotFound

4.	PUT /api/products/{id} (Admin) – Actualizar producto
•	Body igual al de creación.
•	200 OK | 400/404/409

5.	PATCH /api/products/{id} (Admin) – Inhabilitar producto
•	204 No Content | 404

Orders:
1.	POST /api/orders (Customer) – Crear orden
{
  "customerId": "GUID-DEL-CLIENTE",
  "shippingAddress": "Av. Sarmiento 742",
  "billingAddress": "Av. Sarmiento 742",
  "orderItems": [
    { "productoId": "GUID-PROD-1", "quantity": 2 },
    { "productoId": "GUID-PROD-2", "quantity": 1 }
  ]
}
•	201 Created + detalle de la orden
•	400 datos inválidos / stock insuficiente
•	404 cliente o producto inexistente/inactivo

2.	GET /api/orders (Admin, Customer) – Listar ordenes
•	Parámetros opcionales de consulta:
o	status (ej: PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELLED)
o	customerId (GUID)
o	pageNumber (default 1), pageSize (default 10)
•	200 OK lista paginada 
•	204 si no hay resultados

3.	GET /api/orders/{id} (Admin, Customer) – Orden por ID
•	200 OK 
•	404 no encontrada

4.	PUT /api/orders/{id}/status (Admin) – Actualizar estado
•	Transiciones válidas:
o	PENDING → PROCESSING | CANCELLED
o	PROCESSING → SHIPPED | CANCELLED
o	SHIPPED → DELIVERED
o	DELIVERED y CANCELLED → no cambian
•	200 OK | 400 transición inválida | 404 orden no existe


