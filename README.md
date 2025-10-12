# CareNest Shop - Clean Architecture với CQRS Pattern

## 📋 Tổng quan

Dự án CareNest Shop được xây dựng theo kiến trúc **Clean Architecture** kết hợp với **CQRS (Command Query Responsibility Segregation)** pattern, sử dụng .NET 8 và PostgreSQL. Kiến trúc này đảm bảo tính maintainable, testable và scalable cho ứng dụng.

## 🏗️ Kiến trúc tổng thể

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Controllers   │  │   Middleware    │  │  Extensions  │ │
│  │                 │  │                 │  │              │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Commands      │  │     Queries     │  │   Handlers   │ │
│  │                 │  │                 │  │              │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Interfaces    │  │   Use Cases     │  │   Services   │ │
│  │                 │  │                 │  │              │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Entities      │  │   Repositories  │  │   Commons    │ │
│  │                 │  │                 │  │              │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Database      │  │   Repositories  │  │   Services   │ │
│  │   Context       │  │                 │  │              │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Cấu trúc thư mục

```
CareNest_Shop/
├── CareNest_Shop/                    # Presentation Layer
│   ├── Controllers/                  # API Controllers
│   ├── Middleware/                   # Custom Middleware
│   ├── Extensions/                   # Extension Methods
│   └── Program.cs                    # Application Entry Point
│
├── Shop.Application/                 # Application Layer
│   ├── Features/                     # CQRS Features
│   │   ├── Commands/                 # Command Objects
│   │   │   ├── Create/
│   │   │   ├── Update/
│   │   │   └── Delete/
│   │   └── Queries/                  # Query Objects
│   │       ├── GetAllPaging/
│   │       └── GetById/
│   ├── Interfaces/                   # Application Interfaces
│   │   ├── CQRS/                     # CQRS Interfaces
│   │   ├── Services/                 # Service Interfaces
│   │   └── UOW/                      # Unit of Work Interface
│   ├── UseCases/                     # Use Case Dispatcher
│   ├── Common/                       # Common DTOs
│   └── Exceptions/                   # Custom Exceptions
│
├── Shop.Domain/                      # Domain Layer
│   ├── Entitites/                    # Domain Entities
│   ├── Repositories/                 # Repository Interfaces
│   └── Commons/                      # Domain Commons
│       ├── BaseEntity.cs
│       ├── Constant/
│       └── Enum/
│
├── Shop.Infrastructure/              # Infrastructure Layer
│   ├── Persistences/                 # Data Persistence
│   │   ├── Database/                 # DbContext & Migrations
│   │   ├── Repository/               # Repository Implementations
│   │   └── Configuration/            # Database Configuration
│   ├── Services/                     # Infrastructure Services
│   └── UOW/                          # Unit of Work Implementation
│
└── Shared/                           # Shared Utilities
    └── Helper/                       # Helper Classes
```

## 🔧 Các thành phần chính

### 1. Presentation Layer (CareNest_Shop)

**Mục đích**: Xử lý HTTP requests, routing, và response formatting.

**Thành phần chính**:

- **Controllers**: API endpoints sử dụng CQRS dispatcher
- **Middleware**: Global exception handling
- **Extensions**: Response formatting extensions

**Ví dụ Controller**:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly IUseCaseDispatcher _dispatcher;

    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] CreateCommand command)
    {
        var shop = await _dispatcher.DispatchAsync<CreateCommand, Shop>(command);
        return this.OkResponse(shop, MessageConstant.SuccessCreate);
    }
}
```

### 2. Application Layer (Shop.Application)

**Mục đích**: Chứa business logic, use cases, và orchestration.

**CQRS Pattern**:

#### Commands (Write Operations)

```csharp
public class CreateCommand : ICommand<Shop>
{
    public string? OwnerId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Status Status { get; set; }
    public string? ImgUrl { get; set; }
    public string? WorkingDays { get; set; }
}
```

#### Queries (Read Operations)

```csharp
public class GetAllPagingQuery : IQuery<PageResult<ShopResponse>>
{
    public int Index { get; set; }
    public int PageSize { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
}
```

#### Use Case Dispatcher

```csharp
public interface IUseCaseDispatcher
{
    Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand;
    Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>;
    Task<TResult> DispatchQueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>;
}
```

### 3. Domain Layer (Shop.Domain)

**Mục đích**: Chứa business entities và domain logic.

**Base Entity**:

```csharp
public abstract class BaseEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeleteAt { get; set; }
}
```

**Domain Entity**:

```csharp
public class Shop : BaseEntity
{
    public required string OwnerId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Status Status { get; set; }
    public string? ImgUrl { get; set; }
    public string? WorkingDays { get; set; }
}
```

### 4. Infrastructure Layer (Shop.Infrastructure)

**Mục đích**: Xử lý data persistence, external services.

**Repository Pattern**:

```csharp
public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Entities { get; }
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    // ... more methods
}
```

**Unit of Work Pattern**:

```csharp
public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## 🚀 Cách triển khai kiến trúc này

### Bước 1: Tạo Solution Structure

```bash
# Tạo solution
dotnet new sln -n YourProjectName

# Tạo các project
dotnet new webapi -n YourProjectName.API
dotnet new classlib -n YourProjectName.Application
dotnet new classlib -n YourProjectName.Domain
dotnet new classlib -n YourProjectName.Infrastructure
dotnet new classlib -n YourProjectName.Shared

# Thêm projects vào solution
dotnet sln add YourProjectName.API/YourProjectName.API.csproj
dotnet sln add YourProjectName.Application/YourProjectName.Application.csproj
dotnet sln add YourProjectName.Domain/YourProjectName.Domain.csproj
dotnet sln add YourProjectName.Infrastructure/YourProjectName.Infrastructure.csproj
dotnet sln add YourProjectName.Shared/YourProjectName.Shared.csproj
```

### Bước 2: Cấu hình Dependencies

**API Project** → Application + Infrastructure
**Application** → Domain
**Infrastructure** → Application + Domain

### Bước 3: Implement CQRS Pattern

1. **Tạo Command/Query Interfaces**:

```csharp
public interface ICommand { }
public interface ICommand<TResult> : ICommand { }
public interface IQuery<TResult> { }
```

2. **Tạo Handler Interfaces**:

```csharp
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command);
}

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}
```

3. **Implement Use Case Dispatcher**:

```csharp
public class UseCaseDispatcher : IUseCaseDispatcher
{
    private readonly IServiceProvider _provider;

    public async Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command)
        where TCommand : ICommand<TResult>
    {
        var handler = _provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.HandleAsync(command);
    }
}
```

### Bước 4: Cấu hình Dependency Injection

```csharp
// Program.cs
builder.Services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Register Command Handlers
builder.Services.AddScoped<ICommandHandler<CreateCommand, Entity>, CreateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateCommand, Entity>, UpdateCommandHandler>();

// Register Query Handlers
builder.Services.AddScoped<IQueryHandler<GetAllPagingQuery, PageResult<EntityResponse>>, GetAllPagingQueryHandler>();
```

### Bước 5: Database Configuration

```csharp
// DatabaseContext.cs
public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<YourEntity> YourEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure entities
    }
}
```

## 📦 Packages cần thiết

### API Project

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.11" />
```

### Application Project

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
```

### Infrastructure Project

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
```

## 🔄 Data Flow

1. **Request** → Controller nhận HTTP request
2. **Command/Query** → Controller tạo Command hoặc Query object
3. **Dispatcher** → UseCaseDispatcher tìm và gọi handler tương ứng
4. **Handler** → Handler thực thi business logic
5. **Repository** → Handler sử dụng Repository để truy cập data
6. **Database** → Repository thực thi database operations
7. **Response** → Kết quả được trả về qua các layer

## ✅ Lợi ích của kiến trúc này

1. **Separation of Concerns**: Mỗi layer có trách nhiệm riêng biệt
2. **Testability**: Dễ dàng unit test và integration test
3. **Maintainability**: Code dễ maintain và extend
4. **Scalability**: Có thể scale từng layer độc lập
5. **CQRS Benefits**: Tách biệt read/write operations
6. **Dependency Inversion**: High-level modules không phụ thuộc low-level modules

## 🛠️ Best Practices

1. **Naming Convention**: Sử dụng suffix rõ ràng (Command, Query, Handler)
2. **Error Handling**: Implement global exception handling middleware
3. **Validation**: Sử dụng FluentValidation cho input validation
4. **Logging**: Implement structured logging
5. **Caching**: Thêm caching layer cho queries
6. **Security**: Implement authentication và authorization
7. **Documentation**: Sử dụng XML comments cho API documentation

## 📝 Kết luận

Kiến trúc Clean Architecture với CQRS pattern này cung cấp một foundation mạnh mẽ cho việc phát triển ứng dụng .NET. Nó đảm bảo code quality, maintainability và scalability, đồng thời dễ dàng test và extend trong tương lai.

Bạn có thể sử dụng template này làm base cho các dự án khác, chỉ cần thay đổi domain entities và business logic phù hợp với yêu cầu cụ thể.
