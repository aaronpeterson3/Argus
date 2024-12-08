CREATE TABLE user_audit_log (
    id SERIAL PRIMARY KEY,
    user_email VARCHAR(255) NOT NULL,
    action VARCHAR(50) NOT NULL,
    details JSONB,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_email) REFERENCES users(email)
);

CREATE INDEX idx_audit_user_email ON user_audit_log(user_email);
CREATE INDEX idx_audit_action ON user_audit_log(action);
CREATE INDEX idx_audit_created_at ON user_audit_log(created_at);