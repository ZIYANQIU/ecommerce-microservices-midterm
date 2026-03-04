# 🛒 E-Commerce Microservices Backend

A containerized distributed backend system for a simplified e-commerce platform, built with **ASP.NET Core**, **Entity Framework Core**, and **Docker**. 

This project strictly implements the **Database-per-service** architectural pattern to ensure data ownership and fault isolation.

## 🚀 Quick Start

To launch the entire microservices cluster, run the following command in the root directory:

```bash
docker compose up --build
```

*Note: EF Core will automatically apply migrations and generate the independent SQLite databases (`.db` files) upon startup.*

## 🔌 API Endpoints & Swagger UI

Once the containers are running, access the Swagger UI for each service via the mapped ports:

| Microservice | Internal Port | External Access (Swagger UI) | Database |
| :--- | :--- | :--- | :--- |
| **Customer Service** | 8080 | [http://localhost:5001/swagger](http://localhost:5001/swagger) | `customers.db` |
| **Order Service** | 8080 | [http://localhost:5002/swagger](http://localhost:5002/swagger) | `orders.db` |
| **Seller Service** | 8080 | [http://localhost:5003/swagger](http://localhost:5003/swagger) | `sellers.db` |
| **Product Service** | 8080 | [http://localhost:5004/swagger](http://localhost:5004/swagger) | `products.db` |

## 🏗️ Architecture Highlights

- **Data Ownership**: 4 independent microservices, each managing its own isolated SQLite database.
- **Synchronous Communication**: HTTP client implementation for cross-service validation (e.g., Order API verifying Customer and Product existence) to maintain data integrity without shared databases.

## 🧪 Testing Flow

For a complete End-to-End (E2E) test, it is recommended to interact with the APIs in the following order:
1. `POST /api/sellers` (Create a Seller)
2. `POST /api/products` (Create a Product linked to the SellerId)
3. `POST /api/customers` (Create a Customer)
4. `POST /api/orders` (Create an Order linked to CustomerId & ProductId)