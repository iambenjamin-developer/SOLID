# SolidShop API

API de e-commerce simple en **.NET 10** para ver los principios **SOLID** aplicados en código real: productos, pedidos, medios de pago y un job en segundo plano. Documentación interactiva con **Scalar**.

Un solo proyecto, organizado por carpetas. Sin capas de más, sin MediatR, sin AutoMapper.

## Cómo correrlo

**Requisitos:** SDK de .NET 10 y Visual Studio 2026 (o `dotnet` por consola / Rider / VS Code).

1. Descomprimí el zip y abrí `SolidShop.sln`.
2. Esperá a que Visual Studio restaure los paquetes NuGet.
3. Presioná **F5** (perfil `https`). Se abre el navegador en `https://localhost:7080/scalar`.

Por consola:

```bash
cd SolidShop.Api
dotnet run --launch-profile https
```

La base de datos SQLite (`solidshop.db`) se crea sola al arrancar, con 4 productos de ejemplo. Para empezar de cero, borrá ese archivo.

También podés probar los endpoints desde Visual Studio con el archivo `SolidShop.Api.http`.

## Estructura

```
SolidShop.Api/
├── Program.cs                     -> arranque, queda cortito
├── DependencyInjection.cs         -> registro de servicios y lifetimes
├── Common/
│   ├── Errors/GlobalExceptionHandler.cs
│   ├── Exceptions/AppExceptions.cs
│   ├── Extensions/StringExtensions.cs
│   └── PagedResult.cs
├── Domain/                        -> Product, Order (entidades con sus reglas)
├── Data/                          -> DbContext y repositorios
├── Features/
│   ├── Products/                  -> catálogo, admin, stock
│   ├── Orders/                    -> crear y cancelar pedidos
│   └── Payments/                  -> Mercado Pago, Stripe, contra entrega (simulados)
└── BackgroundJobs/ErpStockSyncJob.cs
```

## Dónde está cada principio

**S - Responsabilidad Única.** Mirá `AdminProductsController`: solo recibe el request y devuelve el status code. La validación está en `ProductValidators.cs`, las reglas de negocio en `ProductAdminService`, el acceso a datos en `ProductRepository`, el mapeo a DTO en `ProductMappings` y la traducción de errores a HTTP en `GlobalExceptionHandler`. Ningún controller tiene `try/catch`.

**O - Abierto/Cerrado.** Dos ejemplos. Primero, `StringExtensions` (`ToSlug`, `Truncate`, `NormalizeSku`) extiende `string` sin modificarlo; lo mismo hace `DependencyInjection.cs` con `IServiceCollection`. Segundo, los medios de pago: `PaymentService` recibe todos los `IPaymentProvider` registrados y elige por nombre. Para agregar uno nuevo (por ejemplo MODO o transferencia) se crea una clase y se agrega una línea en `DependencyInjection.cs`. `PaymentService` no se toca.

**L - Sustitución de Liskov.** `CashOnDeliveryProvider` no puede reembolsar, así que no implementa `IRefundablePaymentProvider`. Si la interfaz base tuviera `RefundAsync`, el pago contra entrega tendría que lanzar `NotSupportedException` y cancelar un pedido daría un 500 en producción. Mirá `PaymentService.TryRefundAsync` y `OrderService.CancelAsync`.

**I - Segregación de Interfaces.** En vez de un `IProductService` gigante hay tres: `IProductCatalogService` (solo lectura, lo usa la tienda pública), `IProductAdminService` (alta, baja, modificación) e `IStockService` (lo usan el admin y el job del ERP). Cada consumidor pide solo lo que necesita.

**D - Inversión de Dependencias.** Todos los servicios reciben interfaces por constructor. `ProductAdminService` no sabe que la base es SQLite, no sabe cómo se envían las notificaciones (`IProductNotifier`) y no usa `DateTime.UtcNow` sino `TimeProvider`, que en tests se reemplaza por un reloj falso.

## Lifetimes (en `DependencyInjection.cs`)

| Lifetime | Qué se registra así | Por qué |
|---|---|---|
| **Scoped** | `DbContext`, repositorios, servicios de negocio | Una instancia por request. Los dos repositorios comparten el mismo `DbContext`, por eso un solo `SaveChangesAsync` guarda el pedido y el stock juntos |
| **Transient** | Proveedores de pago, `IProductNotifier` | No tienen estado y son baratos de crear |
| **Singleton** | `TimeProvider`, el `BackgroundService` | Uno solo para toda la app, tienen que ser thread-safe |

El caso de la **dependencia cautiva** está en `ErpStockSyncJob`: como es singleton no puede recibir `IStockService` (scoped) por constructor, así que usa `IServiceScopeFactory` y crea un scope en cada ejecución.

## Cosas para probar

1. `GET /api/payments/methods`: fijate cuáles tienen `supportsRefunds: true`.
2. Creá un pedido con `mercadopago` y cancelalo: estado `Cancelled`, `refunded: true`.
3. Creá un pedido con `cash-on-delivery` y cancelalo: estado `CancelledPendingManualRefund`, `refunded: false`. Ese es el ejemplo de Liskov.
4. Pedí más stock del que hay (por ejemplo 50 mochilas): 400 con el mensaje de stock insuficiente, y el stock no cambia.
5. Creá un producto con datos inválidos: 400 con los errores por campo.
6. Creá un producto con un SKU existente (`REM-DRY-001`): 409.
7. La "Botella Térmica 750ml" arranca sin stock. Esperá un minuto y mirá la consola: el job del ERP la repone.
8. Pagos contra entrega de más de 300.000 se rechazan.

Los medios de pago y el ERP son **simulados**: no llaman a ninguna API externa.
