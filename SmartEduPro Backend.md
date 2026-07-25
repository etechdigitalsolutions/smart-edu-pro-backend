# SmartEdu Pro — Backend API & Database Specification
**Version:** 1.0.0 | **Date:** July 2025 | **Platform:** Sri Lankan Tuition Management SaaS

---

## Table of Contents
1. [System Architecture](#1-system-architecture)
2. [Technology Stack](#2-technology-stack)
3. [Authentication & Security](#3-authentication--security)
4. [Standard API Conventions](#4-standard-api-conventions)
5. [PostgreSQL Database Schema](#5-postgresql-database-schema)
6. [API Endpoints — Auth](#6-api-endpoints--auth)
7. [API Endpoints — Super Admin](#7-api-endpoints--super-admin)
8. [API Endpoints — Institutes & Branches](#8-api-endpoints--institutes--branches)
9. [API Endpoints — Students](#9-api-endpoints--students)
10. [API Endpoints — Teachers](#10-api-endpoints--teachers)
11. [API Endpoints — Parents](#11-api-endpoints--parents)
12. [API Endpoints — Classes & Subjects](#12-api-endpoints--classes--subjects)
13. [API Endpoints — Attendance (QR)](#13-api-endpoints--attendance-qr)
14. [API Endpoints — Exams & Results](#14-api-endpoints--exams--results)
15. [API Endpoints — Timetable](#15-api-endpoints--timetable)
16. [API Endpoints — Finance & Fees](#16-api-endpoints--finance--fees)
17. [API Endpoints — Online Learning](#17-api-endpoints--online-learning)
18. [API Endpoints — Notifications](#18-api-endpoints--notifications)
19. [API Endpoints — Messages (Chat)](#19-api-endpoints--messages-chat)
20. [API Endpoints — Dashboard & Reports](#20-api-endpoints--dashboard--reports)
21. [API Endpoints — QR Code](#21-api-endpoints--qr-code)
22. [API Endpoints — Billing (SaaS)](#22-api-endpoints--billing-saas)
23. [Enum Reference](#23-enum-reference)
24. [Error Code Reference](#24-error-code-reference)
25. [Webhook Events](#25-webhook-events)

---

## 1. System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      CLIENT LAYER                           │
│  Super Admin Web  │  Institute Web  │  Teacher Web          │
│  Parent Mobile (iOS/Android)  │  Student Mobile (iOS/Android)│
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTPS / WSS
┌──────────────────────────▼──────────────────────────────────┐
│                   API GATEWAY (Nginx / AWS ALB)             │
│              Rate Limiting · SSL Termination · CORS         │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                 REST API SERVER (Node.js / Express)         │
│  JWT Auth Middleware  │  Role-Based Access Control          │
│  Multi-tenant Institute Isolation Middleware                 │
└────────┬──────────────────────────────────┬─────────────────┘
         │                                  │
┌────────▼─────────┐             ┌──────────▼─────────────────┐
│  PostgreSQL 15   │             │  Redis (Cache + Sessions)   │
│  Primary DB      │             │  QR Token Store             │
│  Read Replicas   │             │  Rate Limit Buckets         │
└────────┬─────────┘             └────────────────────────────┘
         │
┌────────▼─────────────────────────────────────────────────────┐
│              BACKGROUND SERVICES                              │
│  Notification Worker  │  SMS Gateway (Dialog/Mobitel)        │
│  WhatsApp API (Meta)  │  Email (SendGrid)                    │
│  Push (FCM/APNs)      │  PDF Generator (Puppeteer)           │
│  Video Storage (S3)   │  QR Code Generator                   │
└──────────────────────────────────────────────────────────────┘
```

### Multi-Tenancy Model
- Every request is scoped to an `institute_id`
- Row-Level Security (RLS) enforced at PostgreSQL layer
- Super Admin bypasses institute scope

---

## 2. Technology Stack

| Layer | Technology | Notes |
|-------|-----------|-------|
| Runtime | Node.js 20 LTS | |
| Framework | Express.js 4.x | |
| Language | TypeScript 5.x | |
| Database | PostgreSQL 15 | Primary + Read Replicas |
| Cache | Redis 7 | Sessions, QR tokens, rate limits |
| ORM | Prisma 5.x | Type-safe queries |
| Auth | JWT (RS256) | Access + Refresh token pair |
| File Storage | AWS S3 / Cloudflare R2 | Videos, PDFs, images |
| Video Streaming | Agora.io or LiveKit | Live classes |
| SMS | Dialog Ideamart / Mobitel | Sri Lankan carriers |
| WhatsApp | Meta Cloud API | Attendance, fee alerts |
| Push | Firebase FCM + APNs | Mobile apps |
| Email | SendGrid | Invoices, reports |
| Queue | BullMQ + Redis | Async notification jobs |
| PDF | Puppeteer | Report cards, invoices |
| QR | `qrcode` npm package | Student ID QR generation |

---

## 3. Authentication & Security

### Token Strategy
```
Access Token:  JWT RS256 · 15 minute TTL
Refresh Token: Opaque · 30 day TTL · stored in HttpOnly cookie + DB
```

### Role Hierarchy
```
SUPER_ADMIN
  └── INSTITUTE_ADMIN
        ├── TEACHER
        ├── STUDENT
        └── PARENT
```

### Request Headers (all protected routes)
```http
Authorization: Bearer <access_token>
X-Institute-ID: inst_abc123          ← required for institute-scoped routes
X-Client-Version: 2.5.0             ← mobile app version tracking
Content-Type: application/json
```

### RBAC Matrix

| Resource | SUPER_ADMIN | INSTITUTE_ADMIN | TEACHER | STUDENT | PARENT |
|----------|:-----------:|:---------------:|:-------:|:-------:|:------:|
| All Institutes | CRUD | own only | — | — | — |
| Students | CRUD | CRUD | R | own | own child |
| Teachers | CRUD | CRUD | own | — | — |
| Attendance | CRUD | CRUD | CRU | R | R (child) |
| Exam Results | CRUD | CRUD | CRU | R | R (child) |
| Payments | CRUD | CRUD | — | R | R (child) |
| Live Sessions | CRUD | CRUD | CRU | R | R (child) |
| Notifications | CRUD | CRUD | CR | R | R |

---

## 4. Standard API Conventions

### Base URL
```
Production:  https://api.smartedupro.lk/api/v1
Staging:     https://staging-api.smartedupro.lk/api/v1
```

### Standard Response Envelope
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully",
  "meta": {
    "page": 1,
    "per_page": 20,
    "total": 420,
    "total_pages": 21,
    "timestamp": "2025-07-09T10:30:00.000Z"
  }
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "STUDENT_NOT_FOUND",
    "message": "Student with ID SE-2401 does not exist",
    "field": "student_id",
    "details": null
  },
  "meta": {
    "timestamp": "2025-07-09T10:30:00.000Z",
    "request_id": "req_7Yd82Kxp"
  }
}
```

### Pagination Query Parameters
```
?page=1&per_page=20&sort_by=created_at&sort_order=desc
?search=kavindi&filter[grade]=AL_SCIENCE&filter[fee_status]=overdue
```

### Date Format
All dates: ISO 8601 — `2025-07-09T10:30:00.000Z`

---

## 5. PostgreSQL Database Schema

### 5.1 Extensions & Configuration
```sql
-- Enable extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";      -- fuzzy search
CREATE EXTENSION IF NOT EXISTS "btree_gin";    -- composite indexes
CREATE EXTENSION IF NOT EXISTS "pgcrypto";     -- hash functions

-- Row-Level Security default
ALTER DATABASE smartedupro SET row_security = on;
```

---

### 5.2 Enums
```sql
CREATE TYPE user_role AS ENUM (
  'SUPER_ADMIN', 'INSTITUTE_ADMIN', 'TEACHER', 'STUDENT', 'PARENT'
);

CREATE TYPE subscription_plan AS ENUM (
  'TRIAL', 'BASIC', 'PRO', 'ENTERPRISE'
);

CREATE TYPE subscription_status AS ENUM (
  'ACTIVE', 'SUSPENDED', 'CANCELLED', 'TRIAL', 'PAST_DUE'
);

CREATE TYPE attendance_status AS ENUM (
  'PRESENT', 'ABSENT', 'LATE', 'EXCUSED', 'HALF_DAY'
);

CREATE TYPE fee_status AS ENUM (
  'PAID', 'PARTIAL', 'OVERDUE', 'WAIVED', 'PENDING'
);

CREATE TYPE payment_method AS ENUM (
  'CASH', 'BANK_TRANSFER', 'ONLINE', 'CHEQUE', 'CARD'
);

CREATE TYPE class_type AS ENUM (
  'IN_PERSON', 'ONLINE', 'HYBRID'
);

CREATE TYPE exam_type AS ENUM (
  'TEST', 'MIDTERM', 'FINAL', 'MOCK', 'ASSIGNMENT', 'PRACTICAL'
);

CREATE TYPE sl_grade AS ENUM (
  'A_PLUS', 'A', 'B', 'C', 'S', 'F', 'AB'
);

CREATE TYPE notification_channel AS ENUM (
  'PUSH', 'SMS', 'WHATSAPP', 'EMAIL'
);

CREATE TYPE notification_type AS ENUM (
  'ATTENDANCE_ALERT', 'EXAM_REMINDER', 'RESULT_PUBLISHED',
  'FEE_DUE', 'FEE_RECEIVED', 'CLASS_CANCELLED', 'CLASS_RESCHEDULED',
  'ANNOUNCEMENT', 'CHAT_MESSAGE', 'LIVE_CLASS_STARTING'
);

CREATE TYPE session_status AS ENUM (
  'SCHEDULED', 'LIVE', 'ENDED', 'CANCELLED'
);

CREATE TYPE day_of_week AS ENUM (
  'MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY',
  'FRIDAY', 'SATURDAY', 'SUNDAY'
);

CREATE TYPE gender AS ENUM ('MALE', 'FEMALE', 'OTHER');

CREATE TYPE al_stream AS ENUM (
  'SCIENCE', 'COMMERCE', 'ARTS', 'TECHNOLOGY', 'BIO_SCIENCE'
);

CREATE TYPE study_level AS ENUM (
  'GRADE_6', 'GRADE_7', 'GRADE_8', 'GRADE_9', 'GRADE_10',
  'OL', 'AL_YEAR_1', 'AL_YEAR_2', 'OTHER'
);
```

---

### 5.3 Core Tables

#### `users`
```sql
CREATE TABLE users (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  email           VARCHAR(255) UNIQUE,
  phone           VARCHAR(20) UNIQUE,                          -- e.g. +94712345678
  password_hash   VARCHAR(255) NOT NULL,
  role            user_role NOT NULL,
  first_name      VARCHAR(100) NOT NULL,
  last_name       VARCHAR(100) NOT NULL,
  avatar_url      VARCHAR(500),
  is_active       BOOLEAN DEFAULT TRUE,
  is_verified     BOOLEAN DEFAULT FALSE,
  last_login_at   TIMESTAMPTZ,
  fcm_token       VARCHAR(500),                                -- Firebase push token
  apns_token      VARCHAR(500),                                -- Apple push token
  preferred_lang  VARCHAR(10) DEFAULT 'en',                   -- 'en', 'si', 'ta'
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  updated_at      TIMESTAMPTZ DEFAULT NOW(),
  deleted_at      TIMESTAMPTZ                                  -- soft delete
);

CREATE INDEX idx_users_email ON users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_phone ON users(phone) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_role ON users(role);
```

#### `refresh_tokens`
```sql
CREATE TABLE refresh_tokens (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id       UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash    VARCHAR(255) NOT NULL UNIQUE,
  device_info   JSONB,                                         -- { device, os, app_version }
  ip_address    INET,
  expires_at    TIMESTAMPTZ NOT NULL,
  revoked_at    TIMESTAMPTZ,
  created_at    TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_hash ON refresh_tokens(token_hash);
```

#### `otp_codes`
```sql
CREATE TABLE otp_codes (
  id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  identifier  VARCHAR(255) NOT NULL,                          -- phone or email
  code        VARCHAR(6) NOT NULL,
  purpose     VARCHAR(50) NOT NULL,                           -- 'LOGIN', 'REGISTER', 'RESET'
  attempts    INTEGER DEFAULT 0,
  verified    BOOLEAN DEFAULT FALSE,
  expires_at  TIMESTAMPTZ NOT NULL,
  created_at  TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_otp_identifier ON otp_codes(identifier, purpose);
```

---

### 5.4 SaaS & Institute Tables

#### `subscription_plans`
```sql
CREATE TABLE subscription_plans (
  id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  name              subscription_plan NOT NULL UNIQUE,
  display_name      VARCHAR(100) NOT NULL,
  price_lkr         DECIMAL(10,2) NOT NULL,
  max_students      INTEGER NOT NULL,
  max_teachers      INTEGER NOT NULL,
  max_branches      INTEGER DEFAULT 1,
  features          JSONB NOT NULL,                           -- feature flags
  is_active         BOOLEAN DEFAULT TRUE,
  created_at        TIMESTAMPTZ DEFAULT NOW()
);

-- Seed data
INSERT INTO subscription_plans VALUES
(uuid_generate_v4(), 'TRIAL',      'Free Trial',  0,      50,   3,  1, '{"qr_attendance":true,"live_class":false,"sms":false}', TRUE, NOW()),
(uuid_generate_v4(), 'BASIC',      'Basic Plan',  15000,  200,  10, 1, '{"qr_attendance":true,"live_class":false,"sms":true}',  TRUE, NOW()),
(uuid_generate_v4(), 'PRO',        'Pro Plan',    35000,  500,  25, 3, '{"qr_attendance":true,"live_class":true,"sms":true,"whatsapp":true}', TRUE, NOW()),
(uuid_generate_v4(), 'ENTERPRISE', 'Enterprise',  85000,  9999, 99, 10,'{"qr_attendance":true,"live_class":true,"sms":true,"whatsapp":true,"custom_domain":true}', TRUE, NOW());
```

#### `institutes`
```sql
CREATE TABLE institutes (
  id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  name                VARCHAR(200) NOT NULL,
  slug                VARCHAR(100) UNIQUE NOT NULL,           -- URL-friendly identifier
  registration_no     VARCHAR(100),                          -- Gov registration
  phone               VARCHAR(20) NOT NULL,
  email               VARCHAR(255) NOT NULL,
  website             VARCHAR(500),
  logo_url            VARCHAR(500),
  address_line1       VARCHAR(255),
  address_line2       VARCHAR(255),
  city                VARCHAR(100),
  district            VARCHAR(100),
  province            VARCHAR(100),
  postal_code         VARCHAR(10),
  owner_name          VARCHAR(200),
  owner_phone         VARCHAR(20),
  owner_email         VARCHAR(255),
  subscription_plan   subscription_plan DEFAULT 'TRIAL',
  subscription_status subscription_status DEFAULT 'TRIAL',
  subscription_start  DATE,
  subscription_end    DATE,
  trial_end           DATE,
  max_students        INTEGER DEFAULT 50,
  max_teachers        INTEGER DEFAULT 3,
  settings            JSONB DEFAULT '{}',                    -- institute-specific config
  is_active           BOOLEAN DEFAULT TRUE,
  created_at          TIMESTAMPTZ DEFAULT NOW(),
  updated_at          TIMESTAMPTZ DEFAULT NOW(),
  deleted_at          TIMESTAMPTZ
);

CREATE INDEX idx_institutes_slug ON institutes(slug);
CREATE INDEX idx_institutes_status ON institutes(subscription_status);
```

#### `branches`
```sql
CREATE TABLE branches (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id  UUID NOT NULL REFERENCES institutes(id) ON DELETE CASCADE,
  name          VARCHAR(200) NOT NULL,
  address       VARCHAR(500),
  city          VARCHAR(100),
  phone         VARCHAR(20),
  manager_id    UUID REFERENCES users(id),
  is_active     BOOLEAN DEFAULT TRUE,
  created_at    TIMESTAMPTZ DEFAULT NOW(),
  updated_at    TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_branches_institute ON branches(institute_id);
```

#### `institute_admins`
```sql
CREATE TABLE institute_admins (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id       UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  institute_id  UUID NOT NULL REFERENCES institutes(id) ON DELETE CASCADE,
  branch_id     UUID REFERENCES branches(id),               -- null = all branches
  permissions   JSONB DEFAULT '{}',
  created_at    TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(user_id, institute_id)
);
```

#### `billing_transactions`
```sql
CREATE TABLE billing_transactions (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  plan            subscription_plan NOT NULL,
  amount_lkr      DECIMAL(10,2) NOT NULL,
  currency        VARCHAR(5) DEFAULT 'LKR',
  payment_method  VARCHAR(50),
  payment_ref     VARCHAR(200),
  status          VARCHAR(50) NOT NULL,                      -- PAID, PENDING, FAILED
  billing_period  DATERANGE NOT NULL,
  invoice_url     VARCHAR(500),
  notes           TEXT,
  created_at      TIMESTAMPTZ DEFAULT NOW()
);
```

---

### 5.5 Academic Structure Tables

#### `subjects`
```sql
CREATE TABLE subjects (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id  UUID NOT NULL REFERENCES institutes(id) ON DELETE CASCADE,
  name          VARCHAR(200) NOT NULL,
  code          VARCHAR(50),                                 -- e.g. 'MATH-AL', 'PHY-AL'
  study_level   study_level NOT NULL,
  al_stream     al_stream,                                   -- only for A/L subjects
  description   TEXT,
  is_active     BOOLEAN DEFAULT TRUE,
  created_at    TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(institute_id, code)
);

CREATE INDEX idx_subjects_institute ON subjects(institute_id);
```

#### `classes`
```sql
CREATE TABLE classes (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID NOT NULL REFERENCES institutes(id) ON DELETE CASCADE,
  branch_id       UUID REFERENCES branches(id),
  name            VARCHAR(200) NOT NULL,                     -- e.g. 'A/L Science Batch 2024'
  code            VARCHAR(50),                               -- e.g. 'CLS-01'
  study_level     study_level NOT NULL,
  al_stream       al_stream,
  academic_year   INTEGER NOT NULL,                          -- e.g. 2024
  class_type      class_type DEFAULT 'IN_PERSON',
  max_students    INTEGER DEFAULT 60,
  description     TEXT,
  is_active       BOOLEAN DEFAULT TRUE,
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  updated_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_classes_institute ON classes(institute_id);
CREATE INDEX idx_classes_year ON classes(academic_year);
```

#### `class_subjects`
```sql
CREATE TABLE class_subjects (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  class_id      UUID NOT NULL REFERENCES classes(id) ON DELETE CASCADE,
  subject_id    UUID NOT NULL REFERENCES subjects(id),
  teacher_id    UUID REFERENCES users(id),
  monthly_fee   DECIMAL(10,2),                              -- per-subject fee override
  created_at    TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(class_id, subject_id)
);
```

---

### 5.6 People Tables

#### `students`
```sql
CREATE TABLE students (
  id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id             UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE UNIQUE,
  institute_id        UUID NOT NULL REFERENCES institutes(id),
  student_no          VARCHAR(20) NOT NULL,                  -- e.g. 'SE-2401' (auto-generated)
  nic_or_birth_cert   VARCHAR(100),
  date_of_birth       DATE,
  gender              gender,
  school_name         VARCHAR(200),
  home_address        VARCHAR(500),
  city                VARCHAR(100),
  district            VARCHAR(100),
  guardian_name       VARCHAR(200),
  guardian_phone      VARCHAR(20),
  guardian_relation   VARCHAR(50),
  photo_url           VARCHAR(500),
  qr_code_url         VARCHAR(500),                         -- pre-generated QR image URL
  qr_secret           VARCHAR(255) NOT NULL,                -- HMAC secret for QR verification
  notes               TEXT,
  enrolled_at         DATE DEFAULT CURRENT_DATE,
  is_active           BOOLEAN DEFAULT TRUE,
  created_at          TIMESTAMPTZ DEFAULT NOW(),
  updated_at          TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(institute_id, student_no)
);

CREATE INDEX idx_students_institute ON students(institute_id);
CREATE INDEX idx_students_no ON students(student_no);
CREATE INDEX idx_students_user ON students(user_id);
```

#### `student_class_enrollments`
```sql
CREATE TABLE student_class_enrollments (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id      UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE,
  class_id        UUID NOT NULL REFERENCES classes(id),
  enrolled_date   DATE DEFAULT CURRENT_DATE,
  left_date       DATE,
  fee_package_id  UUID,                                     -- ref to fee_packages
  is_active       BOOLEAN DEFAULT TRUE,
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(student_id, class_id)
);

CREATE INDEX idx_enrollments_student ON student_class_enrollments(student_id);
CREATE INDEX idx_enrollments_class ON student_class_enrollments(class_id);
```

#### `teachers`
```sql
CREATE TABLE teachers (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE UNIQUE,
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  teacher_no      VARCHAR(20),                               -- e.g. 'TCH-001'
  nic             VARCHAR(100),
  date_of_birth   DATE,
  gender          gender,
  qualification   VARCHAR(500),
  specialization  VARCHAR(200),
  experience_yrs  INTEGER DEFAULT 0,
  photo_url       VARCHAR(500),
  bank_name       VARCHAR(200),
  bank_account    VARCHAR(50),
  salary_lkr      DECIMAL(10,2),
  joined_date     DATE DEFAULT CURRENT_DATE,
  is_active       BOOLEAN DEFAULT TRUE,
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  updated_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_teachers_institute ON teachers(institute_id);
```

#### `teacher_class_assignments`
```sql
CREATE TABLE teacher_class_assignments (
  id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  teacher_id  UUID NOT NULL REFERENCES teachers(id) ON DELETE CASCADE,
  class_id    UUID NOT NULL REFERENCES classes(id),
  subject_id  UUID NOT NULL REFERENCES subjects(id),
  is_primary  BOOLEAN DEFAULT TRUE,
  from_date   DATE DEFAULT CURRENT_DATE,
  to_date     DATE,
  created_at  TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(teacher_id, class_id, subject_id)
);
```

#### `parents`
```sql
CREATE TABLE parents (
  id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id           UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE UNIQUE,
  institute_id      UUID NOT NULL REFERENCES institutes(id),
  occupation        VARCHAR(200),
  whatsapp_no       VARCHAR(20),
  alt_phone         VARCHAR(20),
  address           VARCHAR(500),
  preferred_channel notification_channel DEFAULT 'WHATSAPP',
  created_at        TIMESTAMPTZ DEFAULT NOW()
);
```

#### `student_parents`
```sql
CREATE TABLE student_parents (
  id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id  UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE,
  parent_id   UUID NOT NULL REFERENCES parents(id) ON DELETE CASCADE,
  relation    VARCHAR(50) NOT NULL,                         -- 'MOTHER', 'FATHER', 'GUARDIAN'
  is_primary  BOOLEAN DEFAULT FALSE,
  created_at  TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(student_id, parent_id)
);
```

---

### 5.7 Attendance Tables

#### `attendance_sessions`
```sql
CREATE TABLE attendance_sessions (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  class_id        UUID NOT NULL REFERENCES classes(id),
  subject_id      UUID NOT NULL REFERENCES subjects(id),
  teacher_id      UUID NOT NULL REFERENCES teachers(id),
  session_date    DATE NOT NULL,
  start_time      TIME NOT NULL,
  end_time        TIME,
  is_completed    BOOLEAN DEFAULT FALSE,
  total_present   INTEGER DEFAULT 0,
  total_absent    INTEGER DEFAULT 0,
  total_late      INTEGER DEFAULT 0,
  notes           TEXT,
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  updated_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_att_sessions_class ON attendance_sessions(class_id, session_date);
CREATE INDEX idx_att_sessions_date ON attendance_sessions(session_date);
CREATE INDEX idx_att_sessions_teacher ON attendance_sessions(teacher_id);
```

#### `attendance_records`
```sql
CREATE TABLE attendance_records (
  id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  session_id          UUID NOT NULL REFERENCES attendance_sessions(id) ON DELETE CASCADE,
  student_id          UUID NOT NULL REFERENCES students(id),
  status              attendance_status NOT NULL DEFAULT 'ABSENT',
  check_in_time       TIMESTAMPTZ,
  check_out_time      TIMESTAMPTZ,
  marked_via          VARCHAR(50) DEFAULT 'QR',             -- 'QR', 'MANUAL', 'SYSTEM'
  qr_scan_log         JSONB,                                -- { device, location, raw_token }
  late_minutes        INTEGER DEFAULT 0,
  parent_notified     BOOLEAN DEFAULT FALSE,
  notified_at         TIMESTAMPTZ,
  notes               TEXT,
  created_at          TIMESTAMPTZ DEFAULT NOW(),
  updated_at          TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(session_id, student_id)
);

CREATE INDEX idx_att_records_session ON attendance_records(session_id);
CREATE INDEX idx_att_records_student ON attendance_records(student_id);
CREATE INDEX idx_att_records_date ON attendance_records(created_at::DATE);
```

#### `student_qr_tokens`
```sql
CREATE TABLE student_qr_tokens (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id    UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE UNIQUE,
  token         VARCHAR(500) NOT NULL UNIQUE,               -- signed JWT or HMAC token
  qr_data       TEXT NOT NULL,                              -- encoded QR payload
  expires_at    TIMESTAMPTZ,                                -- null = non-expiring
  last_used_at  TIMESTAMPTZ,
  created_at    TIMESTAMPTZ DEFAULT NOW(),
  updated_at    TIMESTAMPTZ DEFAULT NOW()
);
```

---

### 5.8 Exam & Results Tables

#### `exams`
```sql
CREATE TABLE exams (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  class_id        UUID NOT NULL REFERENCES classes(id),
  subject_id      UUID NOT NULL REFERENCES subjects(id),
  teacher_id      UUID NOT NULL REFERENCES teachers(id),
  title           VARCHAR(300) NOT NULL,                    -- e.g. 'Test 4 — June 2025'
  exam_type       exam_type DEFAULT 'TEST',
  exam_date       DATE NOT NULL,
  start_time      TIME,
  duration_mins   INTEGER,
  total_marks     DECIMAL(6,2) NOT NULL DEFAULT 100,
  pass_marks      DECIMAL(6,2) DEFAULT 40,
  description     TEXT,
  instructions    TEXT,
  is_published    BOOLEAN DEFAULT FALSE,
  published_at    TIMESTAMPTZ,
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  updated_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_exams_class ON exams(class_id);
CREATE INDEX idx_exams_subject ON exams(subject_id);
CREATE INDEX idx_exams_date ON exams(exam_date);
```

#### `exam_results`
```sql
CREATE TABLE exam_results (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  exam_id         UUID NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
  student_id      UUID NOT NULL REFERENCES students(id),
  marks_obtained  DECIMAL(6,2),
  grade           sl_grade,
  rank_in_class   INTEGER,
  is_absent       BOOLEAN DEFAULT FALSE,
  teacher_notes   TEXT,
  parent_notified BOOLEAN DEFAULT FALSE,
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  updated_at      TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(exam_id, student_id)
);

CREATE INDEX idx_results_exam ON exam_results(exam_id);
CREATE INDEX idx_results_student ON exam_results(student_id);
```

---

### 5.9 Timetable Tables

#### `timetable_slots`
```sql
CREATE TABLE timetable_slots (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  branch_id       UUID REFERENCES branches(id),
  class_id        UUID NOT NULL REFERENCES classes(id),
  subject_id      UUID NOT NULL REFERENCES subjects(id),
  teacher_id      UUID NOT NULL REFERENCES teachers(id),
  day_of_week     day_of_week NOT NULL,
  start_time      TIME NOT NULL,
  end_time        TIME NOT NULL,
  room            VARCHAR(100),
  class_type      class_type DEFAULT 'IN_PERSON',
  effective_from  DATE DEFAULT CURRENT_DATE,
  effective_to    DATE,
  is_active       BOOLEAN DEFAULT TRUE,
  created_at      TIMESTAMPTZ DEFAULT NOW(),
  updated_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_timetable_class ON timetable_slots(class_id);
CREATE INDEX idx_timetable_teacher ON timetable_slots(teacher_id);
CREATE INDEX idx_timetable_day ON timetable_slots(day_of_week);
```

#### `timetable_overrides`
```sql
CREATE TABLE timetable_overrides (
  id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  timetable_slot_id UUID NOT NULL REFERENCES timetable_slots(id),
  override_date     DATE NOT NULL,
  action            VARCHAR(50) NOT NULL,                   -- 'CANCELLED', 'RESCHEDULED'
  new_start_time    TIME,
  new_end_time      TIME,
  new_room          VARCHAR(100),
  reason            TEXT,
  notify_students   BOOLEAN DEFAULT TRUE,
  created_by        UUID REFERENCES users(id),
  created_at        TIMESTAMPTZ DEFAULT NOW()
);
```

---

### 5.10 Finance Tables

#### `fee_packages`
```sql
CREATE TABLE fee_packages (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  name            VARCHAR(200) NOT NULL,                    -- e.g. 'A/L Science Full Package'
  class_id        UUID REFERENCES classes(id),             -- null = generic package
  amount_lkr      DECIMAL(10,2) NOT NULL,
  billing_cycle   VARCHAR(20) DEFAULT 'MONTHLY',           -- 'MONTHLY', 'TERM', 'ANNUAL'
  due_day         INTEGER DEFAULT 5,                        -- day of month fee is due
  late_fee_lkr    DECIMAL(10,2) DEFAULT 0,
  description     TEXT,
  is_active       BOOLEAN DEFAULT TRUE,
  created_at      TIMESTAMPTZ DEFAULT NOW()
);
```

#### `student_fee_records`
```sql
CREATE TABLE student_fee_records (
  id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id        UUID NOT NULL REFERENCES students(id),
  institute_id      UUID NOT NULL REFERENCES institutes(id),
  fee_package_id    UUID NOT NULL REFERENCES fee_packages(id),
  billing_month     DATE NOT NULL,                          -- first day of month, e.g. 2025-06-01
  amount_due        DECIMAL(10,2) NOT NULL,
  amount_paid       DECIMAL(10,2) DEFAULT 0,
  late_fee          DECIMAL(10,2) DEFAULT 0,
  discount          DECIMAL(10,2) DEFAULT 0,
  balance           DECIMAL(10,2) GENERATED ALWAYS AS (amount_due + late_fee - discount - amount_paid) STORED,
  fee_status        fee_status DEFAULT 'PENDING',
  due_date          DATE NOT NULL,
  reminder_count    INTEGER DEFAULT 0,
  last_reminder_at  TIMESTAMPTZ,
  notes             TEXT,
  created_at        TIMESTAMPTZ DEFAULT NOW(),
  updated_at        TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(student_id, fee_package_id, billing_month)
);

CREATE INDEX idx_fee_records_student ON student_fee_records(student_id);
CREATE INDEX idx_fee_records_status ON student_fee_records(fee_status);
CREATE INDEX idx_fee_records_month ON student_fee_records(billing_month);
```

#### `fee_payments`
```sql
CREATE TABLE fee_payments (
  id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  fee_record_id     UUID NOT NULL REFERENCES student_fee_records(id),
  student_id        UUID NOT NULL REFERENCES students(id),
  institute_id      UUID NOT NULL REFERENCES institutes(id),
  amount_lkr        DECIMAL(10,2) NOT NULL,
  payment_method    payment_method NOT NULL,
  payment_ref       VARCHAR(200),                           -- bank slip / online ref
  payment_date      DATE NOT NULL DEFAULT CURRENT_DATE,
  received_by       UUID REFERENCES users(id),
  receipt_no        VARCHAR(100) UNIQUE,
  receipt_url       VARCHAR(500),
  notes             TEXT,
  created_at        TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_payments_fee_record ON fee_payments(fee_record_id);
CREATE INDEX idx_payments_student ON fee_payments(student_id);
CREATE INDEX idx_payments_date ON fee_payments(payment_date);
```

#### `invoices`
```sql
CREATE TABLE invoices (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  invoice_no      VARCHAR(100) UNIQUE NOT NULL,
  student_id      UUID NOT NULL REFERENCES students(id),
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  fee_record_id   UUID REFERENCES student_fee_records(id),
  amount_lkr      DECIMAL(10,2) NOT NULL,
  issued_date     DATE NOT NULL DEFAULT CURRENT_DATE,
  due_date        DATE NOT NULL,
  pdf_url         VARCHAR(500),
  sent_via        JSONB,                                    -- channels invoice was sent
  created_at      TIMESTAMPTZ DEFAULT NOW()
);
```

---

### 5.11 Online Learning Tables

#### `live_sessions`
```sql
CREATE TABLE live_sessions (
  id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id      UUID NOT NULL REFERENCES institutes(id),
  class_id          UUID NOT NULL REFERENCES classes(id),
  subject_id        UUID NOT NULL REFERENCES subjects(id),
  teacher_id        UUID NOT NULL REFERENCES teachers(id),
  title             VARCHAR(300),
  timetable_slot_id UUID REFERENCES timetable_slots(id),
  scheduled_start   TIMESTAMPTZ NOT NULL,
  scheduled_end     TIMESTAMPTZ,
  actual_start      TIMESTAMPTZ,
  actual_end        TIMESTAMPTZ,
  status            session_status DEFAULT 'SCHEDULED',
  room_id           VARCHAR(200),                           -- Agora channel / LiveKit room
  room_token        VARCHAR(500),
  join_url          VARCHAR(500),
  recording_url     VARCHAR(500),
  participant_count INTEGER DEFAULT 0,
  peak_viewers      INTEGER DEFAULT 0,
  chat_enabled      BOOLEAN DEFAULT TRUE,
  recording_enabled BOOLEAN DEFAULT TRUE,
  whiteboard_data   JSONB,
  created_at        TIMESTAMPTZ DEFAULT NOW(),
  updated_at        TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_live_sessions_class ON live_sessions(class_id);
CREATE INDEX idx_live_sessions_teacher ON live_sessions(teacher_id);
CREATE INDEX idx_live_sessions_status ON live_sessions(status);
```

#### `live_session_participants`
```sql
CREATE TABLE live_session_participants (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  session_id    UUID NOT NULL REFERENCES live_sessions(id) ON DELETE CASCADE,
  user_id       UUID NOT NULL REFERENCES users(id),
  role          VARCHAR(20) NOT NULL,                       -- 'HOST', 'STUDENT', 'PARENT'
  joined_at     TIMESTAMPTZ,
  left_at       TIMESTAMPTZ,
  duration_secs INTEGER DEFAULT 0,
  UNIQUE(session_id, user_id)
);

CREATE INDEX idx_participants_session ON live_session_participants(session_id);
```

#### `recordings`
```sql
CREATE TABLE recordings (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id  UUID NOT NULL REFERENCES institutes(id),
  session_id    UUID REFERENCES live_sessions(id),
  class_id      UUID REFERENCES classes(id),
  subject_id    UUID REFERENCES subjects(id),
  teacher_id    UUID REFERENCES teachers(id),
  title         VARCHAR(300) NOT NULL,
  description   TEXT,
  duration_secs INTEGER,
  file_url      VARCHAR(500) NOT NULL,
  thumbnail_url VARCHAR(500),
  file_size_mb  DECIMAL(8,2),
  view_count    INTEGER DEFAULT 0,
  is_published  BOOLEAN DEFAULT TRUE,
  recorded_at   TIMESTAMPTZ DEFAULT NOW(),
  created_at    TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_recordings_class ON recordings(class_id);
```

#### `learning_resources`
```sql
CREATE TABLE learning_resources (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id  UUID NOT NULL REFERENCES institutes(id),
  class_id      UUID REFERENCES classes(id),
  subject_id    UUID REFERENCES subjects(id),
  uploaded_by   UUID NOT NULL REFERENCES users(id),
  title         VARCHAR(300) NOT NULL,
  description   TEXT,
  file_type     VARCHAR(20),                                -- 'PDF', 'DOC', 'VIDEO', 'IMAGE'
  file_url      VARCHAR(500) NOT NULL,
  file_size_mb  DECIMAL(8,2),
  download_count INTEGER DEFAULT 0,
  is_published  BOOLEAN DEFAULT TRUE,
  created_at    TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_resources_class ON learning_resources(class_id);
CREATE INDEX idx_resources_subject ON learning_resources(subject_id);
```

---

### 5.12 Communication Tables

#### `announcements`
```sql
CREATE TABLE announcements (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID NOT NULL REFERENCES institutes(id),
  author_id       UUID NOT NULL REFERENCES users(id),
  title           VARCHAR(300) NOT NULL,
  body            TEXT NOT NULL,
  target_class_id UUID REFERENCES classes(id),             -- null = all classes
  target_role     user_role,                               -- null = all roles
  is_pinned       BOOLEAN DEFAULT FALSE,
  send_push       BOOLEAN DEFAULT TRUE,
  send_sms        BOOLEAN DEFAULT FALSE,
  send_whatsapp   BOOLEAN DEFAULT FALSE,
  published_at    TIMESTAMPTZ DEFAULT NOW(),
  expires_at      TIMESTAMPTZ,
  created_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_announcements_institute ON announcements(institute_id);
CREATE INDEX idx_announcements_class ON announcements(target_class_id);
```

#### `messages`
```sql
CREATE TABLE messages (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id  UUID NOT NULL REFERENCES institutes(id),
  sender_id     UUID NOT NULL REFERENCES users(id),
  recipient_id  UUID NOT NULL REFERENCES users(id),
  body          TEXT NOT NULL,
  is_read       BOOLEAN DEFAULT FALSE,
  read_at       TIMESTAMPTZ,
  attachment_url VARCHAR(500),
  created_at    TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_messages_sender ON messages(sender_id, recipient_id, created_at DESC);
CREATE INDEX idx_messages_recipient ON messages(recipient_id, is_read);
```

#### `notifications`
```sql
CREATE TABLE notifications (
  id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id    UUID REFERENCES institutes(id),
  recipient_id    UUID NOT NULL REFERENCES users(id),
  type            notification_type NOT NULL,
  title           VARCHAR(300) NOT NULL,
  body            TEXT NOT NULL,
  data            JSONB DEFAULT '{}',                      -- action payload
  channel         notification_channel NOT NULL,
  is_read         BOOLEAN DEFAULT FALSE,
  read_at         TIMESTAMPTZ,
  is_delivered    BOOLEAN DEFAULT FALSE,
  delivered_at    TIMESTAMPTZ,
  delivery_error  TEXT,
  created_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_notifications_recipient ON notifications(recipient_id, is_read, created_at DESC);
CREATE INDEX idx_notifications_type ON notifications(type, created_at DESC);
```

#### `notification_templates`
```sql
CREATE TABLE notification_templates (
  id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  institute_id  UUID REFERENCES institutes(id),             -- null = system default
  type          notification_type NOT NULL,
  channel       notification_channel NOT NULL,
  title_template VARCHAR(300) NOT NULL,
  body_template  TEXT NOT NULL,                            -- uses {{variable}} syntax
  is_active     BOOLEAN DEFAULT TRUE,
  created_at    TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(institute_id, type, channel)
);
```

---

## 6. API Endpoints — Auth

### POST `/auth/register`
Register a new user (used by institute admin onboarding).

**Auth:** Public (rate-limited: 5/hour per IP)

**Request Body:**
```json
{
  "email": "admin@jayasingheacademy.lk",
  "phone": "+94712345678",
  "password": "SecurePass@2025",
  "first_name": "Jayasinghe",
  "last_name": "Perera",
  "role": "INSTITUTE_ADMIN",
  "institute_name": "Jayasinghe Academy",
  "institute_city": "Colombo"
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "admin@jayasingheacademy.lk",
      "first_name": "Jayasinghe",
      "last_name": "Perera",
      "role": "INSTITUTE_ADMIN",
      "is_verified": false
    },
    "institute": {
      "id": "inst_abc123",
      "name": "Jayasinghe Academy",
      "slug": "jayasinghe-academy",
      "subscription_plan": "TRIAL",
      "trial_end": "2025-08-09"
    }
  },
  "message": "Registration successful. Check your phone for OTP verification."
}
```

---

### POST `/auth/login`
Login with email/phone + password.

**Auth:** Public

**Request Body:**
```json
{
  "identifier": "+94712345678",
  "password": "SecurePass@2025",
  "device_info": {
    "device": "iPhone 15",
    "os": "iOS 17.4",
    "app_version": "2.5.0"
  }
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type": "Bearer",
    "expires_in": 900,
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "admin@jayasingheacademy.lk",
      "phone": "+94712345678",
      "first_name": "Jayasinghe",
      "last_name": "Perera",
      "role": "INSTITUTE_ADMIN",
      "avatar_url": null,
      "institute_id": "inst_abc123",
      "institute_name": "Jayasinghe Academy"
    }
  }
}
```
> Refresh token is set as `HttpOnly` cookie: `smartedupro_rt`

---

### POST `/auth/login/otp/request`
Request OTP for mobile login (students/parents).

**Request Body:**
```json
{ "phone": "+94762345678", "purpose": "LOGIN" }
```

**Response 200:**
```json
{
  "success": true,
  "data": { "expires_in": 300, "masked_phone": "+9476****678" },
  "message": "OTP sent to your registered WhatsApp number"
}
```

---

### POST `/auth/login/otp/verify`
Verify OTP and get tokens.

**Request Body:**
```json
{ "phone": "+94762345678", "otp": "482917", "purpose": "LOGIN" }
```

**Response 200:** *(same as `/auth/login` response)*

---

### POST `/auth/refresh`
Get new access token using refresh token cookie.

**Auth:** Refresh token in HttpOnly cookie

**Response 200:**
```json
{
  "success": true,
  "data": {
    "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expires_in": 900
  }
}
```

---

### POST `/auth/logout`
**Auth:** Bearer token

**Response 200:**
```json
{ "success": true, "message": "Logged out successfully" }
```

---

### GET `/auth/me`
Get current user profile.

**Auth:** Bearer token

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "admin@jayasingheacademy.lk",
    "phone": "+94712345678",
    "first_name": "Jayasinghe",
    "last_name": "Perera",
    "role": "INSTITUTE_ADMIN",
    "avatar_url": "https://cdn.smartedupro.lk/avatars/abc123.jpg",
    "is_verified": true,
    "last_login_at": "2025-07-09T08:30:00.000Z",
    "institute": {
      "id": "inst_abc123",
      "name": "Jayasinghe Academy",
      "subscription_plan": "PRO",
      "subscription_status": "ACTIVE"
    }
  }
}
```

---

## 7. API Endpoints — Super Admin

**All routes require:** `role: SUPER_ADMIN`

### GET `/admin/stats`
Platform-wide analytics.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "total_institutes": 47,
    "active_institutes": 45,
    "trial_institutes": 5,
    "total_students": 12380,
    "total_teachers": 284,
    "total_classes": 892,
    "revenue": {
      "current_month_lkr": 392000,
      "last_month_lkr": 378000,
      "growth_percent": 3.7
    },
    "plan_distribution": {
      "TRIAL": 5,
      "BASIC": 14,
      "PRO": 23,
      "ENTERPRISE": 5
    },
    "new_institutes_this_month": 3,
    "active_live_sessions": 2
  }
}
```

---

### GET `/admin/institutes`
List all institutes with filters.

**Query Params:** `?plan=PRO&status=ACTIVE&page=1&per_page=20&search=jayasinghe`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "inst_abc123",
      "name": "Jayasinghe Academy",
      "slug": "jayasinghe-academy",
      "city": "Colombo 07",
      "subscription_plan": "ENTERPRISE",
      "subscription_status": "ACTIVE",
      "subscription_end": "2026-06-30",
      "total_students": 420,
      "total_teachers": 12,
      "monthly_revenue_lkr": 85000,
      "created_at": "2023-01-15T00:00:00.000Z",
      "owner_name": "Mr. Jayasinghe Perera",
      "owner_phone": "+94712345678"
    },
    {
      "id": "inst_def456",
      "name": "Sunrise Tuition Centre",
      "slug": "sunrise-tuition-kandy",
      "city": "Kandy",
      "subscription_plan": "PRO",
      "subscription_status": "ACTIVE",
      "subscription_end": "2026-05-31",
      "total_students": 310,
      "total_teachers": 8,
      "monthly_revenue_lkr": 35000,
      "created_at": "2023-03-20T00:00:00.000Z"
    }
  ],
  "meta": { "page": 1, "per_page": 20, "total": 47, "total_pages": 3 }
}
```

---

### POST `/admin/institutes`
Create new institute.

**Request Body:**
```json
{
  "name": "Future Stars Institute",
  "phone": "+94912345678",
  "email": "info@futurestars.lk",
  "city": "Galle",
  "district": "Galle",
  "owner_name": "Ms. Priya Senanayake",
  "owner_phone": "+94772345678",
  "owner_email": "priya@futurestars.lk",
  "subscription_plan": "BASIC",
  "trial_days": 30
}
```

---

### PATCH `/admin/institutes/:institute_id/subscription`
Update institute subscription.

**Request Body:**
```json
{
  "plan": "PRO",
  "billing_months": 12,
  "discount_percent": 10,
  "notes": "Annual renewal discount applied"
}
```

---

### GET `/admin/activity-log`
System-wide activity log.

**Query Params:** `?type=PAYMENT&from=2025-07-01&to=2025-07-09&page=1`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "log_001",
      "action": "INSTITUTE_REGISTERED",
      "description": "New institute: Scholar Hub — Kurunegala",
      "actor": "System",
      "target_type": "institute",
      "target_id": "inst_ghi789",
      "ip_address": "203.143.xx.xx",
      "created_at": "2025-07-09T10:05:00.000Z"
    },
    {
      "id": "log_002",
      "action": "SUBSCRIPTION_UPGRADED",
      "description": "Sunrise Academy: BASIC → ENTERPRISE",
      "actor": "admin@smartedupro.lk",
      "target_type": "institute",
      "target_id": "inst_def456",
      "created_at": "2025-07-09T09:48:00.000Z"
    }
  ]
}
```

---

## 8. API Endpoints — Institutes & Branches

**Header required:** `X-Institute-ID: inst_abc123`

### GET `/institutes/me`
Get current institute details.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "inst_abc123",
    "name": "Jayasinghe Academy",
    "slug": "jayasinghe-academy",
    "phone": "+94112345678",
    "email": "info@jayasingheacademy.lk",
    "logo_url": "https://cdn.smartedupro.lk/logos/inst_abc123.png",
    "address_line1": "23, Temple Road",
    "city": "Colombo 07",
    "district": "Colombo",
    "subscription_plan": "ENTERPRISE",
    "subscription_status": "ACTIVE",
    "subscription_start": "2024-07-01",
    "subscription_end": "2026-06-30",
    "max_students": 9999,
    "stats": {
      "total_students": 420,
      "total_teachers": 12,
      "total_classes": 18,
      "total_branches": 2
    }
  }
}
```

---

### GET `/institutes/me/branches`
**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "branch_001",
      "name": "Main Branch — Colombo 07",
      "city": "Colombo 07",
      "phone": "+94112345678",
      "student_count": 320,
      "is_active": true
    },
    {
      "id": "branch_002",
      "name": "Nugegoda Branch",
      "city": "Nugegoda",
      "phone": "+94114567890",
      "student_count": 100,
      "is_active": true
    }
  ]
}
```

---

## 9. API Endpoints — Students

### GET `/students`
List students for institute.

**Auth:** INSTITUTE_ADMIN, TEACHER (own classes only)

**Query Params:** `?class_id=cls_01&fee_status=overdue&search=kavindi&page=1&per_page=20&sort_by=student_no`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "stu_001",
      "student_no": "SE-2401",
      "first_name": "Kavindi",
      "last_name": "Perera",
      "full_name": "Kavindi Perera",
      "phone": "+94712345678",
      "email": "kavindi@gmail.com",
      "photo_url": "https://cdn.smartedupro.lk/photos/stu_001.jpg",
      "grade": "AL_SCIENCE",
      "al_stream": "SCIENCE",
      "batch_year": 2024,
      "attendance_rate": 94.2,
      "gpa": 3.8,
      "fee_status": "PAID",
      "enrolled_classes": ["A/L Science 2024"],
      "is_active": true,
      "created_at": "2024-01-10T00:00:00.000Z"
    },
    {
      "id": "stu_002",
      "student_no": "SE-2402",
      "first_name": "Sachith",
      "last_name": "Fernando",
      "full_name": "Sachith Fernando",
      "phone": "+94773456789",
      "email": "sachith@gmail.com",
      "photo_url": null,
      "grade": "OL",
      "batch_year": 2024,
      "attendance_rate": 87.0,
      "gpa": 3.2,
      "fee_status": "PARTIAL",
      "enrolled_classes": ["O/L Batch 2024"],
      "is_active": true
    }
  ],
  "meta": { "page": 1, "per_page": 20, "total": 420, "total_pages": 21 }
}
```

---

### POST `/students`
Register new student.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "first_name": "Kavindi",
  "last_name": "Perera",
  "date_of_birth": "2007-03-15",
  "gender": "FEMALE",
  "nic_or_birth_cert": "200701502345",
  "phone": "+94712345678",
  "email": "kavindi@gmail.com",
  "school_name": "Visakha Vidyalaya",
  "home_address": "23/A, Temple Road",
  "city": "Colombo 05",
  "district": "Colombo",
  "guardian_name": "Mrs. Kumari Perera",
  "guardian_phone": "+94778912345",
  "guardian_relation": "MOTHER",
  "class_ids": ["cls_001"],
  "fee_package_id": "fpkg_001",
  "branch_id": "branch_001",
  "enrolled_at": "2024-01-10",
  "send_sms_credentials": true
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "stu_001",
    "student_no": "SE-2401",
    "first_name": "Kavindi",
    "last_name": "Perera",
    "qr_code_url": "https://cdn.smartedupro.lk/qrcodes/SE-2401.png",
    "login_credentials": {
      "phone": "+94712345678",
      "temp_password": "Kav@2024"
    },
    "sms_sent": true
  },
  "message": "Student registered. Login credentials sent via SMS."
}
```

---

### GET `/students/:student_id`
Get student full profile.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "stu_001",
    "student_no": "SE-2401",
    "first_name": "Kavindi",
    "last_name": "Perera",
    "date_of_birth": "2007-03-15",
    "gender": "FEMALE",
    "phone": "+94712345678",
    "email": "kavindi@gmail.com",
    "school_name": "Visakha Vidyalaya",
    "photo_url": "https://cdn.smartedupro.lk/photos/stu_001.jpg",
    "qr_code_url": "https://cdn.smartedupro.lk/qrcodes/SE-2401.png",
    "address": {
      "line1": "23/A, Temple Road",
      "city": "Colombo 05",
      "district": "Colombo"
    },
    "guardian": {
      "name": "Mrs. Kumari Perera",
      "phone": "+94778912345",
      "relation": "MOTHER"
    },
    "enrolled_classes": [
      {
        "class_id": "cls_001",
        "class_name": "A/L Science 2024",
        "subjects": ["Mathematics", "Physics", "Chemistry"],
        "enrolled_date": "2024-01-10",
        "fee_package": "Full Package — LKR 9,800/month"
      }
    ],
    "stats": {
      "overall_attendance": 94.2,
      "current_month_attendance": 96.0,
      "gpa": 3.8,
      "class_rank": 4,
      "fee_status": "PAID",
      "current_month_balance": 0
    },
    "parents": [
      {
        "id": "par_001",
        "name": "Mrs. Kumari Perera",
        "phone": "+94778912345",
        "whatsapp": "+94778912345",
        "relation": "MOTHER",
        "is_primary": true
      }
    ],
    "enrolled_at": "2024-01-10",
    "is_active": true
  }
}
```

---

### PATCH `/students/:student_id`
Update student details.

**Auth:** INSTITUTE_ADMIN

**Request Body:** *(partial — any fields)*
```json
{
  "phone": "+94712999888",
  "school_name": "Visakha Vidyalaya, Colombo 5",
  "guardian_phone": "+94779876543"
}
```

---

### DELETE `/students/:student_id`
Soft-delete student.

**Auth:** INSTITUTE_ADMIN

**Response 200:**
```json
{ "success": true, "message": "Student SE-2401 deactivated successfully" }
```

---

### GET `/students/:student_id/attendance-summary`
Get attendance summary for a student.

**Query Params:** `?from=2025-01-01&to=2025-07-09&class_id=cls_001`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "student_id": "stu_001",
    "student_name": "Kavindi Perera",
    "period": { "from": "2025-01-01", "to": "2025-07-09" },
    "summary": {
      "total_sessions": 120,
      "present": 113,
      "absent": 4,
      "late": 3,
      "attendance_rate": 94.2
    },
    "by_subject": [
      {
        "subject": "Mathematics",
        "total": 48,
        "present": 46,
        "absent": 1,
        "late": 1,
        "rate": 95.8
      },
      {
        "subject": "Physics",
        "total": 36,
        "present": 33,
        "absent": 2,
        "late": 1,
        "rate": 91.7
      }
    ],
    "monthly_trend": [
      { "month": "2025-01", "rate": 85.0 },
      { "month": "2025-02", "rate": 87.5 },
      { "month": "2025-03", "rate": 91.3 },
      { "month": "2025-04", "rate": 88.0 },
      { "month": "2025-05", "rate": 93.3 },
      { "month": "2025-06", "rate": 94.0 }
    ]
  }
}
```

---

### GET `/students/:student_id/exam-results`
Get all exam results for a student.

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "exam_id": "exam_001",
      "exam_title": "Test 4 — June 2025",
      "exam_type": "TEST",
      "exam_date": "2025-06-09",
      "subject": "Mathematics",
      "marks_obtained": 88,
      "total_marks": 100,
      "percentage": 88.0,
      "grade": "A",
      "rank_in_class": 4,
      "class_average": 79.6,
      "class_size": 42
    },
    {
      "exam_id": "exam_002",
      "exam_title": "Test 3 — May 2025",
      "exam_type": "TEST",
      "exam_date": "2025-05-15",
      "subject": "Physics",
      "marks_obtained": 76,
      "total_marks": 100,
      "percentage": 76.0,
      "grade": "B",
      "rank_in_class": 11,
      "class_average": 73.2
    }
  ]
}
```

---

### GET `/students/:student_id/fee-history`
**Response 200:**
```json
{
  "success": true,
  "data": {
    "student_id": "stu_001",
    "student_name": "Kavindi Perera",
    "fee_package": "A/L Full Package",
    "monthly_amount": 9800,
    "fee_records": [
      {
        "id": "fee_rec_001",
        "billing_month": "2025-06-01",
        "amount_due": 9800,
        "amount_paid": 9800,
        "late_fee": 0,
        "balance": 0,
        "status": "PAID",
        "due_date": "2025-06-05",
        "payments": [
          {
            "payment_date": "2025-06-02",
            "amount": 9800,
            "method": "ONLINE",
            "ref": "TXN20250602KP",
            "receipt_url": "https://cdn.smartedupro.lk/receipts/rcpt_001.pdf"
          }
        ]
      },
      {
        "id": "fee_rec_002",
        "billing_month": "2025-05-01",
        "amount_due": 9800,
        "amount_paid": 9800,
        "balance": 0,
        "status": "PAID",
        "due_date": "2025-05-05"
      }
    ]
  }
}
```

---

## 10. API Endpoints — Teachers

### GET `/teachers`
List teachers for institute.

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "tch_001",
      "teacher_no": "TCH-001",
      "first_name": "Ravindu",
      "last_name": "Tharaka",
      "full_name": "Mr. Ravindu Tharaka",
      "phone": "+94714567890",
      "email": "ravindu@jayasingheacademy.lk",
      "photo_url": "https://cdn.smartedupro.lk/photos/tch_001.jpg",
      "specialization": "Mathematics & Statistics",
      "experience_yrs": 8,
      "total_classes": 4,
      "total_students": 165,
      "subjects": ["Mathematics", "Statistics"],
      "is_active": true
    },
    {
      "id": "tch_002",
      "teacher_no": "TCH-002",
      "full_name": "Ms. Priya Mendis",
      "phone": "+94726789012",
      "email": "priya@jayasingheacademy.lk",
      "specialization": "Physics",
      "experience_yrs": 6,
      "total_classes": 2,
      "total_students": 76,
      "subjects": ["Physics"],
      "is_active": true
    }
  ],
  "meta": { "total": 12 }
}
```

---

### POST `/teachers`
Register a new teacher.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "first_name": "Ravindu",
  "last_name": "Tharaka",
  "email": "ravindu@jayasingheacademy.lk",
  "phone": "+94714567890",
  "gender": "MALE",
  "nic": "198901502344",
  "date_of_birth": "1989-01-15",
  "qualification": "B.Sc. Mathematics, University of Colombo",
  "specialization": "Mathematics & Statistics",
  "experience_yrs": 8,
  "salary_lkr": 85000,
  "joined_date": "2022-03-01",
  "class_assignments": [
    { "class_id": "cls_001", "subject_id": "sub_math_al" },
    { "class_id": "cls_003", "subject_id": "sub_math_ol" }
  ]
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "tch_001",
    "teacher_no": "TCH-001",
    "login_credentials": {
      "email": "ravindu@jayasingheacademy.lk",
      "temp_password": "Rav@2025"
    }
  },
  "message": "Teacher registered. Credentials sent to email."
}
```

---

### GET `/teachers/:teacher_id/schedule`
Get teacher's personal weekly timetable.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "teacher_id": "tch_001",
    "teacher_name": "Mr. Ravindu Tharaka",
    "weekly_hours": 15,
    "schedule": {
      "MONDAY": [
        {
          "slot_id": "slot_001",
          "class": "A/L Commerce 2024",
          "subject": "Statistics",
          "start_time": "15:00",
          "end_time": "16:30",
          "room": "Room 3",
          "class_type": "IN_PERSON",
          "students": 31
        },
        {
          "slot_id": "slot_002",
          "class": "A/L Science 2024",
          "subject": "Mathematics",
          "start_time": "17:00",
          "end_time": "19:00",
          "room": "Hall A",
          "class_type": "IN_PERSON",
          "students": 42
        }
      ],
      "TUESDAY": [],
      "WEDNESDAY": [
        {
          "slot_id": "slot_003",
          "class": "A/L Commerce 2024",
          "subject": "Statistics",
          "start_time": "15:00",
          "end_time": "16:30",
          "room": "Room 3",
          "class_type": "IN_PERSON",
          "students": 31
        },
        {
          "slot_id": "slot_004",
          "class": "A/L Science 2024",
          "subject": "Mathematics",
          "start_time": "17:00",
          "end_time": "19:00",
          "room": "Hall A",
          "class_type": "IN_PERSON",
          "students": 42
        }
      ],
      "THURSDAY": [],
      "FRIDAY": [
        {
          "slot_id": "slot_005",
          "class": "A/L Science 2024",
          "subject": "Mathematics",
          "start_time": "17:00",
          "end_time": "19:00",
          "room": "Hall A",
          "class_type": "IN_PERSON",
          "students": 42
        }
      ],
      "SATURDAY": [
        {
          "slot_id": "slot_006",
          "class": "A/L Science 2024",
          "subject": "Mathematics Revision",
          "start_time": "08:00",
          "end_time": "10:00",
          "room": "Hall B",
          "class_type": "IN_PERSON",
          "students": 42
        },
        {
          "slot_id": "slot_007",
          "class": "O/L 2024",
          "subject": "Mathematics",
          "start_time": "14:00",
          "end_time": "16:00",
          "room": "Hall A",
          "class_type": "IN_PERSON",
          "students": 54
        }
      ],
      "SUNDAY": [
        {
          "slot_id": "slot_008",
          "class": "O/L 2024",
          "subject": "Mathematics",
          "start_time": "08:00",
          "end_time": "10:00",
          "room": "Hall A",
          "class_type": "ONLINE",
          "students": 54
        }
      ]
    }
  }
}
```

---

## 11. API Endpoints — Parents

### GET `/parents`
**Auth:** INSTITUTE_ADMIN

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "par_001",
      "full_name": "Mrs. Kumari Perera",
      "phone": "+94778912345",
      "whatsapp_no": "+94778912345",
      "preferred_channel": "WHATSAPP",
      "children": [
        {
          "student_id": "stu_001",
          "student_no": "SE-2401",
          "name": "Kavindi Perera",
          "relation": "MOTHER"
        }
      ]
    }
  ]
}
```

---

### GET `/parents/me/children`
Get parent's children data (used in parent mobile app).

**Auth:** PARENT (own token)

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "student_id": "stu_001",
      "student_no": "SE-2401",
      "name": "Kavindi Perera",
      "photo_url": "https://cdn.smartedupro.lk/photos/stu_001.jpg",
      "class": "A/L Science 2024",
      "today_attendance": {
        "status": "PRESENT",
        "check_in_time": "2025-07-09T17:02:00.000Z",
        "class": "Mathematics",
        "teacher": "Mr. Ravindu Tharaka"
      },
      "stats": {
        "attendance_rate": 94.2,
        "gpa": 3.8,
        "class_rank": 4,
        "fee_status": "PAID",
        "outstanding_balance": 0
      },
      "latest_result": {
        "subject": "Mathematics",
        "test": "Test 4",
        "score": 88,
        "grade": "A"
      }
    }
  ]
}
```

---

## 12. API Endpoints — Classes & Subjects

### GET `/classes`
**Auth:** INSTITUTE_ADMIN, TEACHER

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cls_001",
      "name": "A/L Science 2024",
      "code": "CLS-01",
      "study_level": "AL_YEAR_2",
      "al_stream": "SCIENCE",
      "academic_year": 2024,
      "class_type": "IN_PERSON",
      "student_count": 42,
      "max_students": 50,
      "subjects": [
        {
          "subject_id": "sub_math_al",
          "subject_name": "Mathematics",
          "teacher": "Mr. Ravindu Tharaka",
          "monthly_fee": 3200
        },
        {
          "subject_id": "sub_phy_al",
          "subject_name": "Physics",
          "teacher": "Ms. Priya Mendis",
          "monthly_fee": 3200
        },
        {
          "subject_id": "sub_chem_al",
          "subject_name": "Chemistry",
          "teacher": "Mr. Asanka Silva",
          "monthly_fee": 3400
        }
      ],
      "total_fee_lkr": 9800,
      "is_active": true
    }
  ]
}
```

---

### POST `/classes`
Create a new class.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "name": "A/L Science Batch 2025",
  "code": "CLS-05",
  "study_level": "AL_YEAR_1",
  "al_stream": "SCIENCE",
  "academic_year": 2025,
  "class_type": "HYBRID",
  "max_students": 45,
  "branch_id": "branch_001",
  "subjects": [
    { "subject_id": "sub_math_al", "teacher_id": "tch_001", "monthly_fee": 3200 },
    { "subject_id": "sub_phy_al", "teacher_id": "tch_002", "monthly_fee": 3200 }
  ]
}
```

---

### POST `/classes/:class_id/enroll`
Enroll student in a class.

**Request Body:**
```json
{
  "student_id": "stu_009",
  "fee_package_id": "fpkg_001",
  "enrolled_date": "2025-07-15"
}
```

---

## 13. API Endpoints — Attendance (QR)

### POST `/attendance/sessions`
Create an attendance session (teacher starts class attendance).

**Auth:** TEACHER

**Request Body:**
```json
{
  "class_id": "cls_001",
  "subject_id": "sub_math_al",
  "session_date": "2025-07-09",
  "start_time": "17:00",
  "timetable_slot_id": "slot_002"
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "session_id": "sess_20250709_001",
    "class": "A/L Science 2024",
    "subject": "Mathematics",
    "date": "2025-07-09",
    "start_time": "17:00",
    "enrolled_students": 42,
    "records_initialized": 42,
    "qr_scan_endpoint": "POST /attendance/sessions/sess_20250709_001/scan"
  }
}
```

---

### POST `/attendance/sessions/:session_id/scan`
Mark student attendance via QR scan.

**Auth:** TEACHER

**Request Body:**
```json
{
  "qr_token": "eyJzdHVkZW50X2lkIjoic3R1XzAwMSIsIm5vbmNlIjoiNzJ4cGsifQ.HMAC_SIGNATURE",
  "scan_type": "CHECK_IN",
  "device_location": { "lat": 6.9271, "lng": 79.8612 }
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "student_id": "stu_001",
    "student_no": "SE-2401",
    "student_name": "Kavindi Perera",
    "status": "PRESENT",
    "check_in_time": "2025-07-09T17:02:13.000Z",
    "late_minutes": 0,
    "parent_notification_queued": false,
    "message": "Checked in successfully"
  }
}
```

**Response 400 (Already scanned):**
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_SCANNED",
    "message": "Kavindi Perera already checked in at 5:02 PM"
  }
}
```

---

### GET `/attendance/sessions/:session_id`
Get live attendance log for a session.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "session_id": "sess_20250709_001",
    "class": "A/L Science 2024",
    "subject": "Mathematics",
    "teacher": "Mr. Ravindu Tharaka",
    "date": "2025-07-09",
    "start_time": "17:00",
    "summary": {
      "total_enrolled": 42,
      "present": 38,
      "absent": 2,
      "late": 2,
      "not_marked": 0,
      "attendance_rate": 90.5
    },
    "records": [
      {
        "student_id": "stu_001",
        "student_no": "SE-2401",
        "student_name": "Kavindi Perera",
        "status": "PRESENT",
        "check_in_time": "2025-07-09T17:02:13.000Z",
        "check_out_time": null,
        "late_minutes": 0,
        "marked_via": "QR"
      },
      {
        "student_id": "stu_002",
        "student_no": "SE-2402",
        "student_name": "Sachith Fernando",
        "status": "ABSENT",
        "check_in_time": null,
        "check_out_time": null,
        "late_minutes": 0,
        "marked_via": null
      }
    ]
  }
}
```

---

### PATCH `/attendance/records/:record_id`
Manual override of attendance record.

**Auth:** TEACHER, INSTITUTE_ADMIN

**Request Body:**
```json
{
  "status": "EXCUSED",
  "notes": "Medical leave — submitted sick note"
}
```

---

### POST `/attendance/sessions/:session_id/close`
Close session and trigger parent notifications.

**Auth:** TEACHER

**Request Body:**
```json
{
  "end_time": "19:05",
  "notify_absent_parents": true,
  "notify_late_parents": true
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "session_closed": true,
    "end_time": "2025-07-09T19:05:00.000Z",
    "final_summary": {
      "present": 38, "absent": 3, "late": 1
    },
    "notifications_queued": {
      "absent_notifications": 3,
      "late_notifications": 1,
      "channels": ["WHATSAPP", "SMS"]
    }
  }
}
```

---

### GET `/attendance/reports`
Attendance analytics report.

**Query Params:** `?class_id=cls_001&from=2025-06-01&to=2025-06-30&format=json`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "period": { "from": "2025-06-01", "to": "2025-06-30" },
    "class": "A/L Science 2024",
    "total_sessions": 26,
    "average_rate": 89.4,
    "student_summary": [
      {
        "student_no": "SE-2401",
        "name": "Kavindi Perera",
        "present": 24,
        "absent": 1,
        "late": 1,
        "rate": 96.2,
        "trend": "improving"
      },
      {
        "student_no": "SE-2404",
        "name": "Ravindu Jayawardena",
        "present": 19,
        "absent": 7,
        "late": 0,
        "rate": 73.1,
        "trend": "declining",
        "flag": "AT_RISK"
      }
    ],
    "daily_trend": [
      { "date": "2025-06-02", "present": 39, "absent": 3, "rate": 92.9 },
      { "date": "2025-06-04", "present": 37, "absent": 5, "rate": 88.1 }
    ]
  }
}
```

---

## 14. API Endpoints — Exams & Results

### POST `/exams`
Create a new exam.

**Auth:** TEACHER, INSTITUTE_ADMIN

**Request Body:**
```json
{
  "class_id": "cls_001",
  "subject_id": "sub_math_al",
  "title": "Test 5 — July 2025",
  "exam_type": "TEST",
  "exam_date": "2025-07-05",
  "start_time": "08:00",
  "duration_mins": 120,
  "total_marks": 100,
  "pass_marks": 40,
  "instructions": "No calculators allowed. Answer all questions."
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "exam_005",
    "title": "Test 5 — July 2025",
    "exam_date": "2025-07-05",
    "enrolled_students": 42,
    "result_sheet_url": null
  }
}
```

---

### POST `/exams/:exam_id/results`
Bulk upload exam results.

**Auth:** TEACHER

**Request Body:**
```json
{
  "results": [
    { "student_id": "stu_001", "marks_obtained": 88, "is_absent": false },
    { "student_id": "stu_002", "marks_obtained": null, "is_absent": true },
    { "student_id": "stu_003", "marks_obtained": 91, "is_absent": false },
    { "student_id": "stu_004", "marks_obtained": 65, "is_absent": false }
  ],
  "publish_immediately": true,
  "notify_students": true,
  "notify_parents": true
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "results_saved": 42,
    "published": true,
    "rankings_computed": true,
    "class_stats": {
      "highest": 98,
      "lowest": 45,
      "average": 76.3,
      "pass_count": 38,
      "fail_count": 4,
      "pass_rate": 90.5
    },
    "notifications_queued": 84
  }
}
```

---

### GET `/exams/:exam_id/results`
Get full result sheet with rankings.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "exam": {
      "id": "exam_004",
      "title": "Test 4 — June 2025",
      "exam_date": "2025-06-09",
      "subject": "Mathematics",
      "class": "A/L Science 2024",
      "total_marks": 100
    },
    "class_stats": {
      "average": 79.6,
      "highest": 98,
      "lowest": 45,
      "pass_rate": 90.5,
      "students_tested": 42
    },
    "leaderboard": [
      {
        "rank": 1,
        "student_no": "SE-2407",
        "name": "Thilini Rathnayake",
        "marks": 98,
        "percentage": 98.0,
        "grade": "A",
        "trend": "UP"
      },
      {
        "rank": 2,
        "student_no": "SE-2401",
        "name": "Kavindi Perera",
        "marks": 88,
        "percentage": 88.0,
        "grade": "A",
        "trend": "UP"
      },
      {
        "rank": 3,
        "student_no": "SE-2403",
        "name": "Nimali Silva",
        "marks": 85,
        "percentage": 85.0,
        "grade": "A",
        "trend": "SAME"
      }
    ]
  }
}
```

---

### GET `/students/:student_id/report-card`
Generate digital report card data.

**Query Params:** `?term=2&year=2025&format=json|pdf`

**Response 200 (json):**
```json
{
  "success": true,
  "data": {
    "institute": {
      "name": "Jayasinghe Academy",
      "logo_url": "https://cdn.smartedupro.lk/logos/inst_abc123.png"
    },
    "student": {
      "name": "Kavindi Perera",
      "student_no": "SE-2401",
      "class": "A/L Science 2024",
      "photo_url": "https://cdn.smartedupro.lk/photos/stu_001.jpg"
    },
    "term": "Term 2, 2025",
    "results": [
      {
        "subject": "Mathematics",
        "marks": "88/100",
        "grade": "A",
        "rank": "#4",
        "class_avg": 79.6,
        "teacher": "Mr. Ravindu Tharaka"
      },
      {
        "subject": "Physics",
        "marks": "82/100",
        "grade": "B",
        "rank": "#11",
        "class_avg": 73.2,
        "teacher": "Ms. Priya Mendis"
      },
      {
        "subject": "Chemistry",
        "marks": "90/100",
        "grade": "A",
        "rank": "#2",
        "class_avg": 78.9,
        "teacher": "Mr. Asanka Silva"
      }
    ],
    "overall": {
      "average": 86.7,
      "gpa": 3.8,
      "class_rank": 4,
      "attendance_rate": 94.2,
      "class_teacher_remark": "Excellent progress. Keep it up!",
      "principal_remark": "Consistent performer — highly commended"
    }
  }
}
```

**Response 200 (pdf):** Binary PDF stream with `Content-Type: application/pdf`

---

## 15. API Endpoints — Timetable

### GET `/timetable`
Get institute-wide timetable grid.

**Auth:** INSTITUTE_ADMIN

**Query Params:** `?branch_id=branch_001&class_id=cls_001&teacher_id=tch_001`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "slots": [
      {
        "slot_id": "slot_001",
        "day": "MONDAY",
        "start_time": "15:00",
        "end_time": "16:30",
        "class": "A/L Commerce 2024",
        "subject": "Statistics",
        "teacher": "Mr. Ravindu Tharaka",
        "room": "Room 3",
        "class_type": "IN_PERSON",
        "student_count": 31,
        "color": "#d97706"
      },
      {
        "slot_id": "slot_002",
        "day": "MONDAY",
        "start_time": "17:00",
        "end_time": "19:00",
        "class": "A/L Science 2024",
        "subject": "Mathematics",
        "teacher": "Mr. Ravindu Tharaka",
        "room": "Hall A",
        "class_type": "IN_PERSON",
        "student_count": 42,
        "color": "#4F46E5"
      }
    ]
  }
}
```

---

### POST `/timetable/slots`
Add a new timetable slot.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "class_id": "cls_001",
  "subject_id": "sub_math_al",
  "teacher_id": "tch_001",
  "day_of_week": "MONDAY",
  "start_time": "17:00",
  "end_time": "19:00",
  "room": "Hall A",
  "class_type": "IN_PERSON",
  "effective_from": "2025-07-14"
}
```

---

### POST `/timetable/slots/:slot_id/override`
Cancel or reschedule a specific day.

**Request Body:**
```json
{
  "override_date": "2025-07-14",
  "action": "RESCHEDULED",
  "new_start_time": "18:00",
  "new_end_time": "20:00",
  "reason": "Power outage maintenance at Hall A",
  "notify_students": true
}
```

---

### GET `/timetable/student/:student_id`
Get timetable for a specific student.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "student": "Kavindi Perera",
    "weekly_schedule": {
      "MONDAY": [
        {
          "time": "17:00 – 19:00",
          "subject": "Mathematics",
          "teacher": "Mr. Ravindu Tharaka",
          "room": "Hall A",
          "type": "IN_PERSON"
        }
      ],
      "TUESDAY": [
        {
          "time": "16:00 – 18:00",
          "subject": "Physics",
          "teacher": "Ms. Priya Mendis",
          "room": "Lab 1",
          "type": "IN_PERSON"
        }
      ],
      "WEDNESDAY": [
        {
          "time": "17:00 – 19:00",
          "subject": "Mathematics",
          "teacher": "Mr. Ravindu Tharaka",
          "room": "Hall A",
          "type": "IN_PERSON"
        },
        {
          "time": "19:00 – 21:00",
          "subject": "Chemistry",
          "teacher": "Mr. Asanka Silva",
          "room": "Lab 2",
          "type": "IN_PERSON"
        }
      ]
    }
  }
}
```

---

## 16. API Endpoints — Finance & Fees

### GET `/fees/records`
List fee records for institute.

**Auth:** INSTITUTE_ADMIN

**Query Params:** `?month=2025-06&status=OVERDUE&class_id=cls_001&page=1`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "fee_rec_001",
      "student_no": "SE-2401",
      "student_name": "Kavindi Perera",
      "class": "A/L Science 2024",
      "fee_package": "A/L Full Package",
      "billing_month": "2025-06",
      "amount_due": 9800,
      "amount_paid": 9800,
      "late_fee": 0,
      "balance": 0,
      "status": "PAID",
      "due_date": "2025-06-05",
      "last_payment": {
        "date": "2025-06-02",
        "method": "ONLINE",
        "ref": "TXN20250602KP"
      }
    },
    {
      "id": "fee_rec_010",
      "student_no": "SE-2404",
      "student_name": "Ravindu Jayawardena",
      "class": "O/L 2024",
      "fee_package": "O/L Standard",
      "billing_month": "2025-06",
      "amount_due": 7200,
      "amount_paid": 0,
      "late_fee": 500,
      "balance": 7700,
      "status": "OVERDUE",
      "due_date": "2025-06-05",
      "last_payment": null,
      "reminder_count": 2
    }
  ],
  "meta": {
    "total": 420,
    "paid_count": 396,
    "partial_count": 16,
    "overdue_count": 8,
    "total_collected_lkr": 2940000,
    "total_outstanding_lkr": 180400
  }
}
```

---

### POST `/fees/payments`
Record a fee payment.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "student_id": "stu_001",
  "fee_record_id": "fee_rec_001",
  "amount_lkr": 9800,
  "payment_method": "CASH",
  "payment_date": "2025-07-02",
  "payment_ref": null,
  "received_by": "user_admin_001",
  "notes": "Paid by parent at reception",
  "generate_receipt": true,
  "send_receipt_whatsapp": true
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "payment_id": "pay_001",
    "receipt_no": "RCPT-2025-0420",
    "receipt_url": "https://cdn.smartedupro.lk/receipts/RCPT-2025-0420.pdf",
    "fee_status_updated": "PAID",
    "whatsapp_sent": true,
    "balance_remaining": 0
  }
}
```

---

### POST `/fees/reminders`
Send fee reminders to overdue/pending students.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "month": "2025-07",
  "statuses": ["OVERDUE", "PENDING"],
  "class_ids": ["cls_001", "cls_003"],
  "channels": ["WHATSAPP", "SMS"],
  "custom_message": "Dear Parent, July fee of LKR {{amount}} is due by {{due_date}}. Please pay at the earliest. - Jayasinghe Academy"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "students_targeted": 26,
    "whatsapp_queued": 26,
    "sms_queued": 26,
    "total_outstanding_lkr": 180400
  }
}
```

---

### GET `/fees/analytics`
Financial analytics dashboard.

**Query Params:** `?from=2025-01-01&to=2025-06-30`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "period": "Jan – Jun 2025",
    "monthly_revenue": [
      { "month": "2025-01", "collected": 285000, "outstanding": 45000, "rate": 86.4 },
      { "month": "2025-02", "collected": 312000, "outstanding": 38000, "rate": 89.1 },
      { "month": "2025-03", "collected": 298000, "outstanding": 52000, "rate": 85.1 },
      { "month": "2025-04", "collected": 341000, "outstanding": 29000, "rate": 92.2 },
      { "month": "2025-05", "collected": 378000, "outstanding": 22000, "rate": 94.5 },
      { "month": "2025-06", "collected": 294000, "outstanding": 86000, "rate": 77.4 }
    ],
    "current_month": {
      "total_due": 380000,
      "collected": 294000,
      "outstanding": 86000,
      "collection_rate": 77.4,
      "paid_students": 396,
      "unpaid_students": 24
    },
    "top_outstanding": [
      { "student": "Ravindu Jayawardena", "amount": 15400, "months_overdue": 2 }
    ]
  }
}
```

---

### POST `/invoices/generate`
Generate and send invoice for a student.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "student_id": "stu_001",
  "fee_record_id": "fee_rec_025",
  "send_via": ["WHATSAPP", "EMAIL"]
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "invoice_no": "INV-2025-0789",
    "pdf_url": "https://cdn.smartedupro.lk/invoices/INV-2025-0789.pdf",
    "whatsapp_sent": true,
    "email_sent": true
  }
}
```

---

## 17. API Endpoints — Online Learning

### POST `/live-sessions`
Create/schedule a live class session.

**Auth:** TEACHER, INSTITUTE_ADMIN

**Request Body:**
```json
{
  "class_id": "cls_001",
  "subject_id": "sub_math_al",
  "title": "Integration — Advanced Methods",
  "scheduled_start": "2025-07-09T17:00:00.000Z",
  "scheduled_end": "2025-07-09T19:00:00.000Z",
  "timetable_slot_id": "slot_002",
  "chat_enabled": true,
  "recording_enabled": true
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "session_id": "sess_live_001",
    "room_id": "smartedu_cls001_20250709",
    "host_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "join_url": "https://live.smartedupro.lk/join/smartedu_cls001_20250709",
    "rtc_provider": "agora",
    "rtc_app_id": "agoraAppId123",
    "rtc_channel": "smartedu_cls001_20250709"
  }
}
```

---

### POST `/live-sessions/:session_id/start`
Mark session as live and notify students.

**Auth:** TEACHER

**Response 200:**
```json
{
  "success": true,
  "data": {
    "session_id": "sess_live_001",
    "status": "LIVE",
    "actual_start": "2025-07-09T17:02:00.000Z",
    "student_notifications_sent": 42,
    "parent_notifications_sent": 38
  }
}
```

---

### GET `/live-sessions/:session_id/join-token`
Get join token for a student/parent to join live class.

**Auth:** STUDENT, PARENT

**Response 200:**
```json
{
  "success": true,
  "data": {
    "session_id": "sess_live_001",
    "room_id": "smartedu_cls001_20250709",
    "participant_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "join_url": "https://live.smartedupro.lk/join/smartedu_cls001_20250709",
    "role": "STUDENT",
    "session_info": {
      "title": "Integration — Advanced Methods",
      "teacher": "Mr. Ravindu Tharaka",
      "class": "A/L Science 2024",
      "subject": "Mathematics",
      "started_at": "2025-07-09T17:02:00.000Z"
    }
  }
}
```

---

### POST `/live-sessions/:session_id/end`
End live session and save recording.

**Auth:** TEACHER

**Response 200:**
```json
{
  "success": true,
  "data": {
    "session_id": "sess_live_001",
    "duration_minutes": 122,
    "peak_viewers": 38,
    "recording_status": "PROCESSING",
    "recording_available_in": "15 minutes"
  }
}
```

---

### GET `/recordings`
List recordings available to user.

**Auth:** All roles

**Query Params:** `?class_id=cls_001&subject_id=sub_math_al&page=1`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "rec_001",
      "title": "Integration by Substitution — Full Walkthrough",
      "subject": "Mathematics",
      "teacher": "Mr. Ravindu Tharaka",
      "class": "A/L Science 2024",
      "duration_secs": 4332,
      "duration_display": "1h 12m",
      "thumbnail_url": "https://cdn.smartedupro.lk/thumbnails/rec_001.jpg",
      "stream_url": "https://cdn.smartedupro.lk/recordings/rec_001.m3u8",
      "view_count": 38,
      "recorded_at": "2025-06-06T17:00:00.000Z",
      "is_published": true
    }
  ]
}
```

---

### POST `/resources`
Upload learning resource.

**Auth:** TEACHER, INSTITUTE_ADMIN

**Request:** `multipart/form-data`
```
class_id: cls_001
subject_id: sub_math_al
title: Integration by Substitution Notes
description: Complete notes from today's class
file: [binary PDF]
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "res_001",
    "title": "Integration by Substitution Notes",
    "file_url": "https://cdn.smartedupro.lk/resources/res_001.pdf",
    "file_type": "PDF",
    "file_size_mb": 2.4,
    "download_url": "https://cdn.smartedupro.lk/resources/res_001.pdf?token=xxx"
  }
}
```

---

## 18. API Endpoints — Notifications

### POST `/notifications/send`
Send notification to users.

**Auth:** INSTITUTE_ADMIN, TEACHER

**Request Body:**
```json
{
  "type": "ANNOUNCEMENT",
  "title": "Extra Revision Class — Saturday",
  "body": "There will be an extra revision class this Saturday at 8:00 AM in Hall B. Attendance is compulsory for all A/L Science students.",
  "channels": ["PUSH", "WHATSAPP"],
  "target": {
    "class_ids": ["cls_001"],
    "roles": ["STUDENT", "PARENT"]
  },
  "schedule_at": null
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "notification_batch_id": "notif_batch_001",
    "recipients": 84,
    "push_queued": 42,
    "whatsapp_queued": 42,
    "estimated_delivery": "< 30 seconds"
  }
}
```

---

### GET `/notifications`
Get notifications for current user.

**Auth:** All roles

**Query Params:** `?is_read=false&type=ATTENDANCE_ALERT&page=1&per_page=20`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "notif_001",
      "type": "ATTENDANCE_ALERT",
      "title": "Attendance Marked ✓",
      "body": "Kavindi Perera has been marked PRESENT for Mathematics class at 5:02 PM",
      "data": {
        "student_id": "stu_001",
        "session_id": "sess_20250709_001",
        "status": "PRESENT"
      },
      "channel": "PUSH",
      "is_read": false,
      "is_delivered": true,
      "created_at": "2025-07-09T17:02:15.000Z"
    },
    {
      "id": "notif_002",
      "type": "RESULT_PUBLISHED",
      "title": "Test 4 Results Available",
      "body": "Mathematics Test 4 results have been published. Kavindi scored 88/100 (A) — Rank #4",
      "data": {
        "exam_id": "exam_004",
        "student_id": "stu_001",
        "score": 88,
        "grade": "A"
      },
      "channel": "PUSH",
      "is_read": false,
      "created_at": "2025-07-08T14:30:00.000Z"
    }
  ],
  "meta": { "unread_count": 7, "total": 28 }
}
```

---

### PATCH `/notifications/mark-read`
Mark notifications as read.

**Request Body:**
```json
{
  "notification_ids": ["notif_001", "notif_002"],
  "mark_all": false
}
```

---

### GET `/notifications/templates`
List notification templates.

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "tmpl_001",
      "type": "ATTENDANCE_ALERT",
      "channel": "WHATSAPP",
      "title_template": "Attendance Update — {{student_name}}",
      "body_template": "Dear Parent,\n\n{{student_name}} has been marked *{{status}}* for {{subject}} class at {{time}}.\n\nRegards,\n{{institute_name}}",
      "variables": ["student_name", "status", "subject", "time", "institute_name"],
      "is_active": true
    },
    {
      "id": "tmpl_002",
      "type": "FEE_DUE",
      "channel": "WHATSAPP",
      "title_template": "Fee Reminder — {{institute_name}}",
      "body_template": "Dear Parent,\n\nThis is a reminder that the {{month}} fee of *LKR {{amount}}* for {{student_name}} is due by *{{due_date}}*.\n\nPlease settle at your earliest convenience.\n\n{{institute_name}}",
      "variables": ["month", "amount", "student_name", "due_date", "institute_name"],
      "is_active": true
    }
  ]
}
```

---

## 19. API Endpoints — Messages (Chat)

### GET `/messages/conversations`
Get list of conversations for current user.

**Auth:** TEACHER, PARENT

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "conversation_id": "conv_001",
      "with_user": {
        "id": "tch_001",
        "name": "Mr. Ravindu Tharaka",
        "role": "TEACHER",
        "avatar_url": "https://cdn.smartedupro.lk/photos/tch_001.jpg"
      },
      "last_message": {
        "body": "Yes, we have revision classes every Saturday until the exam.",
        "created_at": "2025-07-09T11:32:00.000Z",
        "is_mine": false
      },
      "unread_count": 0
    }
  ]
}
```

---

### GET `/messages/:recipient_id`
Get message thread with a user.

**Query Params:** `?page=1&per_page=30`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "messages": [
      {
        "id": "msg_001",
        "sender_id": "par_001",
        "sender_name": "Mrs. Kumari Perera",
        "body": "Thank you sir! She has been studying more regularly.",
        "is_mine": true,
        "is_read": true,
        "created_at": "2025-07-09T11:25:00.000Z"
      },
      {
        "id": "msg_002",
        "sender_id": "tch_001",
        "sender_name": "Mr. Ravindu Tharaka",
        "body": "Please encourage her to attempt the past papers I uploaded.",
        "is_mine": false,
        "is_read": true,
        "created_at": "2025-07-09T11:27:00.000Z"
      }
    ]
  }
}
```

---

### POST `/messages/:recipient_id`
Send a message.

**Auth:** TEACHER, PARENT, INSTITUTE_ADMIN

**Request Body:**
```json
{
  "body": "Will do. Is there any extra class before the A/L exam?",
  "attachment_url": null
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "message_id": "msg_003",
    "delivered": true,
    "push_sent": true,
    "created_at": "2025-07-09T11:30:00.000Z"
  }
}
```

---

## 20. API Endpoints — Dashboard & Reports

### GET `/dashboard/institute`
Institute admin dashboard analytics.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "today": {
      "date": "2025-07-09",
      "scheduled_classes": 4,
      "sessions_started": 2,
      "attendance_rate": 89.3,
      "students_present": 156,
      "students_absent": 19,
      "live_sessions_active": 1
    },
    "this_month": {
      "attendance_rate": 88.7,
      "fee_collection_rate": 77.4,
      "fees_collected_lkr": 294000,
      "fees_outstanding_lkr": 86000,
      "new_enrollments": 8,
      "exams_conducted": 6
    },
    "snapshots": {
      "total_active_students": 420,
      "total_teachers": 12,
      "total_classes": 18,
      "average_gpa": 3.41
    },
    "upcoming": {
      "classes_today": [
        { "time": "15:00", "subject": "Statistics", "teacher": "Mr. Ravindu", "room": "Room 3" },
        { "time": "17:00", "subject": "Mathematics", "teacher": "Mr. Ravindu", "room": "Hall A" },
        { "time": "16:00", "subject": "Physics", "teacher": "Ms. Priya", "room": "Lab 1" },
        { "time": "19:00", "subject": "Chemistry", "teacher": "Mr. Asanka", "room": "Lab 2" }
      ]
    }
  }
}
```

---

### GET `/reports/attendance/export`
Export attendance report as CSV or PDF.

**Query Params:** `?format=csv&class_id=cls_001&from=2025-06-01&to=2025-06-30`

**Response:** Binary file download

---

### GET `/reports/results/export`
Export exam results report.

**Query Params:** `?format=pdf&exam_id=exam_004`

**Response:** Binary PDF

---

## 21. API Endpoints — QR Code

### GET `/qr/student/:student_id`
Get QR code data for a student.

**Auth:** STUDENT (own), INSTITUTE_ADMIN, TEACHER

**Response 200:**
```json
{
  "success": true,
  "data": {
    "student_no": "SE-2401",
    "student_name": "Kavindi Perera",
    "qr_token": "eyJzdHVkZW50X2lkIjoic3R1XzAwMSIsIm5vbmNlIjoiN...",
    "qr_data": "SMARTEDU:SE-2401:Kavindi Perera:inst_abc123:SIG",
    "qr_image_url": "https://cdn.smartedupro.lk/qrcodes/SE-2401.png",
    "expires_at": null,
    "generated_at": "2025-01-10T00:00:00.000Z"
  }
}
```

---

### POST `/qr/verify`
Verify a scanned QR token.

**Auth:** TEACHER

**Request Body:**
```json
{
  "qr_token": "eyJzdHVkZW50X2lkIjoic3R1XzAwMSIsIm5vbmNlIjoiN...",
  "session_id": "sess_20250709_001"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "valid": true,
    "student_id": "stu_001",
    "student_no": "SE-2401",
    "student_name": "Kavindi Perera",
    "is_enrolled_in_class": true,
    "previous_attendance_today": false
  }
}
```

**Response 400 (Invalid/Tampered QR):**
```json
{
  "success": false,
  "error": {
    "code": "INVALID_QR_TOKEN",
    "message": "QR token signature verification failed"
  }
}
```

---

### POST `/qr/regenerate/:student_id`
Regenerate QR for a student (if lost/compromised).

**Auth:** INSTITUTE_ADMIN

**Response 200:**
```json
{
  "success": true,
  "data": {
    "new_qr_image_url": "https://cdn.smartedupro.lk/qrcodes/SE-2401-v2.png",
    "old_token_revoked": true,
    "sms_sent": true
  }
}
```

---

## 22. API Endpoints — Billing (SaaS)

### GET `/billing/plans`
Get all available subscription plans.

**Auth:** Public

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "plan": "TRIAL",
      "display_name": "Free Trial",
      "price_lkr": 0,
      "duration_days": 30,
      "features": {
        "max_students": 50,
        "max_teachers": 3,
        "qr_attendance": true,
        "live_classes": false,
        "sms_notifications": false,
        "whatsapp_notifications": false,
        "recording": false,
        "reports": false
      }
    },
    {
      "plan": "BASIC",
      "display_name": "Basic Plan",
      "price_lkr": 15000,
      "price_display": "LKR 15,000 / month",
      "features": {
        "max_students": 200,
        "max_teachers": 10,
        "qr_attendance": true,
        "live_classes": false,
        "sms_notifications": true,
        "whatsapp_notifications": false,
        "recording": false,
        "reports": true
      }
    },
    {
      "plan": "PRO",
      "display_name": "Pro Plan",
      "price_lkr": 35000,
      "price_display": "LKR 35,000 / month",
      "features": {
        "max_students": 500,
        "max_teachers": 25,
        "max_branches": 3,
        "qr_attendance": true,
        "live_classes": true,
        "sms_notifications": true,
        "whatsapp_notifications": true,
        "recording": true,
        "reports": true
      }
    },
    {
      "plan": "ENTERPRISE",
      "display_name": "Enterprise",
      "price_lkr": 85000,
      "price_display": "LKR 85,000 / month",
      "features": {
        "max_students": 9999,
        "max_teachers": 99,
        "max_branches": 10,
        "qr_attendance": true,
        "live_classes": true,
        "sms_notifications": true,
        "whatsapp_notifications": true,
        "recording": true,
        "reports": true,
        "custom_domain": true,
        "priority_support": true,
        "api_access": true
      }
    }
  ]
}
```

---

### POST `/billing/subscribe`
Upgrade/change institute subscription.

**Auth:** INSTITUTE_ADMIN

**Request Body:**
```json
{
  "plan": "PRO",
  "billing_months": 12,
  "payment_method": "BANK_TRANSFER",
  "payment_ref": "SLIP20250709001"
}
```

---

## 23. Enum Reference

| Enum | Values |
|------|--------|
| `user_role` | `SUPER_ADMIN`, `INSTITUTE_ADMIN`, `TEACHER`, `STUDENT`, `PARENT` |
| `subscription_plan` | `TRIAL`, `BASIC`, `PRO`, `ENTERPRISE` |
| `subscription_status` | `ACTIVE`, `SUSPENDED`, `CANCELLED`, `TRIAL`, `PAST_DUE` |
| `attendance_status` | `PRESENT`, `ABSENT`, `LATE`, `EXCUSED`, `HALF_DAY` |
| `fee_status` | `PAID`, `PARTIAL`, `OVERDUE`, `WAIVED`, `PENDING` |
| `payment_method` | `CASH`, `BANK_TRANSFER`, `ONLINE`, `CHEQUE`, `CARD` |
| `class_type` | `IN_PERSON`, `ONLINE`, `HYBRID` |
| `exam_type` | `TEST`, `MIDTERM`, `FINAL`, `MOCK`, `ASSIGNMENT`, `PRACTICAL` |
| `sl_grade` | `A_PLUS`, `A`, `B`, `C`, `S`, `F`, `AB` |
| `notification_channel` | `PUSH`, `SMS`, `WHATSAPP`, `EMAIL` |
| `notification_type` | `ATTENDANCE_ALERT`, `EXAM_REMINDER`, `RESULT_PUBLISHED`, `FEE_DUE`, `FEE_RECEIVED`, `CLASS_CANCELLED`, `CLASS_RESCHEDULED`, `ANNOUNCEMENT`, `CHAT_MESSAGE`, `LIVE_CLASS_STARTING` |
| `session_status` | `SCHEDULED`, `LIVE`, `ENDED`, `CANCELLED` |
| `day_of_week` | `MONDAY` … `SUNDAY` |
| `gender` | `MALE`, `FEMALE`, `OTHER` |
| `al_stream` | `SCIENCE`, `COMMERCE`, `ARTS`, `TECHNOLOGY`, `BIO_SCIENCE` |
| `study_level` | `GRADE_6`…`GRADE_10`, `OL`, `AL_YEAR_1`, `AL_YEAR_2`, `OTHER` |

### Sri Lankan Grade Boundaries
| Grade | Marks (%) | Description |
|-------|-----------|-------------|
| A+ | 90 – 100 | Distinction |
| A | 75 – 89 | Credit |
| B | 65 – 74 | Merit |
| C | 55 – 64 | Pass |
| S | 35 – 54 | Subsidiary Pass |
| F | 0 – 34 | Fail |
| AB | — | Absent |

---

## 24. Error Code Reference

| HTTP | Code | Description |
|------|------|-------------|
| 400 | `VALIDATION_ERROR` | Request body validation failed |
| 400 | `INVALID_QR_TOKEN` | QR token tampered or malformed |
| 400 | `ALREADY_SCANNED` | Student already marked for this session |
| 400 | `SESSION_CLOSED` | Attendance session already closed |
| 401 | `UNAUTHORIZED` | Missing or invalid access token |
| 401 | `TOKEN_EXPIRED` | Access token has expired |
| 401 | `INVALID_CREDENTIALS` | Wrong phone/email or password |
| 401 | `INVALID_OTP` | OTP code is wrong or expired |
| 403 | `FORBIDDEN` | Insufficient role permissions |
| 403 | `INSTITUTE_SCOPE` | Resource belongs to different institute |
| 403 | `PLAN_LIMIT_EXCEEDED` | Feature not available on current plan |
| 404 | `STUDENT_NOT_FOUND` | Student ID does not exist |
| 404 | `TEACHER_NOT_FOUND` | Teacher ID does not exist |
| 404 | `CLASS_NOT_FOUND` | Class ID does not exist |
| 404 | `EXAM_NOT_FOUND` | Exam ID does not exist |
| 404 | `SESSION_NOT_FOUND` | Attendance session not found |
| 409 | `STUDENT_ALREADY_ENROLLED` | Student already enrolled in class |
| 409 | `DUPLICATE_RESULT` | Result already entered for this student |
| 409 | `DUPLICATE_PHONE` | Phone number already registered |
| 422 | `MARKS_EXCEED_TOTAL` | Marks exceed maximum total |
| 429 | `RATE_LIMIT_EXCEEDED` | Too many requests |
| 500 | `INTERNAL_SERVER_ERROR` | Unexpected server error |
| 503 | `SMS_GATEWAY_ERROR` | SMS delivery service unavailable |
| 503 | `WHATSAPP_API_ERROR` | WhatsApp API unavailable |

---

## 25. Webhook Events

The backend fires webhook events to registered URLs. Configure in: `PATCH /institutes/me/settings`

### Event Payload Structure
```json
{
  "event": "attendance.marked",
  "timestamp": "2025-07-09T17:02:15.000Z",
  "institute_id": "inst_abc123",
  "data": { ... }
}
```

### Events List

| Event | Description | Payload |
|-------|-------------|---------|
| `student.registered` | New student created | `{ student_id, student_no, name }` |
| `attendance.marked` | QR attendance recorded | `{ session_id, student_id, status, time }` |
| `attendance.session_closed` | Session finalized | `{ session_id, summary }` |
| `exam.result_published` | Results published | `{ exam_id, class_id, stats }` |
| `fee.payment_received` | Payment recorded | `{ payment_id, student_id, amount }` |
| `fee.overdue_flagged` | Fee marked overdue | `{ fee_record_id, student_id, amount }` |
| `live_session.started` | Live class began | `{ session_id, class_id }` |
| `live_session.ended` | Live class ended | `{ session_id, duration_mins, recording_url }` |
| `message.received` | Chat message sent | `{ message_id, sender_id, recipient_id }` |
| `subscription.upgraded` | Plan changed | `{ institute_id, old_plan, new_plan }` |
| `subscription.expired` | Plan expired | `{ institute_id, plan }` |

### Webhook Security
```
X-SmartEdu-Signature: sha256=HMAC_SHA256(payload, webhook_secret)
X-SmartEdu-Timestamp: 1720526535
```

---

## Complete Table Relationship Summary

```
institutes
  ├── branches
  ├── institute_admins → users
  ├── subscription_plans (M:1)
  ├── billing_transactions
  ├── subjects
  ├── classes
  │     ├── class_subjects → subjects, teachers
  │     ├── student_class_enrollments → students
  │     └── timetable_slots → teachers, subjects
  ├── students → users
  │     ├── student_parents → parents
  │     ├── attendance_records → sessions
  │     ├── exam_results → exams
  │     ├── student_fee_records → fee_packages
  │     └── student_qr_tokens
  ├── teachers → users
  │     └── teacher_class_assignments → classes, subjects
  ├── parents → users
  ├── attendance_sessions → classes, subjects, teachers
  │     └── attendance_records → students
  ├── exams → classes, subjects, teachers
  │     └── exam_results → students
  ├── fee_packages
  │     ├── student_fee_records → students
  │     └── fee_payments
  ├── live_sessions → classes, subjects, teachers
  │     ├── live_session_participants → users
  │     └── recordings
  ├── learning_resources → classes, subjects, users
  ├── announcements → users, classes
  ├── messages → users (sender, recipient)
  ├── notifications → users
  └── notification_templates
```

---

*Document prepared for SmartEdu Pro backend development handoff.*
*Questions: dev@smartedupro.lk | Slack: #backend-api*
