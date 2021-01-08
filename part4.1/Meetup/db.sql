DROP DATABASE "meetup";
CREATE DATABASE "meetup";

CREATE SCHEMA scheduling;
CREATE USER meetup WITH PASSWORD 'password';

GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;
GRANT ALL PRIVILEGES ON SCHEMA "scheduling" to meetup;


SELECT * FROM "MeetupEventDetails" 

SELECT * FROM "MeetupEventDetails" M LEFT JOIN "AttendantList" AL on M."Id" = AL."MeetupEventId" LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListId";


SELECT M.\"Id\", M.\"Title\", M.\"Group\", M.\"Capacity\", M.\"Status\", A.\"Id\", A.\"UserId\", A.\"Status\" " 
FROM \"MeetupEvents\" M LEFT JOIN \"Attendant\" A ON M.\"Id\" = A.\"MeetupEventId\

DELETE FROM "Attendant";
DELETE FROM "AttendantList";
DELETE FROM "MeetupEventDetails";
