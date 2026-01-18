# UTE Learning Hub

Nền tảng chia sẻ tài liệu học tập thông minh dành cho sinh viên Đại học Sư phạm Kỹ thuật (UTE), tích hợp AI để gợi ý tài liệu và nhóm học tập phù hợp.

---

## Tính năng chính

### Dành cho Sinh viên
- **Đăng nhập Microsoft** - Xác thực qua tài khoản email sinh viên UTE (@ute.udn.vn)
- **Quản lý tài liệu** - Upload, chia sẻ và tìm kiếm tài liệu theo môn học, khoa, chuyên ngành
- **Nhóm học tập** - Tạo và tham gia các nhóm học tập theo chủ đề
- **Chat realtime** - Nhắn tin trong nhóm với hỗ trợ gửi file và hình ảnh
- **Gợi ý thông minh (AI)** - Đề xuất nhóm học tập và tài liệu dựa trên sở thích và hành vi
- **Thư viện cá nhân** - Lưu tài liệu yêu thích để đọc sau
- **Đánh giá tài liệu** - Review và đánh giá chất lượng tài liệu
- **Báo cáo nội dung** - Báo cáo tài liệu/nhóm vi phạm quy định

### Dành cho Admin
- **Quản lý tài khoản** - Xem, phân quyền và khóa tài khoản người dùng
- **Duyệt tài liệu** - Phê duyệt hoặc từ chối tài liệu được upload
- **Xử lý báo cáo** - Giải quyết các báo cáo vi phạm từ người dùng
- **Quản lý danh mục** - CRUD khoa, chuyên ngành, môn học, loại tài liệu, tags
- **Thống kê** - Dashboard với biểu đồ phân tích hoạt động hệ thống

---

## Công nghệ sử dụng

| Layer | Công nghệ |
|-------|-----------|
| **Frontend** | Next.js 16, React 19, TypeScript, TailwindCSS 4, Radix UI |
| **Backend** | .NET 9.0, Clean Architecture, MediatR (CQRS), Entity Framework Core |
| **AI Service** | Python, FastAPI, Sentence Transformers (all-MiniLM-L6-v2) |
| **Database** | SQL Server 2022 |
| **Realtime** | SignalR (WebSocket) |
| **Auth** | Microsoft Identity (MSAL) |
| **Infrastructure** | Docker, Nginx (Reverse Proxy), Let's Encrypt SSL |

---

## Cấu trúc dự án

```
ute-learning-hub/
├── frontend/          # Next.js 16 - Giao diện người dùng
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

## Hướng dẫn cài đặt

### Yêu cầu
- Docker & Docker Compose
- Git

### Chạy với Docker

```bash
# Clone repository
git clone https://github.com/your-org/ute-learning-hub.git
cd ute-learning-hub

# Copy file environment
cp .env.example .env
# Chỉnh sửa .env với các giá trị phù hợp

# Build và khởi động tất cả services
docker-compose up -d --build

# Xem logs
docker-compose logs -f
```

### Truy cập ứng dụng

| Service | URL | Mô tả |
|---------|-----|-------|
| **Frontend** | http://localhost | Giao diện web |
| **API Docs** | http://localhost/scalar | Swagger/Scalar documentation |
| **Health Check** | http://localhost/health | Kiểm tra trạng thái |

---

## Phát triển Local

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
| Service | Port | Mô tả |
|---------|------|-------|
| SQL Server | 1433 | Database chính |

### Application
| Service | Port nội bộ | Mô tả |
|---------|-------------|-------|
| Backend | 7080 | .NET API |
| Frontend | 3000 | Next.js SSR |
| AI | 8000 | Recommendation Engine |
| Nginx | 80, 443 | Reverse Proxy & SSL |

### Quản lý Services

```bash
# Khởi động tất cả
docker-compose up -d --build

# Khởi động từng service
docker-compose up -d backend
docker-compose up -d frontend
docker-compose up -d ai

# Xem logs service cụ thể
docker-compose logs -f backend

# Dừng tất cả
docker-compose down

# Dừng và xóa volumes (reset data)
docker-compose down -v
```

---

## Nginx Routing

| Path | Service | Mô tả |
|------|---------|-------|
| `/` | Frontend | Next.js pages |
| `/api/*` | Backend | REST API |
| `/hubs/*` | Backend | SignalR WebSocket |
| `/images/*` | Backend | Static files (cached 30 days) |
| `/scalar` | Backend | API Documentation |

---

## AI Recommendation System

Hệ thống sử dụng **Sentence Transformers** (model `all-MiniLM-L6-v2`) để tạo vector embeddings 384 chiều cho:

- **User Vector**: Dựa trên môn học và tags mà user quan tâm (từ lịch sử đọc, upload, đánh giá)
- **Conversation Vector**: Dựa trên tên nhóm, môn học liên quan và tags

### API Endpoints

```bash
# Tạo user vector
POST /vector/user
{
  "subjects": ["Lập trình Python", "Machine Learning"],
  "subjectWeights": [10, 5],
  "tags": ["AI", "Data Science"],
  "tagWeights": [12, 8]
}

# Gợi ý nhóm học tập
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

Các entity chính:
- **AppUser** - Thông tin người dùng (linked với Microsoft Identity)
- **Document** - Tài liệu với nhiều DocumentFile
- **DocumentFile** - File PDF/DOCX/PPTX với metadata
- **Conversation** - Nhóm học tập
- **Message** - Tin nhắn trong nhóm (hỗ trợ attachments)
- **Subject, Faculty, Major, Type, Tag** - Danh mục phân loại
- **ProfileVector, ConversationVector** - Vector embeddings cho AI

---

## Biến môi trường

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