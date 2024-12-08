--liquibase formatted sql

--changeset argus:4
ALTER TABLE tenants
ADD COLUMN logo_url VARCHAR(512);

--rollback ALTER TABLE tenants DROP COLUMN logo_url;