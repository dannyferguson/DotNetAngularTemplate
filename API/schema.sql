-- Users table
CREATE TABLE users
(
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT UTC_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT UTC_TIMESTAMP ON UPDATE UTC_TIMESTAMP
);
CREATE INDEX idx_users_email_password ON users (email, password_hash);

-- Login history table
CREATE TABLE users_login_history
(
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    logged_in TIMESTAMP DEFAULT UTC_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);
CREATE INDEX idx_login_history_user_id ON users_login_history (user_id);

-- Password Reset Codes table
CREATE TABLE users_password_reset_codes
(
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    code VARCHAR(64) NOT NULL,
    created_at TIMESTAMP DEFAULT UTC_TIMESTAMP,
    expires_at DATETIME NOT NULL,
    used_at TIMESTAMP DEFAULT NULL,
    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

-- Email Confirmation Codes table
CREATE TABLE users_email_confirmation_codes
(
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    code VARCHAR(64) NOT NULL,
    created_at TIMESTAMP DEFAULT UTC_TIMESTAMP,
    expires_at DATETIME NOT NULL,
    confirmed_at TIMESTAMP DEFAULT NULL,
    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);
