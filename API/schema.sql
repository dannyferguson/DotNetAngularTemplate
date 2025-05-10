-- Used to keep track of the db structure, in a non-migrations fashion for simplicity

-- Users table
CREATE TABLE users
(
    id            INT AUTO_INCREMENT PRIMARY KEY,
    email         VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255)        NOT NULL,
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX idx_users_email_password ON users (email, password_hash);

-- Password Reset Codes table
CREATE TABLE users_password_reset_codes
(
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    code VARCHAR(64) NOT NULL,
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP + INTERVAL 1 HOUR),
    used_at    TIMESTAMP DEFAULT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE -- If user is deleted, delete their reset codes.
);

-- Email Confirmation Codes table
CREATE TABLE users_email_confirmation_codes
(
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    code VARCHAR(64) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP + INTERVAL 24 HOUR),
    confirmed_at TIMESTAMP DEFAULT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE -- If user is deleted, delete their email confirmation codes.
);