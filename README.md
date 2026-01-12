# Currency Exchange Wallet Management

This project is a solution for managing currency exchange and user wallets, incorporating real-time exchange rate updates and persistent storage. It's designed with a hexagonal architecture, separating concerns into different layers such as Core, API, and various adapters for external services and data storage.

## Features

*   **Wallet Management:** Create, add funds to, and subtract funds from user wallets.
*   **Currency Exchange Rates:** Fetches and stores up-to-date currency exchange rates, primarily from the European Central Bank (ECB).
*   **Data Persistence:** Utilizes SQL for primary data storage (wallets, currency rates).
*   **Caching:** Implements Redis for caching currency exchange rates to improve performance and reduce external API calls.
*   **Scheduled Updates:** Background jobs to periodically update currency exchange rates.
*   **API:** Provides a RESTful API for interacting with wallet and currency exchange functionalities.
*   **Extensible Architecture:** Designed with ports and adapters, allowing for easy swapping of data storage or external service providers.

## Technologies Used

*   **.NET 8:** The core framework for the application.
*   **C#:** Primary programming language.
*   **ASP.NET Core:** For building the Web API.
*   **Entity Framework Core:** ORM for interacting with SQL databases.
*   **Redis:** In-memory data store used for caching currency rates.
*   **SQL Server:** Relational database for persistent storage.
*   **Unit Testing:** Ensures reliability and correctness of the application logic.

## Project Structure

The solution is organized into several projects, each with a specific responsibility:

*   **`Core`**: Contains domain entities, business logic (services, strategies), and port interfaces for external dependencies. This is the heart of the application.
    *   `Entities`: `CurrencyRate`, `Wallet`
    *   `Ports`: `ICurrencyRateCache`, `ICurrencyRateProvider`, `ICurrencyRateRepository`, `IWalletRepository`
    *   `Services`: `CurrencyRateService`, `WalletService`
    *   `Strategies`: `IWalletAdjustmentStrategy`, `AddFundsStrategy`, `SubtractFundsStrategy`, `ForceSubtractFundsStrategy`
*   **`Api`**: The entry point of the application, exposing RESTful endpoints and orchestrating interactions between the UI and the Core domain.
    *   `Controllers`: `HealthController`, `WalletsController`
    *   `Jobs`: `UpdateCurrencyRatesJob` (for scheduled updates)
    *   `Contracts`: DTOs for API requests and responses.
*   **`Adapter.ECB`**: Implements the `ICurrencyRateProvider` port using the European Central Bank's exchange rate data.
*   **`Adapter.Redis`**: Implements the `ICurrencyRateCache` port using Redis.
*   **`Adapter.SQL`**: Implements `ICurrencyRateRepository` and `IWalletRepository` ports using Entity Framework Core for SQL database interaction.
    *   `DbContext`: `AppDbContext`
    *   `Migrations`: Database migrations for EF Core.
*   **`ApiTests`**, **`CurrencyRateServiceTests`**, **`EcbCurrencyRateProviderTests`**, **`WalletApiTests`**, **`WalletServiceTests`**, **`WalletStrategiesTests`**: Projects containing unit tests for different parts of the application.

## Setup and Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/CurrencyExchangeWalletManagment.git
    cd CurrencyExchangeWalletManagment
    ```
2.  **Restore NuGet packages:**
    ```bash
    dotnet restore
    ```
3.  **Database Setup:**
    *   Ensure you have a SQL Server (or compatible) instance running.
    *   Update the connection string in `Api/appsettings.json` (or `appsettings.Development.json`) to point to your database.
    *   Apply database migrations:
        ```bash
        dotnet ef database update --project Adapter.SQL
        ```
4.  **Redis Setup:**
    *   Ensure a Redis instance is running and accessible.
    *   Update Redis connection details in `Api/appsettings.json` if necessary.

## How to Run

To run the API:

```bash
dotnet run --project Api
```

The API will typically be available at `https://localhost:7270` or `http://localhost:5131`, depending on your `launchSettings.json` and environment configuration.

## Running Tests

To run all unit tests:

```bash
dotnet test
```

## License

This project is licensed under the terms of the LICENSE file.
