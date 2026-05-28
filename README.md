# 💼 PayrollPro — Professional HR & Payroll Management System

![PayrollPro Banner](https://img.shields.io/badge/PayrollPro-HR%20%26%20Payroll%20Management-2563EB?style=for-the-badge&logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI+PHBhdGggZmlsbD0id2hpdGUiIGQ9Ik0xMiAyQzYuNDggMiAyIDYuNDggMiAxMnM0LjQ4IDEwIDEwIDEwIDEwLTQuNDggMTAtMTBTMTcuNTIgMiAxMiAyem0xIDE1aC0ydi02aDJ2NnptMC04aC0yVjdoMnYyeiIvPjwvc3ZnPg==)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![Next.js](https://img.shields.io/badge/Next.js-15-000000?style=for-the-badge&logo=nextdotjs)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-336791?style=for-the-badge&logo=postgresql)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=for-the-badge&logo=typescript)

> A full-stack, production-ready payroll management system built for
> Nigerian businesses. Handles employee management, automated tax
> calculations (PAYE, Pension, NHF), payroll processing with approval
> workflows, PDF payslip generation, and financial reporting — all in
> one secure, role-based platform.

---

## ✨ Features

### 👥 Employee Management
- Complete employee lifecycle management (onboard, update, deactivate)
- Never hard-deletes records — full audit trail preserved forever
- Bank account verification with Nigerian account number validation
- Department and employment type classification

### 💰 Payroll Engine
- One-click payroll processing for entire company
- Automatic Nigerian statutory deductions:
  - **PAYE** — Pay As You Earn income tax (FIRS 2024 bands)
  - **Pension** — Employee 8% + Employer 10% (PRA 2014)
  - **NHF** — National Housing Fund 2.5% of basic salary
- Pro-rated salary support for mid-month joiners
- Duplicate payroll prevention per pay period

### 🔐 Security & Access Control
- JWT-based authentication with 1-hour token expiry
- Role-based access control (RBAC) with 5 distinct roles
- **Four-Eyes Principle** — payroll creator cannot approve own run
- Account lockout after 5 failed login attempts
- Full immutable audit trail of every action

### 🧾 Payslip Generation
- Professional branded PDF payslips via QuestPDF
- Automatic breakdown of earnings and deductions
- Downloadable by employees via self-service portal
- Employer pension contribution noted separately

### 📊 Reports & Exports
- Dashboard with live KPIs and department breakdown
- Monthly payroll summary with statutory deductions report
- Bank payment schedule CSV export for bulk transfers
- Audit log with pagination for compliance

### 📱 Fully Responsive
- Mobile-first design works on all screen sizes
- Slide-in sidebar navigation on mobile devices
- Auto-dismissing success and error notifications

---

## 🏗️ Tech Stack

| Layer | Technology | Purpose |
|---|---|---|
| **Frontend** | Next.js 15 (App Router) | UI and routing |
| **Language** | TypeScript | Type safety |
| **Styling** | Tailwind CSS | Responsive design |
| **Backend** | .NET 10 Web API | Business logic |
| **Auth** | ASP.NET Identity + JWT | Authentication |
| **Database** | PostgreSQL 18 | Data storage |
| **ORM** | Entity Framework Core | Database access |
| **PDF** | QuestPDF | Payslip generation |
| **Email** | MailKit | Notifications |

---

## 🚀 Getting Started

### Prerequisites

Make sure you have these installed:

| Tool | Version | Download |
|---|---|---|
| Node.js | 20+ LTS | https://nodejs.org |
| .NET SDK | 10.0 | https://dotnet.microsoft.com/download |
| PostgreSQL | 15+ | https://www.postgresql.org/download |
| Git | Latest | https://git-scm.com |

---

### 1. Clone the Repository

```bash
git clone https://github.com/milado98/payroll-app.git
cd payroll-app
```

---

### 2. Set Up the Backend

```bash
cd payrollAPI
```

Create `appsettings.json` and fill in your values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=payrolldb;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "PayrollAPI",
    "Audience": "PayrollClient",
    "ExpiryMinutes": 60
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "yourcompany@gmail.com",
    "SenderPassword": "your_app_password"
  }
}
```

Install dependencies and run migrations:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

The API will start at `http://localhost:5262`
API docs available at `http://localhost:5262/scalar/v1`

---

### 3. Create Your Super Admin

Call this endpoint once to create the first admin account:

```bash
curl -X POST http://localhost:5262/api/auth/setup \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@yourcompany.com",
    "firstName": "Super",
    "lastName": "Admin",
    "password": "Admin@12345"
  }'
```

> ⚠️ This endpoint automatically disables itself after first use.

---

### 4. Set Up the Frontend

```bash
cd ../payroll-frontend
```

Create `.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5262/api
```

Install and run:

```bash
npm install
npm run dev
```

The app will be available at `http://localhost:3000`

---

## 👤 User Roles & Permissions

| Feature | Super Admin | HR Admin | Finance Officer | Manager | Employee |
|---|---|---|---|---|---|
| Create employees | ✅ | ✅ | ❌ | ❌ | ❌ |
| Edit employees | ✅ | ✅ | ❌ | ❌ | ❌ |
| View all employees | ✅ | ✅ | ✅ | Team | ❌ |
| Run payroll | ✅ | ✅ | ❌ | ❌ | ❌ |
| Approve payroll | ✅ | ❌ | ✅ | ❌ | ❌ |
| View all payslips | ✅ | ✅ | ✅ | Team | Own |
| Download payslips | ✅ | ✅ | ✅ | ✅ | ✅ |
| View reports | ✅ | ✅ | ✅ | ❌ | ❌ |
| Bank schedule | ✅ | ❌ | ✅ | ❌ | ❌ |
| Audit logs | ✅ | ❌ | ❌ | ❌ | ❌ |
| Manage users | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## 🧮 Nigerian Tax Calculation

PayrollPro implements the **FIRS 2024 PAYE tax bands**:

| Annual Taxable Income | Tax Rate |
|---|---|
| First ₦300,000 | 7% |
| Next ₦300,000 | 11% |
| Next ₦500,000 | 15% |
| Next ₦500,000 | 19% |
| Next ₦1,600,000 | 21% |
| Above ₦3,200,000 | 24% |

**Statutory Deductions Applied:**
- **PAYE** — Calculated on taxable income after CRA and reliefs
- **Pension** — 8% employee contribution + 10% employer (PRA 2014)
- **NHF** — 2.5% of basic salary only
- **CRA** — Consolidated Relief Allowance deducted before tax

---

## 📁 Project Structure

```
payroll-app/
├── payrollAPI/                  # .NET 10 Web API
│   ├── Controllers/             # API endpoints
│   │   ├── AuthController.cs    # Login, user creation
│   │   ├── EmployeeController.cs
│   │   ├── SalaryController.cs
│   │   ├── PayrollController.cs
│   │   ├── PayslipController.cs
│   │   └── ReportController.cs
│   ├── Services/                # Business logic
│   │   ├── EmployeeService.cs
│   │   ├── SalaryService.cs
│   │   ├── PayrollService.cs
│   │   └── PayslipPdfService.cs
│   ├── Models/                  # Database entities
│   ├── DTOs/                    # Request/response shapes
│   ├── Helpers/                 # Tax calculator
│   └── Data/                    # DB context
│
└── payroll-frontend/            # Next.js 15 Frontend
    ├── app/
    │   ├── login/               # Login page
    │   └── dashboard/           # Protected pages
    │       ├── page.tsx         # Dashboard home
    │       ├── employees/       # Employee management
    │       ├── payroll/         # Payroll runs
    │       ├── payslips/        # Payslip viewer
    │       ├── reports/         # Financial reports
    │       └── settings/        # User management
    ├── components/              # Reusable UI
    ├── context/                 # Auth context
    ├── hooks/                   # Custom hooks
    └── lib/                     # API client
```

---

## 🔒 Security Features

- 🔑 **JWT Authentication** with configurable expiry
- 🛡️ **Role-Based Access Control** enforced on every API endpoint
- 🔐 **BCrypt password hashing** — passwords never stored in plain text
- 🚫 **Rate limiting** — accounts lock after 5 failed attempts
- 👁️ **Four-Eyes Principle** — financial approval requires two users
- 📋 **Immutable Audit Trail** — every action logged with IP and timestamp
- 🔒 **HTTPS enforced** in production
- 🚧 **CORS configured** — only trusted origins accepted

---


## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License.

---

## 👨‍💻 Author

Built with ❤️ for Nigerian businesses that deserve professional
payroll tooling without enterprise price tags.

---

> **PayrollPro** — Because your team deserves to be paid right,
> on time, every time.