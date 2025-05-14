* 🔧 Setup instructions
* 🏗 Architectural overview
* 🧠 Implementation highlights
* 🚀 Dev/test guidance

---

# 📋 TaskManager Backend

This is the backend implementation for the **TaskManager** interview home assignment. It provides:

* A RESTful API to manage user tasks
* A background service for due-date reminders using RabbitMQ

---

## 🚀 Tech Stack

* **.NET 8** (ASP.NET Core Web API + Worker Service)
* **SQL Server** (via EF Core)
* **RabbitMQ** for queue-based reminder handling
* **FluentValidation + xUnit + Moq** for validation and testing

---

## 🔧 Prerequisites

Before running the project, make sure you have:

| Tool                                                                                                              | Version / Info               |
| ----------------------------------------------------------------------------------------------------------------- | ---------------------------- |
| [.NET SDK](https://dotnet.microsoft.com/en-us/download)                                                           | **.NET 8.0** or higher       |
| [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)                                     | Local or remote instance     |
| [Docker](https://www.docker.com/products/docker-desktop)                                                          | For running RabbitMQ locally |
| Git                                                                                                               | For cloning the repository   |
| Optional: [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) | For development/debugging    |

---

## 📂 Project Structure

```
TaskManagerSolution/
├── TaskManagerApi/             # Main Web API project
├── TaskReminderService/       # Background worker for task reminders
├── TaskManagerApi.Tests/      # Unit tests for the API
```

---

## 🏗 Architecture Overview

* The **API** exposes endpoints for managing tasks.
* Each task includes:

  * Title, description, due date, priority
  * Full name, telephone, and email address
* The **Worker Service**:

  * Periodically polls the DB for overdue tasks
  * Publishes tasks to a RabbitMQ queue
  * Consumes from that queue and logs reminders

---

## ⚙️ Setup Instructions

### ✅ 1. Clone the Repository

```bash
git clone https://github.com/your-username/task-manager-interview.git
cd TaskManagerSolution
```

### ✅ 2. Start RabbitMQ via Docker

```bash
docker run -d --hostname rabbit \
  --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

UI available at [http://localhost:15672](http://localhost:15672)
Login: `guest / guest`

---

### ✅ 3. Configure SQL Server

Update the connection string in both projects:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Ensure SQL Server is running and accessible on your machine.

---

### ✅ 4. Apply EF Core Migrations

From inside the `TaskManagerApi/` project:

```bash
dotnet ef database update
```

---

### ✅ 5. Run the API

```bash
cd TaskManagerApi
dotnet run
```

API will be accessible at:
[https://localhost:5001/swagger](https://localhost:5001/swagger)

---

### ✅ 6. Run the Reminder Service

In another terminal:

```bash
cd TaskReminderService
dotnet run
```

This will start the background service that polls the DB every 30 seconds and sends task reminders via RabbitMQ.

---

## 🧪 Running Tests

```bash
cd TaskManagerApi.Tests
dotnet test
```

Includes unit tests for:

* `TasksController` CRUD logic
* Validation failure cases
* Repository mocking

---

## 🧠 Key Implementation Highlights

### ✅ Reminder Reliability

* Uses RabbitMQ `durable` queues
* Manual `ACK`/`NACK` to avoid lost messages

### ✅ Configuration-Driven

* Uses `appsettings.json` for:

  * RabbitMQ host/queue name
  * SQL connection string

### ✅ Extensibility

* RabbitMQ logic abstracted via `IRabbitMqService`
* Seeding includes:

  * 5 overdue
  * 5 due today
  * 5 future tasks (all with valid 10-digit phone numbers)

---

## 📎 Useful Endpoints

| Method | Route             | Description    |
| ------ | ----------------- | -------------- |
| GET    | `/api/tasks`      | Get all tasks  |
| GET    | `/api/tasks/{id}` | Get task by ID |
| POST   | `/api/tasks`      | Create task    |
| PUT    | `/api/tasks/{id}` | Update task    |
| DELETE | `/api/tasks/{id}` | Delete task    |

---

## ✅ Future Improvements

* Replace logging with real email/SMS reminder logic
* Add user authentication
* Use DTOs and FluentValidation
* Switch from polling to scheduled task queue (e.g. Quartz/Hangfire)

---

Let me know if you want me to generate this as a file or add CI/CD instructions as well!
