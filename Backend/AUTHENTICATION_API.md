# YadgarCafe Authentication API Documentation

## Overview
This document describes the authentication endpoints for the YadgarCafe Restaurant Management System. The authentication system uses JWT (JSON Web Token) for stateless authentication with refresh token support for secure token renewal.

## Base URL
```
http://localhost:5000/api/auth
```

## Authentication Flow
1. User registers with email and password
2. User logs in and receives access token + refresh token
3. Access token is valid for 15 minutes
4. Refresh token is valid for 7 days
5. When access token expires, client uses refresh token to get a new access token
6. All protected endpoints require a valid JWT access token in the Authorization header

## Endpoints

### 1. Register User
**POST** `/api/auth/register`

Creates a new user account.

#### Request Body
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "fullName": "John Doe"
}
```

#### Response (Success - 200)
```json
{
  "success": true,
  "message": "User registered successfully.",
  "data": null
}
```

#### Response (Failure - 400)
```json
{
  "success": false,
  "message": "User with this email already exists.",
  "data": null
}
```

#### Validation Rules
- Email must be a valid email format
- Password must be at least 6 characters
- Password must contain uppercase, lowercase, and numbers
- Passwords must match
- Full name is required

---

### 2. Login User
**POST** `/api/auth/login`

Authenticates a user and returns access and refresh tokens.

#### Request Body
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

#### Response (Success - 200)
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlM==",
    "expiresIn": "2024-01-15T10:45:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "fullName": "John Doe",
      "userType": "Customer"
    }
  }
}
```

#### Response (Failure - 401)
```json
{
  "success": false,
  "message": "Invalid email or password.",
  "data": null
}
```

#### HTTP Headers (Required)
```
Content-Type: application/json
```

---

### 3. Refresh Access Token
**POST** `/api/auth/refresh-token`

Generates a new access token using a valid refresh token.

#### Request Body
```json
{
  "refreshToken": "AbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlM=="
}
```

#### Response (Success - 200)
```json
{
  "success": true,
  "message": "Token refreshed successfully.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "NewAbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGh==",
    "expiresIn": "2024-01-15T11:00:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "fullName": "John Doe",
      "userType": "Customer"
    }
  }
}
```

#### Response (Failure - 401)
```json
{
  "success": false,
  "message": "Invalid or expired refresh token.",
  "data": null
}
```

---

### 4. Logout User
**POST** `/api/auth/logout`

Revokes all active refresh tokens for the current user.

#### HTTP Headers (Required)
```
Authorization: Bearer <access_token>
Content-Type: application/json
```

#### Response (Success - 200)
```json
{
  "success": true,
  "message": "Logout successful.",
  "data": null
}
```

#### Response (Failure - 401)
```json
{
  "success": false,
  "message": "Invalid user identification.",
  "data": null
}
```

---

### 5. Get Current User
**GET** `/api/auth/me`

Retrieves the current authenticated user's information.

#### HTTP Headers (Required)
```
Authorization: Bearer <access_token>
```

#### Response (Success - 200)
```json
{
  "success": true,
  "message": "User retrieved successfully.",
  "data": {
    "accessToken": "",
    "refreshToken": "",
    "expiresIn": "2024-01-15T10:45:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "fullName": "John Doe",
      "userType": "Customer"
    }
  }
}
```

#### Response (Failure - 401)
```json
{
  "success": false,
  "message": "Invalid user identification.",
  "data": null
}
```

---

## Token Management

### JWT Access Token Structure
```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "user_id",
  "email": "user@example.com",
  "role": "Customer",
  "iat": 1705316700,
  "exp": 1705317600,
  "iss": "YadgarCafe",
  "aud": "YadgarCafeUsers"
}
```

### Token Lifetime
- **Access Token**: 15 minutes
- **Refresh Token**: 7 days

### Using the Access Token
All protected API requests must include the access token in the Authorization header:
```
Authorization: Bearer <access_token>
```

---

## Error Handling

### Common Error Codes

| Status | Message | Description |
|--------|---------|-------------|
| 400 | Bad Request | Malformed request or validation failed |
| 401 | Unauthorized | Invalid credentials or expired token |
| 404 | Not Found | User not found |
| 500 | Internal Server Error | Server error occurred |

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

---

## Security Best Practices

1. **HTTPS Only**: Always use HTTPS in production
2. **Token Storage**: Store tokens securely (HttpOnly cookies for web, secure storage for mobile)
3. **Token Expiration**: Never extend token lifetime beyond configured limits
4. **Refresh Token Rotation**: New refresh token issued on each refresh
5. **Logout**: Always revoke tokens on logout
6. **Password Policy**: 
   - Minimum 6 characters
   - Must contain uppercase, lowercase, and numbers
   - Store only hashed passwords

---

## Example Workflow

### 1. Register
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123",
    "confirmPassword": "Password123",
    "fullName": "John Doe"
  }'
```

### 2. Login
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123"
  }'
```

### 3. Access Protected Resource
```bash
curl -X GET "http://localhost:5000/api/auth/me" \
  -H "Authorization: Bearer <access_token>"
```

### 4. Refresh Token
```bash
curl -X POST "http://localhost:5000/api/auth/refresh-token" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "<refresh_token>"
  }'
```

### 5. Logout
```bash
curl -X POST "http://localhost:5000/api/auth/logout" \
  -H "Authorization: Bearer <access_token>"
```

---

## User Types
- **Admin**: Full access to all features
- **Staff**: Access to staff-specific features
- **Customer**: Access to customer features only

---

## Configuration

JWT settings are configured in `appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-change-this-in-production-must-be-at-least-32-characters-long-12345",
    "Issuer": "YadgarCafe",
    "Audience": "YadgarCafeUsers",
    "ExpirationMinutes": 15
  }
}
```

### Production Configuration
?? **IMPORTANT**: Before deploying to production:
1. Change the `SecretKey` to a secure, randomly generated value (at least 32 characters)
2. Use environment variables for sensitive data
3. Set `RequireHttpsMetadata` to `true`
4. Use secure password hashing algorithms

