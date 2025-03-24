# Inno_Shop Microservices

Inno_Shop is a microservices-based project with **User Management** and **Product Management** services.
Built with **ASP.NET Core**, the services communicate via **RESTful APIs** and are secured using **JWT authentication**. **Docker** is used for deployment.

---

## Project Status

### Time Spent: ~24 hours

### Completed:
1. **User Management Microservice**:
   - CRUD operations for users.
   - JWT authentication and password recovery.
   - Account verification and soft delete for users.

2. **Product Management Microservice**:
   - CRUD operations for products.
   - User-specific product management (users can only manage their products).
   - Basic search and filter functionality.

3. **Architecture**:
   - Implemented **Clean/Onion Architecture**.
   - Used **MediatR** and **FluentValidation**.
   - **ProblemDetails** for exception handling.

### Not Done:
1. **Docker Deployment**:
   - Need to finalize Docker Compose setup

2. **Integration Tests**:
   - Pending integration tests for both services

---

## Setup & Installation

### Prerequisites:
- .NET 8.0 SDK
- Docker and Docker Compose
- PostgreSQL or SQL Server

### Run Locally:
1. **Clone the Repo:**
   ```bash
   git clone <repo-url>
   cd Inno_Shop
