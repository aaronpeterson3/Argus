CREATE TABLE tenants (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    subdomain VARCHAR(255) NOT NULL UNIQUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    settings JSONB
);

CREATE TABLE user_tenants (
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    role VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    invited_by UUID,
    invited_at TIMESTAMP,
    accepted_at TIMESTAMP,
    PRIMARY KEY (user_id, tenant_id),
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (tenant_id) REFERENCES tenants(id)
);

CREATE INDEX idx_user_tenants_tenant ON user_tenants(tenant_id);
CREATE INDEX idx_user_tenants_user ON user_tenants(user_id);

CREATE TABLE permissions (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT
);

CREATE TABLE role_permissions (
    role_id UUID NOT NULL,
    permission_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    PRIMARY KEY (role_id, permission_id, tenant_id),
    FOREIGN KEY (permission_id) REFERENCES permissions(id),
    FOREIGN KEY (tenant_id) REFERENCES tenants(id)
);

CREATE INDEX idx_role_permissions_tenant ON role_permissions(tenant_id);

-- Function to create tenant schema
CREATE OR REPLACE FUNCTION create_tenant_schema(tenant_id UUID)
RETURNS VOID AS $$
BEGIN
    EXECUTE format('CREATE SCHEMA IF NOT EXISTS tenant_%s', tenant_id);
    
    -- Create tenant-specific tables
    EXECUTE format('
        CREATE TABLE tenant_%s.transactions (
            id UUID PRIMARY KEY,
            external_id VARCHAR(255),
            transaction_date DATE,
            amount DECIMAL(19,4),
            metadata JSONB,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )', tenant_id);
        
    -- Add more tenant-specific tables as needed
END;
$$ LANGUAGE plpgsql;