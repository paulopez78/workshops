DROP DATABASE "meetup";
DROP USER "meetup";

CREATE DATABASE "meetup";
CREATE SCHEMA scheduling;
CREATE USER meetup WITH PASSWORD 'password';

GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;
GRANT ALL PRIVILEGES ON SCHEMA "scheduling" to meetup;

SELECT * FROM "MeetupEvents" INNER JOIN "Attendant" A on "MeetupEvents"."Id" = A."MeetupEventId"

DELETE FROM "Attendant";
DELETE FROM "MeetupEvents";
