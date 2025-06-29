
# 🚢 Vessel Movement Tracker – Backend (ASP.NET Core + PostgreSQL)

This is the **backend API** for the Vessel Movement Tracker project. It is built with **ASP.NET Core Web API** and **Entity Framework Core**, and uses **PostgreSQL (Supabase)** as the database. The API provides endpoints to simulate vessel movement, track progress, and manage vessel data in real-time.

## 🌐 Live API
- **Swagger UI**: [https://vessel-movement-tracker-production.up.railway.app/swagger/index.html](https://vessel-movement-tracker-production.up.railway.app/swagger/index.html)
- **Base URL**: `https://vessel-movement-tracker-production.up.railway.app/api/vessels`

## 🛠 Features

- Add new vessels with origin, destination, speed, and distance.
- Automatically updates vessels’ progress based on elapsed time.
- Real-time simulation of movement.
- View all vessels with current progress and estimated time remaining.
- Reset all vessels to restart journeys.
- Delete specific vessels.

## 🧩 Technologies Used

- ASP.NET Core 8
- PostgreSQL (via Supabase)
- Entity Framework Core (EF Core)
- Docker (for deployment)
- Railway (for hosting)

## 📁 Project Structure

```
VesselMovementAPI/
├── Controllers/         # API Controllers
│   └── VesselsController.cs
├── Models/              # Vessel model
│   └── Vessel.cs
├── Data/                # DB Context
│   └── AppDbContext.cs
├── Program.cs           # Main entry point & app setup
├── appsettings.json     # Config file
└── Dockerfile           # Docker build setup
```

## 🔗 API Endpoints

| Method | Endpoint                 | Description                     |
|--------|--------------------------|---------------------------------|
| GET    | `/api/vessels`           | List all vessels                |
| GET    | `/api/vessels/{id}`      | Get a specific vessel           |
| POST   | `/api/vessels`           | Add a new vessel                |
| DELETE | `/api/vessels/{id}`      | Delete a specific vessel        |
| POST   | `/api/vessels/reset`     | Reset all vessels               |

## ⚙️ Setup & Run (Local)

```bash
git clone https://github.com/<your-username>/vessel-movement-tracker.git
cd VesselMovementAPI
dotnet restore
dotnet run
```

## 📦 Deployment

- **Platform**: [Railway](https://railway.app)
- **Database**: Supabase PostgreSQL
- Use IPv4-compatible connection string for Railway.
- `.env` or Railway variable: `DATABASE_URL`

## 📄 License

MIT License
