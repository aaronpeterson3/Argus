--liquibase formatted sql

--changeset argus:3
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

--rollback DROP TABLE role_permissions;
--rollback DROP TABLE permissions;