-- PostgreSQL Database Initialization Script
-- Note: Make sure you are connected to the 'sql-friends' database before running this script

-- Create schemas
CREATE SCHEMA IF NOT EXISTS gstusr;
CREATE SCHEMA IF NOT EXISTS usr;
CREATE SCHEMA IF NOT EXISTS supusr;

SELECT COUNT(*) FROM supusr."Friends";

-- Create views
CREATE OR REPLACE VIEW gstusr."vwInfoDb" AS
    SELECT (SELECT COUNT(*) FROM supusr."Friends" WHERE "Seeded" = true) as "NrSeededFriends", 
        (SELECT COUNT(*) FROM supusr."Friends" WHERE "Seeded" = false) as "NrUnseededFriends",
        (SELECT COUNT(*) FROM supusr."Friends" WHERE "AddressId" IS NOT NULL) as "NrFriendsWithAddress",
        (SELECT COUNT(*) FROM supusr."Addresses" WHERE "Seeded" = true) as "NrSeededAddresses", 
        (SELECT COUNT(*) FROM supusr."Addresses" WHERE "Seeded" = false) as "NrUnseededAddresses",
        (SELECT COUNT(*) FROM supusr."Pets" WHERE "Seeded" = true) as "NrSeededPets", 
        (SELECT COUNT(*) FROM supusr."Pets" WHERE "Seeded" = false) as "NrUnseededPets",
        (SELECT COUNT(*) FROM supusr."Quotes" WHERE "Seeded" = true) as "NrSeededQuotes", 
        (SELECT COUNT(*) FROM supusr."Quotes" WHERE "Seeded" = false) as "NrUnseededQuotes";

CREATE OR REPLACE VIEW gstusr."vwInfoFriends" AS
    SELECT a."Country" as "Country", a."City" as "City", COUNT(*) as "NrFriends" FROM supusr."Friends" f
    INNER JOIN supusr."Addresses" a ON f."AddressId" = a."AddressId"
    GROUP BY ROLLUP(a."Country", a."City");

CREATE OR REPLACE VIEW gstusr."vwInfoPets" AS
    SELECT a."Country" as "Country", a."City" as "City", COUNT(p."PetId") as "NrPets" FROM supusr."Friends" f
    INNER JOIN supusr."Addresses" a ON f."AddressId" = a."AddressId"
    INNER JOIN supusr."Pets" p ON p."FriendId" = f."FriendId"
    GROUP BY ROLLUP(a."Country", a."City");

CREATE OR REPLACE VIEW gstusr."vwInfoQuotes" AS
    SELECT "Author" as "Author", COUNT("QuoteText") as "NrQuotes" FROM supusr."Quotes" 
    GROUP BY "Author";

-- Create the DeleteAll function (PostgreSQL uses functions instead of procedures for this pattern)
CREATE OR REPLACE FUNCTION supusr."spDeleteAll"(
    seededParam BOOLEAN DEFAULT true,
    OUT nrFriendsAffected INTEGER,
    OUT nrAddressesAffected INTEGER,
    OUT nrPetsAffected INTEGER,
    OUT nrQuotesAffected INTEGER
)
RETURNS RECORD
LANGUAGE plpgsql
AS $$
BEGIN
    SELECT COUNT(*) INTO nrFriendsAffected FROM supusr."Friends" WHERE "Seeded" = seededParam;
    SELECT COUNT(*) INTO nrAddressesAffected FROM supusr."Addresses" WHERE "Seeded" = seededParam;
    SELECT COUNT(*) INTO nrPetsAffected FROM supusr."Pets" WHERE "Seeded" = seededParam;
    SELECT COUNT(*) INTO nrQuotesAffected FROM supusr."Quotes" WHERE "Seeded" = seededParam;

    DELETE FROM supusr."Friends" WHERE "Seeded" = seededParam;
    DELETE FROM supusr."Addresses" WHERE "Seeded" = seededParam;
    DELETE FROM supusr."Pets" WHERE "Seeded" = seededParam;
    DELETE FROM supusr."Quotes" WHERE "Seeded" = seededParam;

    -- Test to throw an error (uncomment if needed)
    -- RAISE EXCEPTION 'Error occurred in supusr.spDeleteAll';
END;
$$;


-- User and role management in PostgreSQL
-- Create roles (PostgreSQL roles are both users and groups)
DO $BODY$
BEGIN
    -- Create login roles
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'gstusr') THEN
        CREATE ROLE gstusr WITH LOGIN PASSWORD 'pa$Word1';
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'usr') THEN
        CREATE ROLE usr WITH LOGIN PASSWORD 'pa$Word1';
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'supusr') THEN
        CREATE ROLE supusr WITH LOGIN PASSWORD 'pa$Word1';
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'dbo') THEN
        CREATE ROLE dbo WITH LOGIN PASSWORD 'pa$Word1';
    END IF;
    
    -- Create group roles (note: lowercase names to match PostgreSQL convention)
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'gstusrrole') THEN
        CREATE ROLE gstusrrole;
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'usrrole') THEN
        CREATE ROLE usrrole;
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'supusrrole') THEN
        CREATE ROLE supusrrole;
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'dborole') THEN
        CREATE ROLE dborole;
    END IF;
END
$BODY$;

-- Grant database connection privileges
GRANT CONNECT ON DATABASE "sql-friends" TO gstusr;
GRANT CONNECT ON DATABASE "sql-friends" TO usr;
GRANT CONNECT ON DATABASE "sql-friends" TO supusr;
GRANT CONNECT ON DATABASE "sql-friends" TO dbo;

-- Grant schema usage privileges
GRANT USAGE ON SCHEMA gstusr TO gstusrrole;
GRANT USAGE ON SCHEMA supusr TO gstusrrole;
GRANT USAGE ON SCHEMA public TO gstusrrole;

-- Grant role privileges for gstusrrole
GRANT SELECT ON gstusr."vwInfoDb" TO gstusrrole;
GRANT SELECT ON gstusr."vwInfoFriends" TO gstusrrole;
GRANT SELECT ON gstusr."vwInfoPets" TO gstusrrole;
GRANT SELECT ON gstusr."vwInfoQuotes" TO gstusrrole;
GRANT EXECUTE ON FUNCTION gstusr."spLogin"(VARCHAR, VARCHAR) TO gstusrrole;

-- Grant role privileges for usrrole
GRANT USAGE ON SCHEMA supusr TO usrrole;
GRANT SELECT, UPDATE, INSERT ON ALL TABLES IN SCHEMA supusr TO usrrole;

-- Grant role privileges for supusrrole (inherit from usrrole)
GRANT DELETE ON ALL TABLES IN SCHEMA supusr TO supusrrole;
GRANT EXECUTE ON FUNCTION supusr."spDeleteAll"(BOOLEAN) TO supusrrole;

-- Grant role privileges for dborole (full privileges)
GRANT ALL PRIVILEGES ON DATABASE "sql-friends" TO dborole;
-- Grant superuser-like privileges (alternative: ALTER ROLE dborole SUPERUSER;)
GRANT CREATE ON DATABASE "sql-friends" TO dborole;
GRANT ALL ON ALL TABLES IN SCHEMA gstusr, usr, supusr, dbo, public TO dborole;
GRANT ALL ON ALL SEQUENCES IN SCHEMA gstusr, usr, supusr, dbo, public TO dborole;
GRANT ALL ON ALL FUNCTIONS IN SCHEMA gstusr, usr, supusr, dbo, public TO dborole;
GRANT USAGE, CREATE ON SCHEMA gstusr, usr, supusr, dbo, public TO dborole;

-- Assign users to roles
GRANT gstusrrole TO gstusr;

GRANT gstusrrole TO usr;
GRANT usrrole TO usr;

GRANT gstusrrole TO supusr;
GRANT usrrole TO supusr;
GRANT supusrrole TO supusr;

GRANT dborole TO dbo;

