### Endpoints

1. POST /register
   - Purpose: Register a new user.
   - Request Body: A JSON object containing Name and Password.
   - Response: 
     - 200 OK with a message "Registered." if registration is successful.
     - 400 Bad Request if the user already exists or if registration fails.
   - Dependencies:
     - IUserRepository: To check if the user exists and to add the new user.
     - IEncryptionService: To hash the user's password.
     - HttpClient: To notify another service about the new user (e.g., scores service).

2. POST /login
   - Purpose: Log in a user.
   - Request Body: A JSON object containing Name and Password.
   - Response: 
     - 200 OK with a JSON object containing a Token if login is successful.
     - 400 Bad Request if the user is already logged in or if the credentials are invalid.
   - Dependencies:
     - IUserRepository: To get the stored password hash for the user.
     - IEncryptionService: To verify the user's password.
     - IHazelcastService: To manage the session token.

3. POST /logout
   - Purpose: Log out a user.
   - Request Body: A JSON object containing Name and Token.
   - Response: 
     - 200 OK with a message "Logged out." if logout is successful.
     - 400 Bad Request if the user is not logged in or if the token is invalid.
   - Dependencies:
     - IHazelcastService: To verify and delete the session token.

4. GET /isOnline
   - Purpose: Check if a user is online.
   - Query Parameters: name (the user's name).
   - Response: 
     - 200 OK with a message "Online" or "Offline".
   - Dependencies:
     - IHazelcastService: To check the session token.

5. GET /validate
   - Purpose: Validate a user's session token.
   - Query Parameters: name (the user's name) and token (the session token).
   - Response: 
     - 200 OK with a message "Valid" if the token is valid.
     - 400 Bad Request if the token is invalid.
   - Dependencies:
     - IHazelcastService: To validate the session token.


### Controllers and Endpoints
- Main application (Program.cs): Sets up the web server, routes, and DI container.

### Services
- IHazelcastService and HazelcastService: Manages interactions with Hazelcast for session tokens.
- IEncryptionService and EncryptionService: Manages password hashing and verification.

### Repositories
- IUserRepository and UserRepository: Manages user data interactions with the MySQL database.

## Configuration Files
- appsettings.json: Contains configuration settings, including Consul and Hazelcast configurations and the database connection string.
