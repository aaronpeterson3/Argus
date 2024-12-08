--liquibase formatted sql

--changeset argus:5
-- Enable TDE for the database
ALTER DATABASE argus SET ENCRYPTION ON;

-- Create encryption key
CREATE MASTER KEY
    ENCRYPTION BY PASSWORD = '${MASTER_KEY_PASSWORD}';

-- Create certificate
CREATE CERTIFICATE ArgusDbCert
   WITH SUBJECT = 'Database Encryption Certificate';

-- Create database encryption key
CREATE DATABASE ENCRYPTION KEY
   WITH ALGORITHM = AES_256
   ENCRYPTION BY SERVER CERTIFICATE ArgusDbCert;

--rollback ALTER DATABASE argus SET ENCRYPTION OFF;
--rollback DROP DATABASE ENCRYPTION KEY;
--rollback DROP CERTIFICATE ArgusDbCert;
--rollback DROP MASTER KEY;