--liquibase formatted sql

--changeset argus:2
CREATE TABLE user_tenants (
    user_id UUID NOT NULL,
    tenant_id UUID NOT NULL,
    role VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    invited_by UUID,
    invited_at TIMESTAMP,
    accepted_at TIMESTAMP,
    PRIMARY KEY (user_id, tenant_id),
    FOREIGN KEY (tenant_id) REFERENCES tenants(id)
);

CREATE INDEX idx_user_tenants_tenant ON user_tenants(tenant_id);
CREATE INDEX idx_user_tenants_user ON user_tenants(user_id);

--rollback DROP TABLE user_tenants;