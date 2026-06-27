# UTE Learning Hub

> **Language / Ngôn ngữ:** **English** | [Tiếng Việt](README.vi.md)

A smart learning-material sharing platform for students, powered by AI to recommend relevant documents and study groups.

---

## Key Features

### For Students
- **Microsoft Sign-in** - Authenticate with the UTE student email account (@ute.udn.vn)
- **Document management** - Upload, share and search documents by subject, faculty and major
- **Study groups** - Create and join topic-based study groups
- **Realtime chat** - Message within groups with file and image support
- **Smart recommendations (AI)** - Suggest study groups and documents based on interests and behavior
- **Personal library** - Save favorite documents to read later
- **Document reviews** - Review and rate document quality
- **Content reporting** - Report documents/groups that violate the rules

### For Admins
- **Account management** - View, assign roles and lock user accounts
- **Document moderation** - Approve or reject uploaded documents
- **Report handling** - Resolve violation reports from users
- **Category management** - CRUD for faculties, majors, subjects, document types and tags
- **Analytics** - Dashboard with charts analyzing system activity

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | Next.js 16, React 19, TypeScript, TailwindCSS 4, Radix UI |
| **Backend** | .NET 9.0, Clean Architecture, MediatR (CQRS), Entity Framework Core |
| **AI Service** | Python, FastAPI, Sentence Transformers (all-MiniLM-L6-v2) |
| **Database** | SQL Server 2022 |
| **Realtime** | SignalR (WebSocket) |
| **Auth** | Microsoft Identity (MSAL) |
| **Infrastructure** | Docker, Nginx (Reverse Proxy), Let's Encrypt SSL |

---

## Project Structure

```
ute-learning-hub/
├── frontend/          # Next.js 16 - User interface
├── backend/           # .NET 9.0 - RESTful API (Clean Architecture)
│   ├── UteLearningHub.Api/           # Controllers, Middleware
│   ├── UteLearningHub.Application/   # Use Cases, Commands, Queries
│   ├── UteLearningHub.Domain/        # Entities, Events, Interfaces
│   ├── UteLearningHub.Infrastructure/# External Services, Email, AI Client
│   └── UteLearningHub.Persistence/   # EF Core, Repositories, Migrations
├── ai/                # Python FastAPI - AI Recommendation Service
├── nginx/             # Nginx configuration
├── docker-compose.yml # Development environment
└── docker-compose.prod.yml # Production environment
```

---

## Getting Started

### Requirements
- Docker & Docker Compose
- Git

### Run with Docker

```bash
# Clone the repository
git clone https://github.com/your-org/ute-learning-hub.git
cd ute-learning-hub

# Copy the environment file
cp .env.example .env
# Edit .env with the appropriate values

# Build and start all services
docker-compose up -d --build

# View logs
docker-compose logs -f
```

### Access the Application

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost | Web interface |
| **API Docs** | http://localhost/scalar | Swagger/Scalar documentation |
| **Health Check** | http://localhost/health | Status check |

---

## Local Development

### Backend (.NET 9.0)

```bash
cd backend
dotnet restore
dotnet run --project UteLearningHub.Api
# → http://localhost:7080
```

### Frontend (Next.js 16)

```bash
cd frontend
npm install
npm run dev
# → http://localhost:3000
```

### AI Service (Python)

```bash
cd ai
pip install -r requirements.txt
python main.py
# → http://localhost:8000/docs
```

---

## Docker Services

### Infrastructure
| Service | Port | Description |
|---------|------|-------------|
| SQL Server | 1433 | Main database |

### Application
| Service | Internal port | Description |
|---------|---------------|-------------|
| Backend | 7080 | .NET API |
| Frontend | 3000 | Next.js SSR |
| AI | 8000 | Recommendation Engine |
| Nginx | 80, 443 | Reverse Proxy & SSL |

### Managing Services

```bash
# Start everything
docker-compose up -d --build

# Start individual services
docker-compose up -d backend
docker-compose up -d frontend
docker-compose up -d ai

# View logs for a specific service
docker-compose logs -f backend

# Stop everything
docker-compose down

# Stop and remove volumes (reset data)
docker-compose down -v
```

---

## Nginx Routing

| Path | Service | Description |
|------|---------|-------------|
| `/` | Frontend | Next.js pages |
| `/api/*` | Backend | REST API |
| `/hubs/*` | Backend | SignalR WebSocket |
| `/images/*` | Backend | Static files (cached 30 days) |
| `/scalar` | Backend | API Documentation |

---

## AI Recommendation System

The system uses **Sentence Transformers** (model `all-MiniLM-L6-v2`) to generate 384-dimensional vector embeddings for:

- **User Vector**: Based on the subjects and tags a user is interested in (from reading, upload and review history)
- **Conversation Vector**: Based on the group name, related subjects and tags

### API Endpoints

```bash
# Create a user vector
POST /vector/user
{
  "subjects": ["Python Programming", "Machine Learning"],
  "subjectWeights": [10, 5],
  "tags": ["AI", "Data Science"],
  "tagWeights": [12, 8]
}

# Recommend study groups
POST /recommend
{
  "UserVector": [...],
  "ConversationVectors": [{"id": "xxx", "vector": [...]}],
  "TopK": 10,
  "MinSimilarity": 0.3
}
```

---

## Database Schema

Main entities:
- **AppUser** - User information (linked with Microsoft Identity)
- **Document** - A document with multiple DocumentFiles
- **DocumentFile** - PDF/DOCX/PPTX file with metadata
- **Conversation** - Study group
- **Message** - Messages within a group (supports attachments)
- **Subject, Faculty, Major, Type, Tag** - Classification categories
- **ProfileVector, ConversationVector** - Vector embeddings for AI

---

## Environment Variables

```bash
# Database
DB_SA_PASSWORD=YourStrong@Passw0rd
DB_NAME=ute-learning-hub

# Microsoft OAuth
MICROSOFT_CLIENT_ID=your-client-id
MICROSOFT_TENANT_ID=your-tenant-id

# Public URL (production)
PUBLIC_URL=https://your-domain.com
```
