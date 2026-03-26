# Notes App

A full-stack notes application built with Vue 3 + TypeScript (frontend) and ASP.NET Core 9 (backend), using MySQL via XAMPP as the database.

> **Note on database:** The assignment specified SQL Server. MySQL was used instead due to local storage constraints. The backend code is identical — only the NuGet package (`MySqlConnector` instead of `Microsoft.Data.SqlClient`) and connection string format differ.

---

## Features

- Register and login with JWT authentication
- Create, view, edit, and delete notes
- Search notes by title or content
- Sort notes by newest, oldest, or title
- Click any note title to view full details
- Each user can only see and manage their own notes
- Responsive design with Tailwind CSS

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Vue 3, TypeScript, Tailwind CSS v4, Pinia, Vue Router, Axios |
| Backend | ASP.NET Core 9, C#, Dapper, JWT Bearer |
| Database | MySQL 8 (via XAMPP) |

---

## Project Structure
```
Noteapp/
├── backend/                        ← C# ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AuthController.cs       ← POST /api/auth/register and /api/auth/login
│   │   └── NotesController.cs      ← GET/POST/PUT/DELETE /api/notes
│   ├── Models/
│   │   ├── Note.cs                 ← Note class + CreateNoteDto + UpdateNoteDto
│   │   └── User.cs                 ← User class + RegisterDto + LoginDto
│   ├── Services/
│   │   ├── NoteService.cs          ← All SQL queries for notes using Dapper
│   │   └── UserService.cs          ← SQL queries for user register and login
│   ├── appsettings.json            ← MySQL connection string + JWT config
│   └── Program.cs                  ← App startup, services, CORS, JWT, Swagger
│
└── frontend/                       ← Vue 3 TypeScript app
    └── src/
        ├── api/
        │   └── index.ts            ← Axios instance + all API call functions
        ├── stores/
        │   └── auth.ts             ← Pinia store for login/logout/token
        ├── router/
        │   └── index.ts            ← Routes + navigation guard (redirect if not logged in)
        ├── views/
        │   ├── LoginView.vue       ← Login and register page
        │   └── NotesView.vue       ← Main notes page with full CRUD + search + sort
        ├── App.vue                 ← Root component, renders current route
        └── main.ts                 ← App entry point, mounts Vue + Pinia + Router
```

---

## How Each File Works

### Backend

**`Program.cs`**
The entry point of the backend. Registers all services (repositories, CORS, JWT authentication, Swagger) and sets up the HTTP pipeline. Think of it as the "startup configuration" of the API.

**`appsettings.json`**
Stores configuration values like the MySQL connection string and the JWT secret key. Never commit real secrets to GitHub.

**`Models/Note.cs`**
Defines the shape of a Note in C#. The `Note` class maps directly to the `Notes` table. `CreateNoteDto` and `UpdateNoteDto` are what the API accepts from the frontend — they intentionally exclude `Id`, `UserId`, and timestamps so users cannot fake those values.

**`Models/User.cs`**
Same concept for users. `RegisterDto` and `LoginDto` define what the frontend sends when registering or logging in.

**`Services/NoteService.cs`**
Contains all database queries for notes using Dapper. Every method opens a MySQL connection, runs a parameterized SQL query, and returns the result. All queries include `UserId` so users can never access another user's notes.

**`Services/UserService.cs`**
Handles finding a user by email (for login) and creating a new user (for register). Passwords are hashed with BCrypt before storing.

**`Controllers/AuthController.cs`**
Handles `POST /api/auth/register` and `POST /api/auth/login`. On success it returns a JWT token which the frontend stores and sends with every future request.

**`Controllers/NotesController.cs`**
Handles all CRUD endpoints for notes. The `[Authorize]` attribute means every endpoint requires a valid JWT token. It reads the user's ID from the token so it always knows which user is making the request.

---

### Frontend

**`main.ts`**
The entry point of the Vue app. Creates the app, registers Pinia (state management) and Vue Router, then mounts everything to the `#app` div in `index.html`.

**`App.vue`**
The root component. It only contains `<RouterView />` which renders whichever page matches the current URL.

**`api/index.ts`**
Creates an Axios instance pointed at the backend URL. An interceptor automatically attaches the JWT token from localStorage to every request so you never have to add it manually. Also defines TypeScript interfaces for `Note` and exports all API call functions.

**`stores/auth.ts`**
A Pinia store that manages login state. Stores the JWT token in both memory (reactive) and localStorage (persists on refresh). Any component can call `auth.login()`, `auth.logout()`, or check `auth.isLoggedIn`.

**`router/index.ts`**
Defines two routes — `/` (notes page) and `/login`. The navigation guard runs before every route change: if you try to go to `/` without being logged in it redirects you to `/login`, and if you are logged in and try to go to `/login` it redirects you to `/`.

**`views/LoginView.vue`**
The login and register page. Toggles between login and register mode. On success calls the auth store and redirects to `/`. Shows error messages if credentials are wrong.

**`views/NotesView.vue`**
The main page. On load it fetches all notes from the API. Has three modals: a detail view (read only), a create/edit form, and uses `confirm()` for delete. Search and sort both trigger a new API call. Logout clears the token and redirects to `/login`.

---

## How to Run

### Requirements
- [XAMPP](https://www.apachefriends.org/) with MySQL started
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)

### 1. Database setup
1. Start MySQL in XAMPP Control Panel
2. Open `http://localhost/phpmyadmin`
3. Click the SQL tab and run:
```sql
CREATE DATABASE IF NOT EXISTS notesdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE notesdb;

CREATE TABLE Users (
  Id        INT AUTO_INCREMENT PRIMARY KEY,
  Username  VARCHAR(100) NOT NULL UNIQUE,
  Email     VARCHAR(200) NOT NULL UNIQUE,
  Password  VARCHAR(500) NOT NULL,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Notes (
  Id        INT AUTO_INCREMENT PRIMARY KEY,
  UserId    INT NOT NULL,
  Title     VARCHAR(200) NOT NULL,
  Content   TEXT,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### 2. Run the backend
```powershell
cd backend
dotnet run
```
API runs at `http://localhost:5033`
Swagger UI at `http://localhost:5033/swagger`

### 3. Run the frontend
```powershell
cd frontend
npm install
npm run dev
```
App runs at `http://localhost:5173`

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | /api/auth/register | No | Create a new account |
| POST | /api/auth/login | No | Login and receive JWT token |
| GET | /api/notes | Yes | Get all notes (supports ?search= and ?sort=) |
| GET | /api/notes/{id} | Yes | Get one note by id |
| POST | /api/notes | Yes | Create a new note |
| PUT | /api/notes/{id} | Yes | Update a note |
| DELETE | /api/notes/{id} | Yes | Delete a note |