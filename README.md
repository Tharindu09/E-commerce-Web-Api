# E-commerce Web API

This repository contains a comprehensive, microservices-based backend for an e-commerce platform, built using .NET 8 and ASP.NET Core. It demonstrates a variety of modern software architecture patterns, including API Gateway, inter-service communication via gRPC, asynchronous messaging with Kafka, and the Saga pattern for distributed transactions.

## Architecture Overview

The system is designed as a collection of independent, domain-focused microservices that work together to provide a complete e-commerce experience.

-   **API Gateway**: A single entry point for all client requests, handling routing, authentication, and load balancing.
-   **Inter-service Communication**: Services communicate internally using high-performance gRPC calls for synchronous operations.
-   **Event-Driven Communication**: For decoupling long-running processes and ensuring resilience, the system uses Apache Kafka for asynchronous event-driven communication, particularly for payment status updates.

![Architecture Diagram](/docs/img/HLA.png)

## Services

The application is composed of the following services:

| Service | Description | Database | Key Technologies |
| :--- | :--- | :--- | :--- |
| **APIGateway** | The single entry point for all external client requests. It uses YARP (Yet Another Reverse Proxy) to route requests to the appropriate downstream service and handles JWT authentication. | - | .NET 8, YARP, JWT |
| **UserService** | Manages user data, including registration, profile management, and authentication. It generates JWTs for authenticated users and provides user profile information to other services via a gRPC endpoint. | PostgreSQL | EF Core, JWT, gRPC, BCrypt |
| **ProductService** | Responsible for the product catalog and inventory management. It implements a stock reservation system (Saga pattern) to ensure data consistency during order creation. | PostgreSQL | EF Core, gRPC, Saga Pattern |
| **CartService** | Manages user shopping carts. It uses Redis for fast, ephemeral storage of cart data and communicates with the Product Service via gRPC to fetch real-time product information. | Redis | StackExchange.Redis, gRPC |
| **OrderService** | Orchestrates the complex process of creating an order. It fetches data from the User, Cart, and Product services via gRPC. It also subscribes to Kafka events to update order statuses based on payment outcomes. | PostgreSQL | EF Core, gRPC, Kafka Consumer |
| **PaymentService** | Handles the payment process by integrating with the Stripe payment gateway. It publishes payment success or failure events to a Kafka topic, which are then consumed by the Order Service. | PostgreSQL | EF Core, Stripe, Kafka Producer |

## Key Features

-   **Microservices Architecture**: A scalable and maintainable system with decoupled services.
-   **Authentication & Authorization**: Secure endpoints using JSON Web Tokens (JWT).
-   **API Gateway**: A unified and secure entry point for all client applications.
-   **Synchronous & Asynchronous Communication**: Efficient service-to-service communication with gRPC and resilient, event-driven workflows with Kafka.
-   **Distributed Transactions**: Implements the Saga pattern for stock reservation to maintain data consistency across services.
-   **Third-Party Integration**: Seamlessly integrates with Stripe for payment processing.
-   **Data Persistence**: Utilizes both PostgreSQL for relational data and Redis for high-performance caching and session management.

## Technologies Used

-   **Framework**: .NET 8, ASP.NET Core
-   **API Gateway**: YARP (Yet Another Reverse Proxy)
-   **Databases**:
    -   PostgreSQL with Entity Framework Core
    -   Redis
-   **Communication**:
    -   RESTful APIs
    -   gRPC
    -   Apache Kafka
-   **Authentication**: JWT (JSON Web Tokens)
-   **Payment**: Stripe

## Getting Started

### Prerequisites

-   .NET 8 SDK
-   Docker and Docker Compose
-   An IDE like Visual Studio or VS Code

### 1. Configure Infrastructure

The project relies on PostgreSQL, Redis, and Kafka. Ensure these services are running. You can use the provided `docker-compose.yaml` in the `CartService` directory to start a Redis instance. You will need to set up PostgreSQL and Kafka separately.

```bash
# In the CartService directory
docker-compose up -d
```

### 2. Update Configuration

Before running the application, you need to update the `appsettings.json` file in each service directory (`UserService`, `ProductService`, `CartService`, `OrderService`, `PaymentService`) with the correct connection strings and API keys.

-   **PostgreSQL Connection String**: Update the `ConnectionStrings:DefaultConnection` for services using PostgreSQL.
-   **Redis Connection String**: Update `ConnectionStrings:Redis` in `CartService`.
-   **Kafka Bootstrap Servers**: Update `Kafka:BootstrapServers` in `OrderService` and `PaymentService`.
-   **Stripe API Keys**: Add your Stripe `SecretKey` and `WebhookSecret` in the `PaymentService/appsettings.json`.

### 3. Apply Database Migrations

For each service that uses PostgreSQL, navigate to its project directory and apply the EF Core migrations to create the database schema.

```bash
# Example for UserService
cd UserService
dotnet ef database update
```
Repeat this process for `ProductService`, `OrderService`, and `PaymentService`.

### 4. Run the Services

You can run the entire solution using Visual Studio or start each service individually from the command line.

```bash
# Navigate to a service directory
cd path/to/service
dotnet run
```
Start all services for the application to be fully functional: `APIGateway`, `UserService`, `ProductService`, `CartService`, `OrderService`, and `PaymentService`.

## API Endpoints

All endpoints are accessed through the API Gateway, which typically runs on `http://localhost:5000`. Most routes require a JWT Bearer token in the `Authorization` header.

### Authentication (`/api/auth`)

-   `POST /register`: Register a new user.
-   `POST /login`: Authenticate a user and receive a JWT.

### Products (`/api/products`)

-   `GET /`: Get a list of all products.
-   `GET /{id}`: Get details for a specific product.
-   `POST /`: (Admin) Add a new product.

### Cart (`/api/cart`)

-   `GET /{userId}`: Get the current user's cart.
-   `POST /{userId}/add`: Add an item to the cart.
-   `DELETE /{userId}/remove/{productId}`: Remove an item from the cart.
-   `DELETE /{userId}/clear`: Clear all items from the cart.

### Orders (`/api/orders`)

-   `POST /create/{userId}`: Create a new order from the user's cart.
-   `GET /{orderId}`: Get details for a specific order.

### Payments (`/api/payments`)

-   `POST /process`: Process a payment for an order.
-   `POST /webhook`: Webhook endpoint for receiving status updates from Stripe.
