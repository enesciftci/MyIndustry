-- Create databases for MyIndustry
-- This script runs automatically when PostgreSQL container starts

-- Create main database (if not exists)
SELECT 'CREATE DATABASE myindustry'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'myindustry')\gexec

-- Create identity database
SELECT 'CREATE DATABASE myindustry_identity'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'myindustry_identity')\gexec

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE myindustry TO postgres;
GRANT ALL PRIVILEGES ON DATABASE myindustry_identity TO postgres;
